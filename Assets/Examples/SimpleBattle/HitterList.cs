using Writership;

namespace Examples.SimpleBattle
{
    public interface IHitterList
    {
        IDamageHitter Damage { get; }
        IAddModifierHitter AddModifier { get; }
    }

    public class HitterList : Disposable, IHitterList
    {
        public IDamageHitter Damage { get; private set; }
        public IAddModifierHitter AddModifier { get; private set; }

        private HitterList() { }

        public HitterList(IEngine engine, Info.HitterList info)
        {
            if (info.Damage.HasValue)
            {
                Damage = cd.Add(new DamageHitter(engine, info.Damage.Value));
            }
            if (info.AddModifier.HasValue)
            {
                AddModifier = cd.Add(new AddModifierHitter(info.AddModifier.Value));
            }
        }

        public void Setup(IEngine engine, IEntity entity)
        {
            if (Damage != null)
            {
                ((DamageHitter)Damage).Setup(engine, entity);
            }
            else if (AddModifier != null)
            {
                ((AddModifierHitter)AddModifier).Setup();
            }
        }

        public HitterList Instantiate(IEngine engine)
        {
            var l = new HitterList();
            if (Damage != null)
            {
                l.Damage = l.cd.Add(((DamageHitter)Damage).Instantiate(engine));
            }
            else if (AddModifier != null)
            {
                l.AddModifier = l.cd.Add(((AddModifierHitter)AddModifier).Instantiate());
            }
            return l;
        }
    }
}