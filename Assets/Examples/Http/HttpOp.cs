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

        public readonly Op<TReq> Request;
        public readonly Op<TRes> Response;
        public readonly Op<HttpError> Error;

        public readonly El<int> Requesting;

        private Op<string> rawResponse;
        private UnityEngine.Coroutine lastExec;

        public HttpOp(IEngine engine, string url,
            HttpPipe pipe = HttpPipe.Multiple, bool allowWriters = false)
        {
            this.engine = engine;
            this.pipe = pipe;
            this.url = url;

            Request = engine.Op<TReq>(allowWriters);
            Response = engine.Op<TRes>();
            Error = engine.Op<HttpError>();
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
            rawResponse = engine.Op<string>();
            return this;
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            if (rawResponse != null)
            {
                engine.Worker(cd, Dep.On(rawResponse), () =>
                {
                    string raw;
                    if (rawResponse.TryRead(out raw))
                    {
                        var res = default(TRes);
                        if (responseParser != null) res = responseParser(raw);
                        Response.Fire(res);
                    }
                });
            }
            engine.Worker(cd, Dep.On(Request, Response, Error), () =>
            {
                int result = Requesting - (Response ? 1 : 0) - (Error ? 1 : 0);
                switch (pipe)
                {
                    case HttpPipe.Multiple:
                        result += Request ? 1 : 0;
                        break;

                    case HttpPipe.SingleFirst:
                    case HttpPipe.SingleLast:
                        result += Requesting > 0 ? 0 : Math.Min(1, Request ? 1 : 0);
                        break;
                }
                if (result < 0) throw new NotImplementedException();
                else if (pipe.IsSingle() && result > 1) throw new NotImplementedException();
                Requesting.Write(result);
            });

            engine.Mainer(cd, Dep.On(Request), () =>
            {
                TReq req;
                if (Request.TryRead(out req))
                {
                    if (pipe == HttpPipe.SingleFirst && Requesting > 0)
                    {
                        UnityEngine.Debug.LogWarning("Skip a HTTP request, because of single on: " + url);
                        UnityEngine.Debug.LogWarning("Request: " + req);
                    }
                    else
                    {
                        if (pipe == HttpPipe.SingleLast && Requesting > 0)
                        {
                            UnityEngine.Debug.LogWarning("Stop previous HTTP request, because of single on: " + url);
                            UnityEngine.Debug.LogWarning("New request: " + req);
                            HttpMain.Instance.StopCoroutine(lastExec);
                        }
                        lastExec = HttpMain.Instance.StartCoroutine(Exec(req));
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