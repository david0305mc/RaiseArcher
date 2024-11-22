using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace NetworkTest
{
    public class ErrorData
    {
        public int code;
        public JToken message;
        public T GetResult<T>() => message.ToObject<T>();
    }

    public class AlertData
    {
        public int code;
        public JToken message;
        public T GetResult<T>() => message.ToObject<T>();
    }

    public class RequestContext
    {
        public string id;
        public string url;
        public int method;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object @params;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sess;

        [JsonIgnore] public RequestType reqType;

        /// <summary>재전송 디폴트 처리 여부</summary>
        [JsonIgnore] public bool defaultRetryHandling;

        public static RequestContext Create<T>(
            string url,
            int method,
            T data,
            RequestType requestType = RequestType.POST,
            bool defaultRetryHandling = true)
        {
            RequestContext ret = new RequestContext();
            ret.url = url;
            ret.id = Utility.RandomId8Bytes();
            ret.method = method;
            ret.reqType = requestType;
            ret.sess = ServerSetting.sess;
            ret.defaultRetryHandling = defaultRetryHandling;
            ret.@params = data;
            return ret;
        }
    }

    public class ResponseContext
    {
        public string id;
        public JToken result;
        public ErrorData error;
        public AlertData alert;
        //public Protocols.Common.MaintenanceData maintenance;
        public double server_time;
        public T GetResult<T>() => result.ToObject<T>();
    }

}

public class NetworkManager : SingletonMono<NetworkManager>
{
    public enum SENDTYPE
    { 
        GET,
        POST,
        PUT,
        DELETE,
    }

    private static Dictionary<string, string> jsonHeaders = new Dictionary<string, string>() { { "Content-Type", "application/json" } };
    public void StartGetRequest()
    {
        //GetRequest("https://naver.com").Forget();
    }

    public async UniTask<T> GetRequest<T>(string url, CancellationTokenSource cts)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(Utility.URLAntiCacheRandomizer(url)))
        {
            cts.CancelAfterSlim(System.TimeSpan.FromSeconds(5));

            try
            {
                await webRequest.SendWebRequest().WithCancellation(cts.Token);
                //return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(await Get(url, headers, progress, defaultLockHandling, defaultRetryHandling, defaultExceptionHandling, cancellationToken)));
                var utf8String = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                var resContext = JsonConvert.DeserializeObject<T>(utf8String);
                return resContext;
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    Debug.LogError("Timeout");
                    await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cts.Token);
                    // To Do : Retry 

                    return await GetRequest<T>(url, cts);
                }
            }
            catch (Exception e)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cts.Token);
                // To Do : Retry 
                return await GetRequest<T>(url, cts);
            }
            return default;
        }
    }

    public async UniTask<T> SendToServer<T>(NetworkTest.RequestContext data, CancellationTokenSource cts)
    {
        var resContext = await SendToServer(data, cts);
        return resContext.GetResult<T>();
    }
    public async UniTask<NetworkTest.ResponseContext> SendToServer(NetworkTest.RequestContext data, CancellationTokenSource cts) 
    {
        try
        {
            TouchBlockManager.Instance.AddLock();
            if (!IsNeworkReachable())
            {
                Debug.LogError("Unable Nework");
                return default;
            }
            if (cts == default)
                cts = new CancellationTokenSource();
            cts.CancelAfterSlim(System.TimeSpan.FromSeconds(5));

            string id = data.id;
            string jsonData = JsonConvert.SerializeObject(data);
            Encryptor encryptor = ServerSetting.isEncryptServer ? ServerSetting.encryptor : null;
            byte[] rawData = encryptor != null ? Encoding.UTF8.GetBytes(await encryptor.EncryptToStringAsync(jsonData)) : Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest req = new UnityWebRequest(ServerSetting.gameUrl, data.reqType.ToString()))
            {
                foreach (var header in jsonHeaders)
                    req.SetRequestHeader(header.Key, header.Value);

                req.uploadHandler = new UploadHandlerRaw(rawData);
                req.downloadHandler = new DownloadHandlerBuffer();

                try
                {
                    await req.SendWebRequest().WithCancellation(cts.Token);
                    var text = encryptor != null ? await encryptor.DecryptToStringAsync(req.downloadHandler.text) : req.downloadHandler.text;
                    var resContext = JsonConvert.DeserializeObject<NetworkTest.ResponseContext>(text);

                    if (resContext.error != null)
                    {
                        Debug.LogError($"resContext.error {resContext.error.message}");
                    }
                    else if (resContext.alert != null)
                    {
                        Debug.LogError($"resContext.alert {resContext.alert.message}");
                    }

                    return resContext;
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        Debug.LogError("Timeout");
                        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cts.Token);
                        // To Do : Retry Popup

                        return await SendToServer(data, cts);
                    }

                }
                catch (Exception e)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cts.Token);
                    // To Do : Retry Popup
                    return await SendToServer(data, cts);
                }
            }
            return default;
        }
        finally
        {
            TouchBlockManager.Instance.RemoveLock();
        }
    }

    private bool IsNeworkReachable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

}
