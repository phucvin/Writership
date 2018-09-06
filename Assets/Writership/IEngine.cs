using System;

namespace Writership
{
    public interface IEngine : IDisposable
    {
        int TotalCells { get; }
        int CurrentCellIndex { get; }
        int WriteCellIndex { get; }
        void MarkDirty(IHaveCells target, bool allowMultiple = false);
        IDisposable RegisterListener(object[] targets, Action job);
        IDisposable RegisterComputer(object[] targets, Action job);
        void UnregisterListener(int at, object[] targets, Action job);
        void Update();
    }
}
