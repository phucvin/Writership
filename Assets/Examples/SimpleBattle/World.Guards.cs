using System;
using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public partial class World
    {
#if DEBUG
        private void SetupGuards(CompositeDisposable cd, IEngine engine)
        {
            engine.Guarder(cd,
                new object[] { Ops.Hit },
                () => CheckHit(Ops.Hit.Read())
            );
        }

        public static void CheckHit(IList<Ops.Hit> hit)
        {
            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (!Hitter.CanHit(h.From, h.To, h.From.HitTo.Value))
                {
                    throw new InvalidOperationException("Hit cannot hit");
                }
            }
        }
#endif
    }
}