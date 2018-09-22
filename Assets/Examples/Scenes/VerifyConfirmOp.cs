using System;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class VerifyConfirmOp<T>
    {
        private readonly Func<T, string> messageFormatter;

        public readonly El<bool> Status;
        public readonly Op<T> Trigger;
        public readonly Op<T> Yes;
        public readonly Op<T> No;
        public readonly Op<T> Rejected;

        public readonly Scene Dialog;
        private readonly El<T> current;

        public VerifyConfirmOp(IEngine engine, Func<T, string> messageFormatter, bool allowWriters = false)
        {
            this.messageFormatter = messageFormatter;

            Status = engine.El(false);
            Trigger = engine.Op<T>(allowWriters);
            Yes = engine.Op<T>();
            No = engine.Op<T>();
            Rejected = engine.Op<T>();

            Dialog = new Scene(engine, "YesNoDialog");
            current = engine.El(default(T));
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            Dialog.Setup(cd, engine);

            engine.Worker(cd, Dep.On(Status, Trigger), () =>
            {
                if (Trigger && Status)
                {
                    Dialog.Open.Fire(Empty.Instance);
                }
            });
            engine.Worker(cd, Dep.On(Status, Trigger, Yes, No), () =>
            {
                if (!Status || Yes || No) current.Write(default(T));
                else if (Trigger) current.Write(Trigger.First);
            });
            engine.Worker(cd, Dep.On(Status, Trigger), () =>
            {
                if (Trigger && !Status)
                {
                    Rejected.Fire(Trigger.First);
                }
            });
            engine.Worker(cd, Dep.On(Status, Dialog.State, Yes, No), () =>
            {
                if (!Status && Dialog.State == SceneState.Opened)
                {
                    Dialog.Close.Fire(Empty.Instance);
                }
                else if (Yes || No)
                {
                    Dialog.Close.Fire(Empty.Instance);
                }
            });

            engine.Worker(cd, Dep.On(Status, Yes), () =>
            {
                if (Yes && !Status) throw new InvalidOperationException();
            });
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            Dialog.SetupUnity(cd, engine);

            engine.Mainer(cd, Dep.On(Dialog.Root), () =>
            {
                var root = Dialog.Root.Read();
                if (!root) return;
                var map = root.GetComponent<Common.Map>();
                var scd = root.GetComponent<Common.DisposeOnDestroy>().cd;
                
                map.GetComponent<Text>("message").text = messageFormatter(current);

                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("yes"), Yes,
                    () => current
                );
                Common.Binders.ButtonInteractable(scd, engine,
                    map.GetComponent<Button>("yes"), Status,
                    b => b
                );
                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("no"), No,
                    () => current
                );
            });
        }
    }
}