using System.Collections.Generic;
using System.Linq;
using Grumpy.DacpacMerge.Core.Interfaces;

namespace Grumpy.DacpacMerge.Core
{
    public class DacpacMergeService : IDacpacMergeService
    {
        private readonly IPackageFactory _packageFactory;
        private readonly IModelFactory _modelFactory;

        public DacpacMergeService(IPackageFactory packageFactory, IModelFactory modelFactory)
        {
            _packageFactory = packageFactory;
            _modelFactory = modelFactory;
        }

        public void Merge(string inputDacpacFileName, string databaseSource, string databaseName, IEnumerable<string> databaseSchemas = null, string schemaOwnerUser = null, string outputDacpacFileName = null)
        {
            var schemas = databaseSchemas as string[] ?? databaseSchemas?.ToArray() ?? new string[] { };

            using (var package = _packageFactory.Create(inputDacpacFileName))
            {
                var deploySchemas = schemas.Any() ? schemas.Select(s => s.Trim('[', ']')).ToList() : package.Model.Schemas.ToList();

                using (var databaseModel = _modelFactory.Create(databaseSource, databaseName))
                {
                    var deployObjects = package.Model.GetObjects(deploySchemas, true).ToArray();

                    var deployNames = deployObjects
                        .Union(databaseModel.GetObjects(deploySchemas, false))
                        .Where(o => o.Name.HasName)
                        .Select(o => o.Name.ToString())
                        .Distinct();

                    databaseModel.Remove(deployNames);

                    if (!string.IsNullOrEmpty(schemaOwnerUser) && databaseModel.GetUser(schemaOwnerUser) == null)
                        databaseModel.AddUser(schemaOwnerUser);

                    databaseModel.AddObjects(deployObjects, schemaOwnerUser);

                    package.Model = databaseModel;

                    var fileName = outputDacpacFileName ?? inputDacpacFileName;
                    
                    package.Save(fileName);
                }
            }
        }
    }
}