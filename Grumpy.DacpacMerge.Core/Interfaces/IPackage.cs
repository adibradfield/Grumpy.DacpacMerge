using System;

namespace Grumpy.DacpacMerge.Core.Interfaces
{
    public interface IPackage : IDisposable
    {
        IModel Model { get; set; }
        void Save(string fileName);
    }
}