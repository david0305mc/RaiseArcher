using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace NetworkTest
{
    public enum Method { GET, POST }

    public class Response
    {
        public int status;
        public string message;
        public string server;
    }

    public class Request
    {
        public int id;

        public WWWForm GetForm()
        {
            WWWForm form = new WWWForm();
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(int))
                {
                    form.AddField(field.Name, (int)field.GetValue(this));
                }
                else if (field.FieldType == typeof(string))
                {
                    form.AddField(field.Name, (string)field.GetValue(this));
                }
            }
            return form;
        }
    }

    public class NetworkCall : Singleton<NetworkCall>
    {
        public string serverUrl = "www.naver.com";

        public void OnClickPostRequest()
        {
            Process<Response>(Method.POST, string.Empty,
                (response) =>
                {
                }, (response) =>
                {

                }).Forget();
        }
        private async UniTask<T> Post<T>(string action, Request data) where T : Response
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(5));
            using UnityWebRequest req = UnityWebRequest.Post($"{serverUrl}{action}", data.GetForm());
            try
            {
                var res = await req.SendWebRequest().WithCancellation(cts.Token);
                var responseString = res.downloadHandler.text;
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            catch (UnityWebRequestException e)
            {
                return JsonConvert.DeserializeObject<T>(e.Text);
            }
        }

        private async UniTask<T> Get<T>(string action, Request data) where T : Response
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(5));
            using var req = UnityWebRequest.Get($"{serverUrl}{action}");
            try
            {
                var res = await req.SendWebRequest().WithCancellation(cts.Token);
                var responseString = res.downloadHandler.text;
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            catch (UnityWebRequestException e)
            {
                return JsonConvert.DeserializeObject<T>(e.Text);
            }
        }

        private async UniTask<T> Process<T>(Method method, string action, Action<T> onSuccess, Action<T> onFailure, Request request = null) where T : Response
        {
            var result = method switch
            {
                Method.GET => await Post<T>(action, request),
                Method.POST => await Get<T>(action, request),
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            };

            if (/*에러 판별*/result.message == null) onSuccess?.Invoke(result);
            else onFailure?.Invoke(result);
            return result;
        }
    }
}
