using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
public class PopupManager : SingletonMono<PopupManager>
{
    private Dictionary<string, PopupBase> popupDic = new Dictionary<string, PopupBase>();

    public CommonPopup ShowSystemOneBtnPopup(string messageStr, string btnStr, System.Action okAction = null)
    {
        var popup = Show<CommonPopup>();
        popup.SetOneBtnData(messageStr, btnStr, okAction);
        return popup;
    }

    public CommonPopup ShowSystemTwoBtnPopup(string messageStr, string btnStr, System.Action okAction = null, System.Action cancelAction = null)
    {
        var popup = Show<CommonPopup>();
        popup.SetTwoBtnData(messageStr, btnStr, okAction, cancelAction);
        return popup;
    }

    public T Show<T>(System.Action _hideAction = null) where T : PopupBase
    {
        var prefabName = typeof(T).Name;
        var uiPopup = MakePopup(prefabName).GetComponent<T>();
        uiPopup.InitPopup(() => {
            popupDic.Remove(prefabName);
            Destroy(uiPopup.gameObject);
            _hideAction?.Invoke();
        });
        return uiPopup;
    }

    private PopupBase MakePopup(string prefabName)
    {
        if (popupDic.TryGetValue(prefabName, out var popup))
        {
            return popup;
        }
        var goPopup = Instantiate(Resources.Load(System.IO.Path.Combine(GamePath.PopupPath, prefabName)), Vector3.zero, Quaternion.identity, gameObject.transform) as GameObject;
        goPopup.name = prefabName;

        var uiPopup = goPopup.GetComponent<PopupBase>();
        var rect = uiPopup.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        popupDic[prefabName] = uiPopup;
        return uiPopup;
    }
}
