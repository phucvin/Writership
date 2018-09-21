using UnityEngine;
using Writership;

namespace Examples.Http
{
    public class State
    {
        public readonly HttpOp<string, int> HttpUserId;

        public State(IEngine engine)
        {
            HttpUserId = new HttpOp<string, int>(engine,
                "https://api.github.com/users/__USER_NAME__",
                isSingle: false, allowWriters: true
            ).WithUrlTransformer((url, name) => url.Replace("__USER_NAME__", name)
            ).WithResponseParser(json => JsonUtility.FromJson<GitHubUser>(json).id
            );
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            HttpUserId.Setup(cd, engine);
        }

        private struct GitHubUser
        {
            public int id;
        }
    }
}