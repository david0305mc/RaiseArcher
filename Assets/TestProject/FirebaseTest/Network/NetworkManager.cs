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
        public int method;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object @params;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sess;

        [JsonIgnore] public RequestType reqType;

        /// <summary>재전송 디폴트 처리 여부</summary>
        [JsonIgnore] public bool defaultRetryHandling;


        public static RequestContext Create(int method,
            RequestType requestType = RequestType.POST,
            bool defaultRetryHandling = true)
        {
            RequestContext ret = new RequestContext();
            ret.id = Utility.RandomId8Bytes();
            ret.method = method;
            ret.@params = null;
            ret.reqType = requestType;
            ret.sess = ServerSetting.sess;
            ret.defaultRetryHandling = defaultRetryHandling;
            return ret;
        }

        public static RequestContext Create<T>(int method,
            T data,
            RequestType requestType = RequestType.POST,
            bool defaultRetryHandling = true)
        {
            var ret = Create(method, requestType, defaultRetryHandling);
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
        GetRequest("https://naver.com").Forget();
    }

    private async UniTask GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // 요청 보내기 및 응답 대기
            await webRequest.SendWebRequest();

            // 결과 처리
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                Debug.Log($"Response: {webRequest.downloadHandler.text}");
            }
        }
    }

    public void StartPostRequest()
    {
        //SendToServer("www.naver.com", SENDTYPE.POST, string.Empty).Forget();
    }

    public async UniTask<T> SendToServer<T>(NetworkTest.RequestContext data, CancellationToken cancellationToken = default) 
    {
        try
        {
            TouchBlockManager.Instance.AddLock();
            if (!IsNeworkReachable())
            {
                Debug.LogError("Unable Nework");
                return default;
            }
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(System.TimeSpan.FromSeconds(5));

            string id = data.id;
            string jsonData = JsonConvert.SerializeObject(data);
            Encryptor encryptor = ServerSetting.isEncryptServer ? ServerSetting.encryptor : null;
            byte[] rawData = encryptor != null ? Encoding.UTF8.GetBytes(await encryptor.EncryptToStringAsync(jsonData)) : Encoding.UTF8.GetBytes(jsonData);


            using (UnityWebRequest req = new UnityWebRequest(ServerSetting.gameUrl, UnityWebRequest.kHttpVerbPOST))
            {
                foreach (var header in jsonHeaders)
                    req.SetRequestHeader(header.Key, header.Value);

                req.uploadHandler = new UploadHandlerRaw(rawData);
                req.downloadHandler = new DownloadHandlerBuffer();

                try
                {
                    var res = await req.SendWebRequest();
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

                    return resContext.GetResult<T>();
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        Debug.Log("Timeout");
                        await UniTask.Delay(TimeSpan.FromSeconds(1f));

                        return await SendToServer<T>(data, cancellationToken);
                    }

                }
                catch (Exception e)
                {
                    return default;
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
