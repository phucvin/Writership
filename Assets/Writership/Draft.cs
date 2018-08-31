namespace Writership
{
    public class Engine
    {
        public El<T> El<T>(T value)
        {
            return new El<T>(this, value);
        }

        public Op<T> Op<T>()
        {
            return new Op<T>(this);
        }
    }

    public class El<T>
    {
        public El(Engine engine, T value)
        {

        }
    }

    public class Op<T>
    {
        public Op(Engine engine)
        {

        }
    }
}
