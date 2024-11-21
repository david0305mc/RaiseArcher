using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

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

    public async UniTask<T> SendToServer<T>(string url, SENDTYPE sendType,  string jsonData)
    {
        if (!IsNeworkReachable())
        {
            Debug.LogError("Unable Nework");
            return default;
        }
        var cts = new CancellationTokenSource();
        cts.CancelAfterSlim(System.TimeSpan.FromSeconds(5));

        using (UnityWebRequest req = new UnityWebRequest(ServerSetting.gameUrl, sendType.ToString()))
        {
            foreach (var header in jsonHeaders)
                req.SetRequestHeader(header.Key, header.Value);

            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            req.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                var res = await req.SendWebRequest();
                T result = JsonUtility.FromJson<T>(res.downloadHandler.text);
                return result;
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    Debug.Log("Timeout");
                    await UniTask.Delay(TimeSpan.FromSeconds(1f));

                    return await SendToServer<T>(url, sendType, jsonData);
                }
                    
            }
            catch (Exception e)
            {

                return default;
            }


            return default;

        }
        //using (UnityWebRequest webRequest = UnityWebRequest.Post(url, ""))
        //{
        //    // JSON 데이터 설정
        //    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        //    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //    webRequest.downloadHandler = new DownloadHandlerBuffer();
        //    webRequest.SetRequestHeader("Content-Type", "application/json");

        //    // 요청 보내기 및 응답 대기
        //    await webRequest.SendWebRequest();

        //    // 결과 처리
        //    if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
        //        webRequest.result == UnityWebRequest.Result.ProtocolError)
        //    {
        //        Debug.LogError($"Error: {webRequest.error}");
        //    }
        //    else
        //    {
        //        Debug.Log($"Response: {webRequest.downloadHandler.text}");
        //    }
        //}
    }

    private bool IsNeworkReachable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

}
