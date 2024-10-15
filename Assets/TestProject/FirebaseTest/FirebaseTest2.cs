using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Threading;
using TMPro;
using UniRx;

public class FirebaseTest2 : MonoBehaviour
{
    [SerializeField] private SelectPlatformPopup platformPopup;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button logOutButton;
    [SerializeField] private Button connectGoogleButton;
    [SerializeField] private Button disConnectGoogleButton;
    [SerializeField] private Button connectAppleButton;
    [SerializeField] private Button connectEmailButton;
    [SerializeField] private Button disConnectEmailButton;


    [SerializeField] private GameObject mainObj;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI unoText;
    [SerializeField] private TMP_InputField emailInput;


    private CompositeDisposable compositeDisposable = new CompositeDisposable();
    private CancellationTokenSource cancelltaionTokenSource = new CancellationTokenSource();
    private void Awake()
    {
        mainObj.SetActive(false);
        platformPopup.gameObject.SetActive(false);
        loginButton.onClick.AddListener(() =>
        {
            StartGame().Forget();
        });
        levelUpButton.onClick.AddListener(() =>
        {
            //UserDataManager.Instance.baseData.level++;
            UserDataManager.Instance.baseData.AddGold();
            //UserDataManager.Instance.baseData.AddDicTest(1);
            //UserDataManager.Instance.inventoryData.AddItem();
            ServerAPI.SaveToServer();

        });
        logOutButton.onClick.AddListener(() =>
        {
            AuthManager.Instance.SignOut();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

        });

        connectGoogleButton.onClick.AddListener(() =>
        {
            UniTask.Create(async () =>
            {
                await AuthManager.Instance.LinkAccount(EPlatform.Google);
                UpdatePlatformUI();
            });
        });
        disConnectGoogleButton.onClick.AddListener(() =>
        {
            UniTask.Create(async () =>
            {
                await AuthManager.Instance.UnLinkAccount(EPlatform.Google);
                UpdatePlatformUI();
            });
        });

        connectEmailButton.onClick.AddListener(() =>
        {
            UniTask.Create(async () =>
            {
                await AuthManager.Instance.LinkAccount(EPlatform.Email);
                unoText.SetText(AuthManager.Instance.User.UserId);
                UpdatePlatformUI();
            });
        });
        disConnectEmailButton.onClick.AddListener(() =>
        {
            UniTask.Create(async () =>
            {
                await AuthManager.Instance.UnLinkAccount(EPlatform.Email);
                UpdatePlatformUI();
            });
        });

        emailInput.onValueChanged.AddListener(_value => {
            AuthManager.Instance.EMail = _value;
        });
        loginButton.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        cancelltaionTokenSource.Clear();
        AuthManager.Instance.Dispose();
        compositeDisposable.Clear();

    }

    void Start()
    {
        Init().Forget();   
    }

    private async UniTaskVoid Init()
    {
        Debug.Log("Init Try");
        var result = await AuthManager.Instance.Initialize();
        if (result)
        {
            Debug.Log("Init Success");
            loginButton.gameObject.SetActive(true);
        }
    }
    private async UniTaskVoid StartGame()
    {
        var serverStatus = await ServerAPI.GetServerStatus(cancellationToken: cancelltaionTokenSource.Token);
        loginButton.gameObject.SetActive(false);

        if (!AuthManager.Instance.IsFirebaseSigned())
        {
            platformPopup.gameObject.SetActive(true);
            UniTaskCompletionSource<EPlatform> ucs = new UniTaskCompletionSource<EPlatform>();
            platformPopup.Set(_platform =>
            {
                ucs.TrySetResult(_platform);
            });
            var platform = await ucs.Task;
            platformPopup.gameObject.SetActive(false);

            await AuthManager.Instance.SignInWithPlatform(platform, cancelltaionTokenSource);
            UpdatePlatformUI();
        }
        mainObj.SetActive(true);

        unoText.SetText(AuthManager.Instance.User.UserId);

        UserDataManager.Instance.baseData.gold.Subscribe(_gold =>
        {
            levelText.SetText(_gold.ToString());
        }).AddTo(compositeDisposable);
        UpdatePlatformUI();
    }

    private void UpdatePlatformUI()
    {
        List<EPlatform> provideList = AuthManager.Instance.GetProvideTypeList();
        
        connectGoogleButton.SetActive(!provideList.Contains(EPlatform.Google));
        disConnectGoogleButton.SetActive(provideList.Contains(EPlatform.Google));

        connectEmailButton.SetActive(!provideList.Contains(EPlatform.Email));
        disConnectEmailButton.SetActive(provideList.Contains(EPlatform.Email));
    }
}
