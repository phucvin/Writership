using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IModifierItem
    {
        Info.IModifier Info { get; }
        IEl<int> Remain { get; }
    }

    public interface IModifierItemFactory
    {
        IModifierItem Create(Info.IModifier info);
        void Dispose(IModifierItem item);
    }

    public class ModifierItem : IModifierItem
    {
        public Info.IModifier Info { get; private set; }
        public IEl<int> Remain { get; private set; }

        public ModifierItem(IEngine engine, Info.IModifier info)
        {
            Info = info;
            Remain = engine.El(info.Duration);
        }

        public void Setup(CompositeDisposable cd, IEngine engine, IWorld world)
        {
            engine.Computer(cd,
                new object[] { world.Ops.Tick },
                () => ComputeRemain(Remain, world.Ops.Tick.Read())
            );
        }

        public static void ComputeRemain(IEl<int> target,
            IList<Ops.Tick> tick)
        {
            int remain = target.Read();
            if (remain == 0) return;

            for (int i = 0, n = tick.Count; i < n; ++i)
            {
                remain -= tick[i].Dt;
            }

            if (remain < 0) remain = 0;

            if (remain != target.Read()) target.Write(remain);
        }

        public class Factory : CompositeDisposableFactory<IModifierItem>, IModifierItemFactory
        {
            private IEngine engine;
            private IWorld world;

            public void Setup(CompositeDisposable cd, IEngine engine, IWorld world)
            {
                this.engine = engine;
                this.world = world;

                cd.Add(this);
            }

            public IModifierItem Create(Info.IModifier info)
            {
                var item = new ModifierItem(engine, info);
                var cd = Add(item);
                item.Setup(cd, engine, world);
                return item;
            }

            public void Dispose(IModifierItem item)
            {
                Remove(item).Dispose();
            }
        }
    }
}