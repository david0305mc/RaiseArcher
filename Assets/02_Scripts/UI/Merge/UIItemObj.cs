using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemObj : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    public void SetData(int _uid)
    {
        var itemData = UserData.Instance.LocalData.GetItem(_uid);
        var itemInfo = DataManager.Instance.GetItemLevelData(itemData.tid);
        iconImage.SetSprite(itemInfo.iconpath);
    }
}
