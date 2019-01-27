using Grumpy.DacpacMerge.Core.Interfaces;

namespace Grumpy.DacpacMerge.Core
{
    public class DacpacMergeServiceBuilder
    {
        public IDacpacMergeService Build()
        {
            var modelFactory = new ModelFactory();
            var packageFactory = new PackageFactory(modelFactory);

            return new DacpacMergeService(packageFactory, modelFactory);
        }
    }
}