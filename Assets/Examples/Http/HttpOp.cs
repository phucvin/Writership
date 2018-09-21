using System;
using System.Collections;
using UnityEngine.Networking;
using Writership;

namespace Examples.Http
{
    public struct HttpError
    {
        public bool DidSent;
        public long StatusCode;
        public bool IsSuccess;
    }

    public class HttpOp<TReq, TRes>
    {
        private readonly bool isSingle;
        private readonly string url;

        public readonly Op<TReq> Request;
        public readonly Op<TRes> Response;
        public readonly Op<HttpError> Error;

        private readonly El<int> requesting;

        public HttpOp(IEngine engine, string url, bool isSingle = false, bool allowWriters = false)
        {
            this.isSingle = isSingle;
            this.url = url;

            Request = engine.Op<TReq>(allowWriters);
            Response = engine.Op<TRes>();
            Error = engine.Op<HttpError>();
            requesting = engine.El(0);
        }

        private Func<string, TReq, string> urlTransformer = null;
        public HttpOp<TReq, TRes> WithUrlTransformer(Func<string, TReq, string> f)
        {
            urlTransformer = f;
            return this;
        }

        private Func<string, TRes> responseParser = null;
        public HttpOp<TReq, TRes> WithResponseParser(Func<string, TRes> f)
        {
            responseParser = f;
            return this;
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            engine.Worker(cd, Dep.On(Request, Response, Error), () =>
            {
                int result = requesting + (isSingle ? (requesting > 0 ? 0 : Math.Min(1, Request.Count)) : Request.Count) - Response.Count - Error.Count;
                if (result < 0) throw new NotImplementedException();
                requesting.Write(result);
            });

            engine.Mainer(cd, Dep.On(Request), () =>
            {
                for (int i = 0, n = Request.Count; i < n; ++i)
                {
                    if ((requesting > 0 || i > 0) && isSingle)
                    {
                        UnityEngine.Debug.LogWarning("Skip a HTTP request, because of single on: " + url);
                        UnityEngine.Debug.LogWarning("Request: " + Request[i]);
                    }
                    else HttpMain.Instance.StartCoroutine(Exec(Request[i]));
                }
            });
        }

        private IEnumerator Exec(TReq req)
        {
            string url = this.url;
            if (urlTransformer != null) url = urlTransformer(url, req);

            var wr = UnityWebRequest.Get(url);
            yield return wr.SendWebRequest();

            if (wr.isNetworkError || wr.isHttpError)
            {
                Error.Fire(new HttpError
                {
                    DidSent = !wr.isNetworkError,
                    StatusCode = wr.responseCode,
                    IsSuccess = false,
                });
            }
            else
            {
                var res = default(TRes);
                if (responseParser != null) res = responseParser(wr.downloadHandler.text);
                Response.Fire(res);
            }
        }
    }
}