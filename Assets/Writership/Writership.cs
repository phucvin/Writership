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
            for (int i = 0, n = Math.Min(a.FrameCount, b.FrameCount); i < n; ++i)
            {
                var af = a.GetFrame(i);
                var bf = b.GetFrame(i);
                if (af.GetMethod() != bf.GetMethod())
                {
                    string assemblyName = af.GetMethod().DeclaringType.Assembly.GetName().Name;
                    // Exeception
                    switch (assemblyName)
                    {
                        case "UnityEngine.UI":
                            return true;
                    }

                    return false;
                }
            }
            return true;
        }
    }
}
