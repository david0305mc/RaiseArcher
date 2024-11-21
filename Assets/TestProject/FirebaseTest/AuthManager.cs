#if UNITY_ANDROID
#define ENABLE_GOOGLE_PLAY
#elif UNITY_IOS
#define ENABLE_GOOGLE_SIGN
#endif

using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Messaging;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AuthManager : Singleton<AuthManager>, IDisposable
{
    private FirebaseApp _app;
    public FirebaseAuth Auth { get; set; }
    public FirebaseUser User { get; set; }
    public string EMail { get; set; }

    public readonly string EmailPw = "123456789kmc";
    private bool initialized = false;
    public UniTaskCompletionSource<string> pushToken = new UniTaskCompletionSource<string>();
    public async UniTask<bool> Initialize()
    {
        if (initialized)
            return false;

#if UNITY_EDITOR
        pushToken.TrySetResult(string.Empty);
#endif      
        var result = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
        if (result == DependencyStatus.Available)
        {
            InitializeFirebase();
            InitializeGPGS();
            initialized = true;
            return true;
        }
        return false;
    }

    public bool IsFirebaseSigned()
    {
        return Auth != null && Auth.CurrentUser != null;
    }

    public List<EPlatform> GetProvideTypeList()
    {
        List<EPlatform> retList = new List<EPlatform>();
        if (!IsFirebaseSigned())
            return retList;

        foreach (var p in Auth.CurrentUser.ProviderData)
        {
            Debug.LogFormat("[Firebase/ProviderData] {0}", p.ProviderId);
            if (p.ProviderId == GoogleAuthProvider.ProviderId)
                retList.Add(EPlatform.Google);
            if (p.ProviderId == "apple.com")
                retList.Add(EPlatform.Apple);
            if (p.ProviderId == "password")
                retList.Add(EPlatform.Email);
        }
        if (retList.Count == 0)
        {
            //if (Auth.CurrentUser.IsAnonymous)    
            retList.Add(EPlatform.Guest);
        }
        return retList;
    }

    public EPlatform GetProviedType()       
    {
        var platformList = GetProvideTypeList();
        if (platformList.Contains(EPlatform.Google))
            return EPlatform.Google;
        if (platformList.Contains(EPlatform.Apple))
            return EPlatform.Apple;
        return EPlatform.Guest;
    }
    public bool HasProvideType(EPlatform _platform)
    {
        var list = GetProvideTypeList();
        return list.Contains(_platform);
    }

    public async UniTask SignInWithPlatform(EPlatform _platform, CancellationTokenSource _cts)
    {
        Debug.Log("SignInWithPlatform 01");
        
        switch (_platform)
        {
            case EPlatform.Google:
                {
                    Credential credential = await GetGoogleCredential();
                    await Auth.SignInWithCredentialAsync(credential).AsUniTask().AttachExternalCancellation(_cts.Token);
                }
                break;
            case EPlatform.Guest:
                await Auth.SignInAnonymouslyAsync().AsUniTask().AttachExternalCancellation(_cts.Token);
                break;
            case EPlatform.Email:
                await Auth.SignInWithEmailAndPasswordAsync(EMail, EmailPw).AsUniTask().AttachExternalCancellation(_cts.Token);
                break;
            default:
                await Auth.SignInAnonymouslyAsync().AsUniTask().AttachExternalCancellation(_cts.Token);
                break;
        }
        Debug.Log($"test1");
    }
    public async UniTask<string> GetFirebaseToken(CancellationTokenSource _cts)
    {
        return await Auth.CurrentUser.TokenAsync(true).AsUniTask().AttachExternalCancellation(_cts.Token);
    }

    public async UniTask LoginGameServer(EPlatform _platform, string _firebaseToken, CancellationTokenSource _cts)
    {
        var repSignIn = await NetworkAPI.SignIn(_platform, _firebaseToken, _cts);
        Debug.Log($"test4");
        var repLogin = await ServerAPI.Login(repSignIn.uno, repSignIn.token, _cts.Token);

        Debug.Log($"test5");
        UserDataManager.Instance.LoadLocalData(repSignIn.uno);
    }
    public async UniTask LoginGameServerOld(EPlatform _platform, string _firebaseToken, CancellationTokenSource _cts)
    {
        var repSignIn = await ServerAPI.SignIn(_platform, _firebaseToken, "KO", string.Empty, _cts.Token);
        Debug.Log($"test4");
        var repLogin = await ServerAPI.Login(repSignIn.uno, repSignIn.token, _cts.Token);
        
        Debug.Log($"test5");
        UserDataManager.Instance.LoadLocalData(repSignIn.uno);
        if (repSignIn.first_login == 0)
        {
            // To Do : �ð� �� �� ����ȭ

            SaveData data = await ServerAPI.LoadFromServer(_cts.Token);
            if (data == null)
            {
                Debug.Log("data == null");
                return;
            }
            
            //UserData.Instance.LoadLocalData();
            
            var versionRaw = data.save_datas.Find(item => item.tableName == "DBVersion");
            if (versionRaw == null)
            {
                Debug.LogError("versionRaw == null");
                return;
            }
            
            DBVersion serverVersion = JsonUtility.FromJson<DBVersion>(versionRaw.save_data);
            // ���� ��ϰ� �� ��ũ

            if (UserDataManager.Instance.dbVersion.dbVersion < serverVersion.dbVersion)
            {
                UserDataManager.Instance.dbVersion.dbVersion = serverVersion.dbVersion;
                // apply serverData
                foreach (var item in data.save_datas)
                {
                    if (item.tableName == "BaseData")
                    {
                        UserDataManager.Instance.baseData = UserDataManager.Instance.baseData.ConvertToObject<BaseData>(item.save_data);
                    }
                    else if (item.tableName == "InventoryData")
                    {
                        UserDataManager.Instance.inventoryData = UserDataManager.Instance.baseData.ConvertToObject<InventoryData>(item.save_data);
                    }
                }
                Debug.Log(UserDataManager.Instance.baseData.level);
                Debug.Log($"itemCount : {UserDataManager.Instance.inventoryData.itemList.Count}");
                UserDataManager.Instance.SaveLocalData();
            }

        }
        else
        {
            // new User
            UserDataManager.Instance.CreateNewUser(repSignIn.uno);
        }
    }

    private async UniTask<Credential> GetGoogleCredential()
    {
        Debug.Log("GetGoogleCredential 0");
#if ENABLE_GOOGLE_PLAY

        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.Log("authenticated");
        }
        else
        {
            UniTaskCompletionSource ucs = new UniTaskCompletionSource();
            Social.localUser.Authenticate((success, msg) =>
            {
                Debug.Log($"msg {msg}");
                if (!success)
                {
                    Debug.Log("GetGoogleCredential 1");
                    ucs.TrySetCanceled();
                    return;
                }
                Debug.Log("GetGoogleCredential 2");
                ucs.TrySetResult();
            });

            await ucs.Task;
        }

        var token = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
        Debug.Log($"token {token}");
        return GoogleAuthProvider.GetCredential(token, null);
#endif

#if ENABLE_GOOGLE_SIGN
        var signInUser = await GoogleSignIn.DefaultInstance.SignIn().AsUniTask();
        return GoogleAuthProvider.GetCredential(signInUser.IdToken, null);
#endif
    }
    
    public void SignOut()
    {
#if !UNITY_EDITOR && ENABLE_GOOGLE_PLAY
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            PlayGamesPlatform.Instance.SignOut();
#endif

#if !UNITY_EDITOR && ENABLE_GOOGLE_SIGN
        GoogleSignIn.DefaultInstance.SignOut();
#endif
        Auth.SignOut();
    }
    public void SignUpWithEmail()
    {
        Auth.CreateUserWithEmailAndPasswordAsync(EMail, EmailPw).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }
    private async UniTask<Credential> SignInWithEmail()
    {
        var ret = await Auth.SignInWithEmailAndPasswordAsync(EMail, EmailPw).AsUniTask();
        return ret.Credential;
    }

    private async UniTask<Credential> SignInWithGuest()
    {
        var ret = await Auth.SignInAnonymouslyAsync().AsUniTask();
        return ret.Credential;
    }
    private void InitializeFirebase()
    {
        Debug.Log("[Firebase] Setting up Firebase Auth");
        _app = FirebaseApp.DefaultInstance;
        Auth = FirebaseAuth.DefaultInstance;
        
        Auth.StateChanged += AuthStateChanged;
        Auth.IdTokenChanged += OnIdTokenChanged;

        //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        FirebaseMessaging.GetTokenAsync().AsUniTask()
            .ContinueWith(x =>
            {
                Debug.LogFormat("[Firebase] FirebaseMessaging Token: {0}", x);
                pushToken.TrySetResult(x);
            }).Forget();
        //AuthStateChanged(this, null);
    }
    void InitializeGPGS()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = BuildSetting.type == EBuildType.Dev;
        PlayGamesPlatform.Activate();
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (Auth.CurrentUser != User)
        {
            bool signedIn = User != Auth.CurrentUser && Auth.CurrentUser != null && Auth.CurrentUser.IsValid();
            if (!signedIn && User != null)
            {
                Debug.Log("Signed out " + User.UserId);
            }
            User = Auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + User.UserId);
                //displayName = user.DisplayName ?? "";
                //emailAddress = user.Email ?? "";
                //photoUrl = user.PhotoUrl ?? "";
            }
        }
    }
    void OnIdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        if (sender == null)
            return;

        Debug.LogFormat("[Firebase/OnIdTokenChanged] Sender : {0}", sender.ToString());
    }

    public void GetUserDataFromServer()
    {
        
    }

    public void SaveUserDataToServer()
    { 
    
    }

    public bool IsActiveEmptyUser()
    {
        return string.IsNullOrEmpty(GetActiveUser());
    }

    public string GetActiveUser()
    {
        string key = string.Format("{0}/DB/User", ServerSetting.serverName);
        return PlayerPrefs.GetString(key);
    }

    public void SetActiveUser(string userName)
    {
        string key = string.Format("{0}/DB/User", ServerSetting.serverName);
        PlayerPrefs.SetString(key, userName);
        
        Debug.LogFormat("[AuthManager/SetActiveUser] {0}", userName ?? "Empty");
    }

    public void Dispose()
    {
        Auth.StateChanged -= AuthStateChanged;
        Auth = null;
    }

    public async UniTask UnLinkAccount(EPlatform target)
    {
        if (!IsFirebaseSigned())
            return;
        if (HasProvideType(EPlatform.Guest))
            return;
        
        switch (target)
        {
            case EPlatform.Google:
                {
                    await Auth.CurrentUser.UnlinkAsync(GoogleAuthProvider.ProviderId);
                }
                break;
            case EPlatform.Apple:
                {
                    await Auth.CurrentUser.UnlinkAsync("apple.com");
                }
                break;
            case EPlatform.Email:
                {
                    await Auth.CurrentUser.UnlinkAsync("password");
                }
                break;
        }
    }
    public async UniTask<AuthResult> LinkWithCredentialAsync(Credential credential)
    {
        try
        {
            return await Auth.CurrentUser.LinkWithCredentialAsync(credential).AsUniTask();
        }
        catch (System.AggregateException e)
        {
            foreach (var ex in e.Flatten().InnerExceptions)
            {
                var linkEx = ex as FirebaseAccountLinkException;
                if (linkEx != null)
                {
                    Debug.LogErrorFormat("[Firebase/FirebaseAccountLinkException] ErrorCode : {0}\n{1}", linkEx.ErrorCode, linkEx.ToString());

                    credential = linkEx.UserInfo.UpdatedCredential.IsValid()
                        ? linkEx.UserInfo.UpdatedCredential
                        : credential;

                    throw new CredentialAlreadyInUseException(credential);
                }

                var firebaseEx = ex as FirebaseException;
                if (firebaseEx != null)
                {
                    Debug.LogErrorFormat("[Firebase/FirebaseException] ErrorCode : {0}\n{1}", firebaseEx.ErrorCode, firebaseEx.ToString());

                    if (firebaseEx.ErrorCode == (int)AuthError.CredentialAlreadyInUse || firebaseEx.ErrorCode == (int)AuthError.EmailAlreadyInUse)
                    {
                        //SignOut(_otherAuth);
                        throw new CredentialAlreadyInUseException(credential);
                    }
                }
                Debug.Log($"link error {ex.Message}");
            }

            Debug.LogErrorFormat("[Firebase/Link] {0}", e.ToString());
            throw e;
        }
    }

    public async UniTask LinkAccount(EPlatform target)
    {
        if (!IsFirebaseSigned())
            return;

        if (!HasProvideType(EPlatform.Guest))
            return;

        switch (target)
        {
            case EPlatform.Email:
                {
                    Firebase.Auth.Credential credential = Firebase.Auth.EmailAuthProvider.GetCredential(EMail, EmailPw);
                    try
                    { 
                        var result = await LinkWithCredentialAsync(credential);
                        Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})", result.User.DisplayName, result.User.UserId);
                    }
                    catch (CredentialAlreadyInUseException e)
                    {
                        try
                        {
                            var secondAuth = FirebaseAuth.GetAuth(FirebaseApp.Create(_app.Options, "Secondary"));
                            var signInResult = secondAuth.SignInAndRetrieveDataWithCredentialAsync(credential).AsUniTask();
                            var authResult = await signInResult;
                            await secondAuth.CurrentUser.DeleteAsync();
                            Debug.LogError(e.ToString());
                            try
                            {
                                //credential = Firebase.Auth.EmailAuthProvider.GetCredential(EMail, EmailPw);
                                var result = await LinkWithCredentialAsync(credential);
                                Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})", result.User.DisplayName, result.User.UserId);
                            }
                            catch (CredentialAlreadyInUseException e2)
                            {
                                Debug.LogError("CredentialAlreadyInUseException ");
                            }
                        }
                        catch(Exception e2)
                        {
                            Debug.LogError(e2.ToString());
                        }
                    }

                }
                break;
            case EPlatform.Google:
                {
                    try
                    {
                        var credential = await GetGoogleCredential();
                        try
                        {
                            var result = await LinkWithCredentialAsync(credential);
                            Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})", result.User.DisplayName, result.User.UserId);
                        }
                        catch (CredentialAlreadyInUseException e)
                        {
                            try
                            {
                                var secondAuth = FirebaseAuth.GetAuth(FirebaseApp.Create(_app.Options, "Secondary"));
                                var signInResult = secondAuth.SignInAndRetrieveDataWithCredentialAsync(credential).AsUniTask();
                                var authResult = await signInResult;
                                await secondAuth.CurrentUser.DeleteAsync();
                                try
                                {
                                    var result = await LinkWithCredentialAsync(credential);
                                    Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})", result.User.DisplayName, result.User.UserId);
                                }
                                catch (CredentialAlreadyInUseException e2)
                                {
                                    Debug.LogError("CredentialAlreadyInUseException ");
                                }
                            }
                            catch (Exception e2)
                            {
                                Debug.LogError(e2.ToString());
                            }
                        }
                        catch
                        {
                            Debug.LogError("Other catch");
                        }
                    }
                    catch
                    {
                        Debug.LogError("Error GetGoogleCredential");
                    }
                }
                break;
        }
    }



}


public class CredentialAlreadyInUseException : Exception
{
    public Credential credential;

    public CredentialAlreadyInUseException(Credential data) => credential = data;
}
