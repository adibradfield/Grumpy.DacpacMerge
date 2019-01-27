using Grumpy.DacpacMerge.Core.Interfaces;

namespace Grumpy.DacpacMerge.Core
{
    public class PackageFactory : IPackageFactory
    {
        private readonly IModelFactory _modelFactory;

        public PackageFactory(IModelFactory modelFactory)
        {
            _modelFactory = modelFactory;
        }

        public IPackage Create(string fileName)
        {
            return new Package(_modelFactory, fileName);
        }
    }
}