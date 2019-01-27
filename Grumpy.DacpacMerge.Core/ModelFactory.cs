using Grumpy.DacpacMerge.Core.Interfaces;
using Microsoft.SqlServer.Dac.Model;

namespace Grumpy.DacpacMerge.Core
{
    public class ModelFactory : IModelFactory
    {
        public IModel Create(string source, string name)
        {
            return new Model(source, name);
        }

        public IModel Create(TSqlModel model)
        {
            return new Model(model);
        }
    }
}