using System;
using System.Diagnostics;

namespace Writership
{
    internal class Writership
    {
        private StackTrace last;

        public void Mark()
        {
            var now = new StackTrace(2, true);

            if (last == null)
            {
                last = now;
            }
            else if (!IsSame(last, now))
            {
                UnityEngine.Debug.LogWarning("Last write: \n" + last.ToString());
                UnityEngine.Debug.LogWarning("Now write: \n" + now.ToString());
                throw new InvalidOperationException("Cannot write to same at different places");
            }
        }

        private static bool IsSame(StackTrace a, StackTrace b)
        {
            if (a.FrameCount != b.FrameCount) return false;
            for (int i = 0, n = a.FrameCount; i < n; ++i)
            {
                var af = a.GetFrame(i);
                var bf = b.GetFrame(i);
                if (af.GetMethod() != bf.GetMethod())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
