namespace Writership
{
    public interface IFireable<T>
    {
        void Fire(T value);
    }
}
