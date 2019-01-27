using Microsoft.SqlServer.Dac.Model;

namespace Grumpy.DacpacMerge.Core.Interfaces
{
    public interface IModelFactory
    {
        IModel Create(string source, string name);
        IModel Create(TSqlModel model);
    }
}