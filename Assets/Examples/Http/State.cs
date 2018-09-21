using UnityEngine;
using Writership;

namespace Examples.Http
{
    public class State
    {
        public readonly El<string> UserName;
        public readonly El<int?> UserId;
        public readonly HttpOp<string, int> HttpUserId;

        public readonly El<bool> IsBusy;

        public State(IEngine engine)
        {
            UserName = engine.El(string.Empty);
            UserId = engine.El<int?>(null);
            HttpUserId = new HttpOp<string, int>(engine,
                "https://api.github.com/users/__USER_NAME__",
                pipe: HttpPipe.SingleLast
            ).WithUrlTransformer((url, name) => url.Replace("__USER_NAME__", name)
            ).WithResponseParser(json => JsonUtility.FromJson<GitHubUser>(json).id
            );

            IsBusy = engine.El(false);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            HttpUserId.Setup(cd, engine);

            engine.Worker(cd, Dep.On(UserName, HttpUserId.Request, HttpUserId.Error, HttpUserId.Response), () =>
            {
                if (string.IsNullOrEmpty(UserName) || HttpUserId.Request || HttpUserId.Error)
                {
                    UserId.Write(null);
                }
                else if (HttpUserId.Response)
                {
                    UserId.Write(HttpUserId.Response.First);
                }
            });
            engine.Worker(cd, Dep.On(UserName), () =>
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    HttpUserId.Request.Fire(UserName);
                }
            });
            engine.Worker(cd, Dep.On(HttpUserId.Requesting), () =>
            {
                IsBusy.Write(HttpUserId.Requesting > 0);
            });
        }

        private struct GitHubUser
        {
            public int id;
        }
    }
}