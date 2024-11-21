using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class NetworkManager : SingletonMono<NetworkManager>
{

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
        PostRequest("www.naver.com", string.Empty).Forget();
    }

    private async UniTask PostRequest(string url, string jsonData)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, ""))
        {
            // JSON 데이터 설정
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

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

}
