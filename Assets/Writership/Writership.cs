using System;
using System.Diagnostics;

namespace Writership
{
    internal class Writership
    {
        private StackTrace last;

        public bool TryMark()
        {
            StackTrace now;
            return TryMark(out now);
        }

        public void Mark()
        {
            StackTrace now;
            if (!TryMark(out now))
            {
                UnityEngine.Debug.LogWarning("Last write: \n" + last.ToString());
                UnityEngine.Debug.LogWarning("Now write: \n" + now.ToString());
                throw new InvalidOperationException("Cannot write to same at different places");
            }
        }

        private bool TryMark(out StackTrace now)
        {
            now = new StackTrace(2, true);

            if (last == null) last = now;
            else if (!IsSame(last, now)) return false;

            return true;
        }

        private static bool IsSame(StackTrace a, StackTrace b)
        {
            for (int i = 0, n = Math.Min(a.FrameCount, b.FrameCount); i < n; ++i)
            {
                var af = a.GetFrame(i);
                var bf = b.GetFrame(i);
                if (af.GetMethod() != bf.GetMethod())
                {
                    string assemblyNameA = af.GetMethod().DeclaringType.Assembly.GetName().Name;
                    string assemblyNameB = bf.GetMethod().DeclaringType.Assembly.GetName().Name;
                    if (assemblyNameA == assemblyNameB)
                    {
                        // Exeception
                        switch (assemblyNameA)
                        {
                            case "UnityEngine.UI":
                                return true;
                        }
                    }
                    else return true;

                    return false;
                }
            }
            return true;
        }
    }
}
