using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Protocols.Response
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
    #region ########### Auth ###########
    public class SignIn
    {
        public ulong uno;
        public string token;
        public string platform_id;
        /// <summary>Ż�� �����ð� -> 0�� �ƴ� ��� Ż���� �˾� ǥ��, ���� �α��� �������� ����</summary>
        public double leave_time;
        public string country;
        /// <summary>���� ���� �������� (0=��α���, 1=���� ���� ����)</summary>
        public int first_login;
    }

    public class Login
    {
        public string session;
    }
    #endregion
}
