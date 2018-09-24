using System;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class VerifyConfirmOp<T>
    {
        private readonly Func<T, string> messageFormatter;

        public readonly El<bool> Status;
        public readonly El<T> Current;
        public readonly Scene<Empty> Dialog;
        public readonly Op<T> Trigger;
        public readonly Op<T> Yes;
        public readonly Op<T> No;
        public readonly Op<T> Rejected;

        public VerifyConfirmOp(IEngine engine, Func<T, string> messageFormatter, bool allowWriters = false)
        {
            this.messageFormatter = messageFormatter;

            Status = engine.El(false);
            Current = engine.El(default(T));
            Dialog = new Scene<Empty>(engine, "YesNoDialog", backAutoClose: false);
            Trigger = engine.Op<T>(allowWriters);
            Yes = engine.Op<T>();
            No = engine.Op<T>();
            Rejected = engine.Op<T>();
        }

        public void Setup(CompositeDisposable cd, IEngine engine, SceneStack sceneStack)
        {
            Dialog.Setup(cd, engine, sceneStack);

            engine.Worker(cd, Dep.On(Status, Trigger), () =>
            {
                if (Trigger && Status)
                {
                    Dialog.Open.Fire(Empty.Instance);
                }
            });
            engine.Worker(cd, Dep.On(Status, Trigger, Yes, No), () =>
            {
                if (!Status || Yes || No) Current.Write(default(T));
                else if (Trigger) Current.Write(Trigger.First);
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
                
                map.GetComponent<Text>("message").text = messageFormatter(Current);

                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("yes"), Yes,
                    () => Current
                );
                Common.Binders.ButtonInteractable(scd, engine,
                    map.GetComponent<Button>("yes"), Status,
                    b => b
                );
                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("no"), No,
                    () => Current
                );
                Common.Binders.Click(scd, engine,
                    map.GetComponent<Common.Clickable>("back"), Dialog.Back,
                    () => false
                );
            });
        }
    }
}