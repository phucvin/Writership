namespace Writership
{
    public static class TestCase01
    {
        public static void Run()
        {
            var e = new MultithreadEngine();
            var name = e.El("jan");
            var age = e.El(1);
            var changeName = e.Op<Empty>();

            e.RegisterComputer(new object[] { name }, () =>
            {
                age.Write(age.Read() + 1);
            });
            e.RegisterComputer(new object[] { changeName }, () =>
            {
                UnityEngine.Debug.Log("changeName count: " + changeName.Read().Count);
                name.Write("new name");
            });
            e.RegisterListener(new object[] { age }, () =>
            {
                UnityEngine.Debug.Log("Age: " + age.Read());
            });

            changeName.Fire(default(Empty));
            changeName.Fire(default(Empty));

            e.Update();
            e.Update();

            e.Dispose();
        }
    }
}
