using System;

namespace Writership
{
    public interface IEngine : IDisposable
    {
        int TotalCells { get; }
        int CurrentCellIndex { get; }
        int MainCellIndex { get; }
        int WorkerCellIndex { get; }
        int WriteCellIndex { get; }
        void MarkDirty(IHaveCells target, bool allowMultiple = false);
        void Listen(int atCellIndex, CompositeDisposable cd, object[] targets, Action job);
        void UnregisterListener(int at, object[] targets, Action job);
        void Update();
    }
}
