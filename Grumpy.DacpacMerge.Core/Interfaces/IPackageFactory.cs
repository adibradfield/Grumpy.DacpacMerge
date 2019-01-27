namespace Grumpy.DacpacMerge.Core.Interfaces
{
    public interface IPackageFactory
    {
        IPackage Create(string fileName);
    }
}