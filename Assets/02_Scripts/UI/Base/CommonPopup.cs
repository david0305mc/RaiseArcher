using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonPopup : PopupBase
{
    [SerializeField] private Text messageText;
    [SerializeField] private Text okBtnText;
    [SerializeField] private Button okBtn;
    [SerializeField] private Button cancelBtn;

    public void SetOneBtnData(string messageStr, string btnStr, System.Action okBtnAction)
    {
        messageText.text = messageStr;
        okBtnText.text = btnStr;
        okBtn.onClick.RemoveAllListeners();
        okBtn.onClick.AddListener(() => {
            okBtnAction?.Invoke();
            Hide();
        });
        cancelBtn.gameObject.SetActive(false);
    }

    public void SetTwoBtnData(string messageStr, string btnStr, System.Action okBtnAction, System.Action cancelAction = null)
    {
        SetOneBtnData(messageStr, btnStr, okBtnAction);
        cancelBtn.gameObject.SetActive(true);
        cancelBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.AddListener(() => {
            cancelAction?.Invoke();
            Hide();
        });
    }

    public override void InitPopup(System.Action _hideAction)
    {
        base.InitPopup(_hideAction);
    }
}
