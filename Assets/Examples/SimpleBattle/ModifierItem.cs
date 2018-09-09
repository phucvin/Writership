﻿using System;
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
    }

    public class ModifierItem : Disposable, IModifierItem
    {
        public Info.IModifier Info { get; private set; }
        public IEl<int> Remain { get; private set; }

        public ModifierItem(IEngine engine, Info.IModifier info)
        {
            Info = info;
            Remain = engine.El(info.Duration);
        }

        public void Setup(IEngine engine, IWorld world)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { world.Ops.Tick },
                () => ComputeRemain(Remain, world.Ops.Tick.Read())
            ));
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

        public class Factory : IModifierItemFactory, IDisposable
        {
            private IEngine engine;
            private IWorld world;

            public void Setup(IEngine engine, IWorld world)
            {
                this.engine = engine;
                this.world = world;
            }

            public void Dispose()
            {
            }

            public IModifierItem Create(Info.IModifier info)
            {
                var item = new ModifierItem(engine, info);
                item.Setup(engine, world);
                return item;
            }
        }
    }
}