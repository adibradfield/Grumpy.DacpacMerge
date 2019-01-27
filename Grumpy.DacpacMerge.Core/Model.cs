using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Grumpy.DacpacMerge.Core.Extensions;
using Grumpy.DacpacMerge.Core.Interfaces;
using Microsoft.SqlServer.Dac.Model;

namespace Grumpy.DacpacMerge.Core
{
    public class Model : IModel
    {
        private bool _disposed;

        public Model(string source, string name)
        {
            var connectionString = new SqlConnectionStringBuilder
            {
                InitialCatalog = name,
                DataSource = source,
                IntegratedSecurity = true
            }.ConnectionString;

            SqlModel = TSqlModel.LoadFromDatabase(connectionString);
        }

        public Model(TSqlModel model)
        {
            SqlModel = model;
        }

        public TSqlModel SqlModel { get; private set; }

        public void AddObjects(IEnumerable<TSqlObject> objects, string schemaOwnerUser)
        {
            var names = new List<string>(SqlModel.GetObjects(DacQueryScopes.UserDefined).Where(o => o.Name.HasName).Select(o => o.Name.ToString()));

            foreach (var obj in objects)
            {
                if (obj.TryGetScript(out var script))
                {
                    if (obj.IsSchema() && script.Contains("[dbo]") && (schemaOwnerUser ?? "dbo").ToLower(CultureInfo.InvariantCulture) != "dbo")
                        script = script.Replace("[dbo]", $"[{schemaOwnerUser}]");

                    if (!obj.Name.HasName || !names.Contains(obj.Name.ToString()))
                    {
                        SqlModel.AddObjects(script);

                        if (obj.Name.HasName)
                            names.Add(obj.Name.ToString());
                    }
                }
            }
        }

        public void Remove(IEnumerable<string> objectNames)
        {
            var options = SqlModel.CloneModelOptions();
            var newModel = new TSqlModel(SqlModel.Version, options);

            foreach (var obj in SqlModel.GetObjects(DacQueryScopes.UserDefined).Where(o => !o.Name.HasName || !objectNames.Contains(o.Name.ToString())))
            {
                if (obj.TryGetScript(out var script))
                    newModel.AddObjects(script);
            }

            lock (SqlModel)
            {
                SqlModel?.Dispose();
                SqlModel = newModel;
            }
        }

        public IEnumerable<string> Schemas =>
            SqlModel
                .GetObjects(DacQueryScopes.UserDefined)
                .Where(o => o.IsSchema())
                .Select(s => s.Name.ToString().Trim('[', ']'));

        public IEnumerable<TSqlObject> GetObjects(IEnumerable<string> databaseSchemas, bool includeObjectOutsideSchemas = true)
        {
            return SqlModel
                .GetObjects(DacQueryScopes.UserDefined)
                .Where(o => !o.IsDatabaseOptions() && 
                            (includeObjectOutsideSchemas && 
                             string.IsNullOrEmpty(o.SchemaName()) ||
                             databaseSchemas.Contains(o.SchemaName(), StringComparer.InvariantCultureIgnoreCase)));
        }

        public TSqlObject GetUser(string user)
        {
            foreach (var obj in SqlModel.GetObjects(DacQueryScopes.UserDefined).Where(o => o.IsUser()))
            {
                if (obj.TryGetScript(out var script))
                {
                    if (script.ToUpperInvariant().Contains($"[{user.ToUpperInvariant()}]"))
                        return obj;
                }
            }

            return null;
        }

        public void AddUser(string user)
        {
            SqlModel.AddObjects($"CREATE USER [{user}] WITHOUT LOGIN WITH DEFAULT_SCHEMA = [dbo];");
            SqlModel.AddObjects($"GRANT CONNECT TO [{user}];");
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;

                    lock (SqlModel)
                    {
                        SqlModel.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}