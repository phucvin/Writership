namespace Writership
{
    public interface IWriteable<T>
    {
        void Write(T value);
    }
}
