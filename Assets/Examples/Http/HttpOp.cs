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

    public enum HttpPipe
    {
        Multiple,
        SingleFirst,
        SingleLast
    }

    public static class HttpPipeExtensions
    {
        public static bool IsSingle(this HttpPipe pipe)
        {
            switch (pipe)
            {
                case HttpPipe.SingleFirst:
                case HttpPipe.SingleLast:
                    return true;

                default:
                    return false;
            }
        }
    }

    public class HttpOp<TReq, TRes>
    {
        private readonly IEngine engine;
        private readonly HttpPipe pipe;
        private readonly string url;

        public readonly MultiOp<TReq> Request;
        public readonly MultiOp<TRes> Response;
        public readonly MultiOp<HttpError> Error;

        public readonly El<int> Requesting;

        private MultiOp<string> rawResponse;
        private UnityEngine.Coroutine lastExec;

        public HttpOp(IEngine engine, string url,
            HttpPipe pipe = HttpPipe.Multiple, bool allowWriters = false)
        {
            this.engine = engine;
            this.pipe = pipe;
            this.url = url;

            Request = engine.MultiOp<TReq>(allowWriters);
            Response = engine.MultiOp<TRes>();
            Error = engine.MultiOp<HttpError>();
            Requesting = engine.El(0);
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
        public HttpOp<TReq, TRes> WithWorkerResponseParser(Func<string, TRes> f)
        {
            responseParser = f;
            rawResponse = engine.MultiOp<string>();
            return this;
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            if (rawResponse != null)
            {
                engine.Worker(cd, Dep.On(rawResponse), () =>
                {
                    for (int i = 0, n = rawResponse.Count; i < n; ++i)
                    {
                        var res = default(TRes);
                        if (responseParser != null) res = responseParser(rawResponse[i]);
                        Response.Fire(res);
                    }
                });
            }
            engine.Worker(cd, Dep.On(Request, Response, Error), () =>
            {
                int result = Requesting - Response.Count - Error.Count;
                switch (pipe)
                {
                    case HttpPipe.Multiple:
                        result += Request.Count;
                        break;

                    case HttpPipe.SingleFirst:
                    case HttpPipe.SingleLast:
                        result += Requesting > 0 ? 0 : Math.Min(1, Request.Count);
                        break;
                }
                if (result < 0) throw new NotImplementedException();
                else if (pipe.IsSingle() && result > 1) throw new NotImplementedException();
                Requesting.Write(result);
            });

            engine.Mainer(cd, Dep.On(Request), () =>
            {
                for (int i = 0, n = Request.Count; i < n; ++i)
                {
                    if (pipe.IsSingle() && (
                        (pipe == HttpPipe.SingleFirst && (i > 0 || Requesting > 0)) ||
                        (pipe == HttpPipe.SingleLast && i < n - 1)))
                    {
                        UnityEngine.Debug.LogWarning("Skip a HTTP request, because of single on: " + url);
                        UnityEngine.Debug.LogWarning("Request: " + Request[i]);
                    }
                    else
                    {
                        if (pipe == HttpPipe.SingleLast && Requesting > 0)
                        {
                            UnityEngine.Debug.LogWarning("Stop previous HTTP request, because of single on: " + url);
                            UnityEngine.Debug.LogWarning("New request: " + Request[i]);
                            HttpMain.Instance.StopCoroutine(lastExec);
                        }
                        lastExec = HttpMain.Instance.StartCoroutine(Exec(Request[i]));
                    }
                }
            });
        }

        private IEnumerator Exec(TReq req)
        {
            yield return new UnityEngine.WaitForSecondsRealtime(0.2f);

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
            else if (rawResponse != null)
            {
                rawResponse.Fire(wr.downloadHandler.text);
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