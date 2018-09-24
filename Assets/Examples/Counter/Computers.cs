using Writership;

namespace Examples.Counter
{
    public static class Computers
    {
        public static void Value(
            IEl<int> target,
            IMultiOp<Empty> inc,
            IMultiOp<Empty> dec
        )
        {
            int delta = inc.Read().Count - dec.Read().Count;
            if (delta != 0) target.Write(target.Read() + delta);
        }
    }
}