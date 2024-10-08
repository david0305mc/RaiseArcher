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
    private FirebaseAuth Auth { get; set; }
    public FirebaseUser User { get; set; }
    public string EMail { get; set; }

    public readonly string EmailPw = "testEmail";
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

    public EPlatform GetFirebaseSignType()
    {
        if (!IsFirebaseSigned())
            return EPlatform.None;
        if (Auth.CurrentUser.IsAnonymous)
            return EPlatform.Guest;
        foreach (var p in Auth.CurrentUser.ProviderData)
        {
            Debug.LogFormat("[Firebase/ProviderData] {0}", p.ProviderId);
            if (p.ProviderId == GoogleAuthProvider.ProviderId)
                return EPlatform.Google;
            if (p.ProviderId == "apple.com")
                return EPlatform.Apple;
        }
        return EPlatform.Unknown;
    }

    public async UniTask<bool> SignInWithPlatform(EPlatform _platform, CancellationTokenSource _cts)
    {
        if (!IsFirebaseSigned())
        {
            Debug.Log("SignInWithPlatform 01");
            Credential credential;
            switch (_platform)
            {
                case EPlatform.Google:
                    await SignInWithGoogle().AttachExternalCancellation(_cts.Token);
                    break;
                case EPlatform.Guest:
                    await SignInWithGuest().AttachExternalCancellation(_cts.Token);
                    break;
                case EPlatform.Email:
                    await SignInWithEmail().AttachExternalCancellation(_cts.Token);
                    break;
                default:
                    await SignInWithGuest().AttachExternalCancellation(_cts.Token);
                    break;
            }
            Debug.Log($"test1");
        }
        Debug.Log($"test2");
        var token = await Auth.CurrentUser.TokenAsync(true).AsUniTask().AttachExternalCancellation(_cts.Token);
        
        Debug.Log($"Token {token}");
        //SetActiveUser("Guest");


        //token = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImUwM2E2ODg3YWU3ZjNkMTAyNzNjNjRiMDU3ZTY1MzE1MWUyOTBiNzIiLCJ0eXAiOiJKV1QifQ.eyJwcm92aWRlcl9pZCI6ImFub255bW91cyIsImlzcyI6Imh0dHBzOi8vc2VjdXJldG9rZW4uZ29vZ2xlLmNvbS9hYnlzc2NsYXNzaWMiLCJhdWQiOiJhYnlzc2NsYXNzaWMiLCJhdXRoX3RpbWUiOjE3MjY3Mzk0NjgsInVzZXJfaWQiOiJGV213T2MxdTZDWE9mR05Pb1ZlMWlqaGRLVEEzIiwic3ViIjoiRldtd09jMXU2Q1hPZkdOT29WZTFpamhkS1RBMyIsImlhdCI6MTcyNjc0MDAyMSwiZXhwIjoxNzI2NzQzNjIxLCJmaXJlYmFzZSI6eyJpZGVudGl0aWVzIjp7fSwic2lnbl9pbl9wcm92aWRlciI6ImFub255bW91cyJ9fQ.GryMezHmwqb6GmFbQCMEkiqwSs4MTc5uPRieb43PpK-L7poD6r6fAkFrCPYrwDshQLPlpPoAt6zEZMa8r7b36uvgqseoH8yef2AwKZQqbLdKigJVbe1P3BjOW1HP05u87CwHNVNp-Q-5v8Xw2xuVij3VGdu9rE5QZO1A1fqGvOZTo2QGz2zyjZso6y8FFF3w9nlZEMMWHJwfMTgo90F-587SvYkR1MfTf91bqUoEiEnEXmdvI1hqmAhwhYjdCnbFd9U4uIDFBqQ73Mn6BLCTkslrHINzF1nFbR8hw3IQ57bIW7JJYFlsN-yXyfBKzaXI-onOdllpWDZcwMHLO3IXDg";
        //var repSignIn = await ServerAPI.SignIn(_platform, token, "KO", string.Empty, default).AttachExternalCancellation(_cts.Token);
        //Debug.Log($"test4");
        //var repLogin = await ServerAPI.Login(repSignIn.uno, repSignIn.token, default).AttachExternalCancellation(_cts.Token);
        //UserDataManager.Instance.Uno = repSignIn.uno;
        //Debug.Log($"test5");
        //if (repSignIn.first_login == 0)
        //{
        //    await ServerAPI.LoadFromServer(_cts.Token);
        //}
        //else
        //{ 
        //    // new User
        //}
        return true;
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
    public async UniTask SignInWithGoogle()
    {
        try
        {
            Debug.Log("SignInWithGoogle");
            var credential = await GetGoogleCredential();
            await Auth.SignInWithCredentialAsync(credential).AsUniTask();
        }
        catch
        {
            Debug.Log("Retry");
            await UniTask.Delay(100);
            await SignInWithGoogle();
        }
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

    public async UniTask LinkAccount(EPlatform target)
    {
        if (!IsFirebaseSigned())
            return;

        if (GetFirebaseSignType() != EPlatform.Guest)
            return;

        switch (target)
        {
            case EPlatform.Email:
                {
                    //var ret = await Auth.SignInWithEmailAndPasswordAsync(EMail, EmailPw).AsUniTask();
                    //return ret.Credential;
                    Firebase.Auth.Credential credential = Firebase.Auth.EmailAuthProvider.GetCredential(EMail, EmailPw);
                    await Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task =>
                    {
                        if (task.IsCanceled)
                        {
                            Debug.LogError("LinkWithCredentialAsync was canceled.");
                            return;
                        }
                        if (task.IsFaulted)
                        {
                            Debug.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);
                            // Gather data for the currently signed in User.
                            string currentUserId = Auth.CurrentUser.UserId;
                            string currentEmail = Auth.CurrentUser.Email;
                            string currentDisplayName = Auth.CurrentUser.DisplayName;
                            System.Uri currentPhotoUrl = Auth.CurrentUser.PhotoUrl;

                            // Sign in with the new credentials.
                            Auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
                                if (task.IsCanceled)
                                {
                                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                                    return;
                                }
                                if (task.IsFaulted)
                                {
                                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                                    return;
                                }

                                Firebase.Auth.AuthResult result = task.Result;
                                Debug.LogFormat("User signed in successfully: {0} ({1})",
                                    result.User.DisplayName, result.User.UserId);

                                // TODO: Merge app specific details using the newUser and values from the
                                // previous user, saved above.
                            });

                            return;
                        }

                        Firebase.Auth.AuthResult result = task.Result;
                        Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})",
                            result.User.DisplayName, result.User.UserId);
                    });
                }
                break;
            case EPlatform.Google:
                {
                    Social.localUser.Authenticate((bool success) => {
                        if (success)
                        {
                            var authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                            Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                            Firebase.Auth.Credential credential =
                                Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);

                            auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task => {
                                if (task.IsCanceled)
                                {
                                    Debug.LogError("LinkWithCredentialAsync was canceled.");
                                    return;
                                }
                                if (task.IsFaulted)
                                {
                                    Debug.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);

                                    // Sign in with the new credentials.
                                    auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
                                        if (task.IsCanceled)
                                        {
                                            Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                                            return;
                                        }
                                        if (task.IsFaulted)
                                        {
                                            Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                                            return;
                                        }

                                        Firebase.Auth.AuthResult result = task.Result;
                                        Debug.LogFormat("User signed in successfully: {0} ({1})",
                                            result.User.DisplayName, result.User.UserId);

                                        // TODO: Merge app specific details using the newUser and values from the
                                        // previous user, saved above.
                                    });


                                    return;
                                }

                                Firebase.Auth.AuthResult result = task.Result;
                                Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})",
                                    result.User.DisplayName, result.User.UserId);
                            });
                        }
                    });

                }
                break;
        }
    }



}
