using System;
using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public partial class World : Disposable
    {
#if DEBUG
        private void SetupGuards(IEngine engine)
        {
            cd.Add(engine.RegisterListener(
                new object[] { actions.hit },
                () => CheckHit(actions.hit.Read())
            ));
        }

        public static void CheckHit(IList<Actions.Hit> hit)
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