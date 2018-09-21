using UnityEngine;
using Writership;

namespace Examples.Http
{
    public class State
    {
        public readonly El<string> UserName;
        public readonly El<int?> UserId;
        public readonly HttpOp<string, int> HttpUserId;

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
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            HttpUserId.Setup(cd, engine);

            engine.Computer(cd, Dep.On(UserName, HttpUserId.Request, HttpUserId.Error, HttpUserId.Response), () =>
            {
                if (string.IsNullOrEmpty(UserName)) UserId.Write(null);
                else if (HttpUserId.Request) UserId.Write(null);
                else if (HttpUserId.Error) UserId.Write(null);
                else if (HttpUserId.Response) UserId.Write(HttpUserId.Response.First);
            });

            engine.Computer(cd, Dep.On(UserName), () =>
            {
                if (!string.IsNullOrEmpty(UserName)) HttpUserId.Request.Fire(UserName);
            });
        }

        private struct GitHubUser
        {
            public int id;
        }
    }
}