using Writership;

namespace Examples.Counter
{
    public static class Computers
    {
        public static void Value(El<int> value, Op<Empty> inc, Op<Empty> dec)
        {
            value.Write(value.Read() + inc.Read().Count - dec.Read().Count);
        }
    }
}