using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IModifierList
    {
        ILi<IModifierItem> Items { get; }
    }

    public class ModifierList : Disposable, IModifierList
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

        public void Setup(IEngine engine, IEntity me,
            IOp<int> tick, IOp<World.Actions.Hit> hit,
            IModifierItemFactory itemFactory)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { tick },
                () => ComputeList(Items, me,
                    tick.Read(), hit.Read(), itemFactory)
            ));
        }

        public static void ComputeList(ILi<IModifierItem> target,
            IEntity me, IList<int> tick, IList<World.Actions.Hit> hit,
            IModifierItemFactory itemFactory)
        {
            if (tick.Count <= 0) return;

            var items = target.AsWrite();

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.To != me) continue;

                var hitters = h.From.Hitters.Items;
                for (int j = 0, m = hitters.Count; j < m; ++j)
                {
                    var a = hitters[j] as IAddModifierHitter;
                    if (a == null) continue;

                    items.Add(itemFactory.Create(a.Modifier));
                }
            }

            if (tick.Count > 0)
            {
                items.RemoveAll(it => it.Remain.Read() == 0);
            }
        }
    }
}