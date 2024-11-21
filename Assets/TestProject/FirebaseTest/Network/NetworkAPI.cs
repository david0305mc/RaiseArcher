using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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

public class NetworkAPI 
{

    public async UniTask Login(RequestSignInData _data)
    {
        string json = JsonUtility.ToJson(_data);
        UserInfo info = await NetworkManager.Instance.SendToServer<UserInfo>("", NetworkManager.SENDTYPE.POST, json);
        Debug.Log(info);
    }


}
