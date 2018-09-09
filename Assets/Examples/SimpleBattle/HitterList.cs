using Writership;

namespace Examples.SimpleBattle
{
    public interface IHitterList
    {
        IDamageHitter Damage { get; }
        IAddModifierHitter AddModifier { get; }
    }

    public class HitterList : IHitterList
    {
        public IDamageHitter Damage { get; private set; }
        public IAddModifierHitter AddModifier { get; private set; }

        private HitterList() { }

        public HitterList(IEngine engine, Info.HitterList info)
        {
            if (info.Damage.HasValue)
            {
                Damage = new DamageHitter(engine, info.Damage.Value);
            }
            if (info.AddModifier.HasValue)
            {
                AddModifier = new AddModifierHitter(info.AddModifier.Value);
            }
        }

        public void Setup(CompositeDisposable cd, IEngine engine, IEntity entity)
        {
            if (Damage != null)
            {
                ((DamageHitter)Damage).Setup(cd, engine, entity);
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
                l.Damage = ((DamageHitter)Damage).Instantiate(engine);
            }
            else if (AddModifier != null)
            {
                l.AddModifier = ((AddModifierHitter)AddModifier).Instantiate();
            }
            return l;
        }
    }
}