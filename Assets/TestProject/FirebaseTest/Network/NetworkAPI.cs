using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

using NetworkTest;
using System.Linq;

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
    public static async UniTask<EServerStatus> GetServerStatus(CancellationTokenSource _cts)
    {
        VersionData[] datas = await NetworkManager.Instance.GetRequest<VersionData[]>(string.Format("{0}/version_list.json", ServerSetting.commonUrl), _cts);
        Debug.Log(datas);

        if (datas != null && datas.Length > 0)
        {
            var orderedEnumerable = datas.OrderByDescending(d => d.version);

            //서버에 해당 버전 테이블이 없는경우 앱 강제 업데이트 요구됨
            //var versionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.version == BuildSetting.version);
            var versionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.version == 10500);
            ServerSetting.Set(versionData);

            //var lastVersionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.status == EServerStatus.Live);
            var lastVersionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode());
            ServerSetting.lastAppVersion = lastVersionData.version;
            Debug.LogFormat("[Server/Setting/LastAppVersion] {0}", ServerSetting.lastAppVersion);
        }

        return ServerSetting.status;
    }

    public static async UniTask<SignInRes> SignIn(EPlatform _platform, string _firebaseToken, CancellationTokenSource _cts)
    {
        //string url = Utility.URLAntiCacheRandomizer(ServerSetting.gameUrl);
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
        var reqContext = NetworkTest.RequestContext.Create(ServerSetting.gameUrl, ServerCmd.AUTH_USER_LOGIN, data);
        
        SignInRes res = await NetworkManager.Instance.SendToServer<SignInRes>(reqContext, _cts);
        Debug.Log(res);
        return res;
    }

    public static async UniTask<LoginRes> Login(ulong uno, string token, CancellationTokenSource _cts)
    {
        var data = new LoginReq();
        data.uno = uno;
        data.token = token;

        var reqContext = NetworkTest.RequestContext.Create(ServerSetting.gameUrl, ServerCmd.AUTH_GAME_LOGIN, data);
        LoginRes res = await NetworkManager.Instance.SendToServer<LoginRes>(reqContext, _cts);
        
        ServerSetting.sess = res.session;
        Debug.Log("Session : " + res.session);
        Debug.Log("uNO : " + uno);
        return res;
    }
}
