using System.Collections.Generic;

namespace Examples.Counter
{
    public static class Computers
    {
        public static int Value(
            int value,
            IList<Empty> opInc,
            IList<Empty> opDec
        )
        {
            value += opInc.Count - opDec.Count;
            if (value <= 0) value = 1;
            return value;
        }
    }
}