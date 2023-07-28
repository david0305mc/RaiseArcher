using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
public class MainUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button testBtn;
    [SerializeField] private Button test2Btn;

    private void Awake()
    {
        //testBtn.onClick.AddListener(() =>
        //{
        //    PopupManager.Instance.ShowSystemOneBtnPopup("setste", "aaaa");
        //});

        UserData.Instance.LocalData.Gold.Subscribe(v =>
        {
            goldText.SetText(v.ToString());
        }).AddTo(gameObject);

        test2Btn.onClick.AddListener(() =>
        {
            UserData.Instance.LocalData.Gold.Value++;
            UserData.Instance.SaveLocalData();
            PopupManager.Instance.Show<ShopPopup>();
        });
    }
}
