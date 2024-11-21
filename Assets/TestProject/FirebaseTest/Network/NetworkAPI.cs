using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

public class UserInfo
{
    public string token;
    public string username;
    public string name;

}

public class RequestSignInData
{
    public string username;
    public string password;
}
public class SignInReq
{
    public int platform;
    /// <summary>플랫폼 아이디 (firebase)</summary>
    public string platform_id;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string lang;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string push_id;
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int os;
    public string gpresto_sdata;
    public int gpresto_engine_state;
}

public class SignInRes
{
    public ulong uno;
    public string token;
    public string platform_id;
    /// <summary>탈퇴 유예시간 -> 0이 아닐 경우 탈퇴중 팝업 표시, 게임 로그인 진행하지 않음</summary>
    public double leave_time;
    public string country;
    /// <summary>최초 계정 생성여부 (0=재로그인, 1=최초 계정 생성)</summary>
    public int first_login;
}

public class LoginReq
{
    public ulong uno;
    public string token;
}

public class LoginRes
{
    public string session;
}


public class NetworkAPI 
{

    public static async UniTask<SignInRes> SignIn(EPlatform _platform, string _firebaseToken, CancellationTokenSource _cts)
    {
        SignInReq data = new SignInReq();
        data.platform = (int)_platform;
        data.platform_id = _firebaseToken;
        data.lang = "KO";
        data.push_id = string.Empty;
#if !UNITY_EDITOR && UNITY_ANDROID
        data.os = 1;
#elif !UNITY_EDITOR && UNITY_IOS
        data.os = 2;
#else
        data.os = 1;
#endif
        var reqContext = NetworkTest.RequestContext.Create(ServerCmd.AUTH_USER_LOGIN, data);
        
        SignInRes res = await NetworkManager.Instance.SendToServer<SignInRes>(reqContext, _cts.Token);
        Debug.Log(res);
        return res;
    }

    public static async UniTask<LoginRes> Login(ulong uno, string token, CancellationToken cancellationToken = default)
    {
        var data = new LoginReq();
        data.uno = uno;
        data.token = token;

        var reqContext = RequestContext.Create(ServerCmd.AUTH_GAME_LOGIN, data);
        var responseData = await UnityHttp.Send <LoginRes>(reqContext, cancellationToken);
        //userData.isLogin = true;
        //userData.uno = uno;
        
        ServerSetting.sess = responseData.session;
        Debug.Log("Session : " + responseData.session);
        Debug.Log("uNO : " + uno);
        return responseData;
    }
}
