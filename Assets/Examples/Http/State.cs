﻿using System;
using UnityEngine;
using Writership;

namespace Examples.Http
{
    public class State
    {
        public readonly El<string> UserName;
        public readonly El<int?> UserId;
        public readonly El<int?> RepoCount;
        public readonly HttpOp<string, int> HttpUserId;
        public readonly HttpOp<string, int?> HttpRepoCount;

        public readonly El<bool> IsBusy;

        public State(IEngine engine)
        {
            UserName = engine.El(string.Empty);
            UserId = engine.El<int?>(null);
            RepoCount = engine.El<int?>(null);
            HttpUserId = new HttpOp<string, int>(engine,
                "https://api.github.com/users/__USER_NAME__",
                pipe: HttpPipe.SingleLast
            ).WithUrlTransformer((url, name) => url.Replace("__USER_NAME__", name)
            ).WithResponseParser(json => {
                UnityEngine.Profiling.Profiler.BeginSample("JsonUtility");
                var user = JsonUtility.FromJson<GitHubUser>(json);
                UnityEngine.Profiling.Profiler.EndSample();
                return user.id;
            });
            HttpRepoCount = new HttpOp<string, int?>(engine,
                "https://api.github.com/users/__USER_NAME__/repos",
                pipe: HttpPipe.SingleLast
            ).WithUrlTransformer((url, name) => url.Replace("__USER_NAME__", name)
            ).WithWorkerResponseParser(json => {
                var arr = JsonHelper.GetJsonArray<GitHubRepo>(json);
                return arr != null ? arr.Length : (int?)null;
            });

            IsBusy = engine.El(false);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            HttpUserId.Setup(cd, engine);
            HttpRepoCount.Setup(cd, engine);

            engine.Worker(cd, Dep.On(UserName, HttpUserId.Request, HttpUserId.Error, HttpUserId.Response), () =>
            {
                int resUserId;
                if (string.IsNullOrEmpty(UserName) || HttpUserId.Request || HttpUserId.Error)
                {
                    UserId.Write(null);
                }
                else if (HttpUserId.Response.TryRead(out resUserId))
                {
                    UserId.Write(resUserId);
                }
            });
            engine.Worker(cd, Dep.On(UserName, HttpRepoCount.Request, HttpRepoCount.Error, HttpRepoCount.Response), () =>
            {
                int? resRepoCount;
                if (string.IsNullOrEmpty(UserName) || HttpRepoCount.Request || HttpRepoCount.Error)
                {
                    RepoCount.Write(null);
                }
                else if (HttpRepoCount.Response.TryRead(out resRepoCount))
                {
                    RepoCount.Write(resRepoCount);
                }
            });
            engine.Worker(cd, Dep.On(UserName), () =>
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    HttpUserId.Request.Fire(UserName);
                    HttpRepoCount.Request.Fire(UserName);
                }
            });
            engine.Worker(cd, Dep.On(HttpUserId.Requesting, HttpRepoCount.Requesting), () =>
            {
                IsBusy.Write(HttpUserId.Requesting > 0 || HttpRepoCount.Requesting > 0);
            });
        }

        [Serializable]
        private struct GitHubUser
        {
            public int id;
        }

        [Serializable]
        private struct GitHubRepo
        {
            public int id;
        }

        private class JsonHelper
        {
            public static T[] GetJsonArray<T>(string json)
            {
                UnityEngine.Profiling.Profiler.BeginSample("GetJsonArray");
                string newJson = "{\"array\":" + json + "}";
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
                UnityEngine.Profiling.Profiler.EndSample();
                return wrapper.array;
            }

            [Serializable]
            private class Wrapper<T>
            {
                public T[] array;
            }
        }
    }
}