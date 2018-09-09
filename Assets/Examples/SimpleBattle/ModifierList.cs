using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IModifierList
    {
        ILi<IModifierItem> Items { get; }
    }

    public class ModifierList : IModifierList
    {
        public ILi<IModifierItem> Items { get; private set; }

        public ModifierList(IEngine engine, IList<Info.IModifier> info)
        {
            var items = new List<IModifierItem>();
            for (int i = 0, n = info != null ? info.Count : 0; i < n; ++i)
            {
                items.Add(new ModifierItem(engine, info[i]));
            }
            Items = engine.Li(items);
        }

        public void Setup(CompositeDisposable cd, IEngine engine, IEntity entity, IWorld world)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { world.Ops.Tick, world.Ops.Hit },
                () => ComputeList(Items, entity,
                    world.Ops.Tick.Read(), world.Ops.Hit.Read(),
                    world.ModifierItemFactory)
            ));
        }

        public static void ComputeList(ILi<IModifierItem> target,
            IEntity entity, IList<Ops.Tick> tick, IList<Ops.Hit> hit,
            IModifierItemFactory itemFactory)
        {
            if (tick.Count <= 0) return;

            var items = target.AsWrite();

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.To != entity) continue;

                var a = h.From.Hitters.AddModifier;
                if (a == null) continue;

                for (int j = 0, m = a.Modifiers.Count; j < m; ++j)
                {
                    items.Add(itemFactory.Create(a.Modifiers[j]));
                }
            }

            if (tick.Count > 0)
            {
                items.RemoveAll(it =>
                {
                    if (it.Remain.Read() == 0)
                    {
                        itemFactory.Dispose(it);
                        return true;
                    }
                    return false;
                });
            }
        }
    }
}