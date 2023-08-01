using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIIntro : MonoBehaviour
{
    [SerializeField] Button touchToStartBtn;

    private void Awake()
    {
        Application.targetFrameRate = 180;
        Screen.SetResolution(1080, 1920, true);
        touchToStartBtn.onClick.AddListener(() => {
            touchToStartBtn.onClick.RemoveAllListeners();
            StartGame().Forget();
            //var mainSceneAsync = SceneManager.LoadSceneAsync("Main");
            //await mainSceneAsync;
        });
    }
    private async UniTaskVoid StartGame()
    {
        Debug.Log("StartGame");
        await DataManager.Instance.LoadDataAsync();
        await DataManager.Instance.LoadConfigTable();
        DataManager.Instance.MakeClientDT();
        UserData.Instance.InitData();
        UserData.Instance.LoadLocalData();

        var mainSceneAsync = SceneManager.LoadSceneAsync("Main");
        await mainSceneAsync;
        //SceneManager.LoadScene("Main");
    }
}
