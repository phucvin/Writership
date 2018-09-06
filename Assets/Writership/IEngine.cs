﻿using System;

namespace Writership
{
    // TODO Iimplement Engine and DualEngine separately
    // (no need preprocessor WRITERSHIP_NO_COMPUTE_THREAD)
    public interface IEngine
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
