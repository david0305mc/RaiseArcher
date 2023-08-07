using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager : SingletonMono<GameManager>
{

    public void GenerateRandomItemEvent()
    {
        var targetTile = UserData.Instance.GetEmptyTile();
        var randomItemData = DataManager.Instance.GetRandomItem();
        var itemData = UserData.Instance.AddItemData(randomItemData.id, targetTile.Item1, targetTile.Item2);
        AddItemObj(itemData.uid);
        UserData.Instance.SaveLocalData();
    }

    public void MoveItemEvent(UIItemObj itemObj, UITileObj targetTileObj)
    {
        var existedItemData = UserData.Instance.LocalData.GetItem(targetTileObj.gridX, targetTileObj.gridY);
        if (existedItemData != null)
        {
            var emptyTile = UserData.Instance.GetEmptyTile(itemObj.ItemData.x, itemObj.ItemData.y);
            UserData.Instance.MoveItem(existedItemData.uid, emptyTile.Item1, emptyTile.Item2);
            uiItemObjDic[existedItemData.uid].MoveToTarget(tileObjDic[emptyTile.Item1][emptyTile.Item2].transform.position).Forget();

            itemObj.MoveToTarget(targetTileObj.transform.position).Forget();
            UserData.Instance.MoveItem(itemObj.ItemData.uid, targetTileObj.gridX, targetTileObj.gridY);
        }
        else
        {
            itemObj.MoveToTarget(targetTileObj.transform.position).Forget();
            UserData.Instance.MoveItem(itemObj.ItemData.uid, targetTileObj.gridX, targetTileObj.gridY);
        }
        UserData.Instance.SaveLocalData();
    }

    public void MoveItemToPlaySlotEvent(int _playSlotIndex, UIItemObj itemObj)
    {
        PlaySlotData prevSlotData = UserData.Instance.LocalData.playSlotDataDic[_playSlotIndex];
        int prevItemUID = prevSlotData.itemUID;
        UIItemObj prevUIitemObj = uiItemObjDic[prevItemUID];

        UserData.Instance.SetPlayItemSlot(_playSlotIndex, itemObj.ItemData.uid);
        tankDic[_playSlotIndex].UpdateData();
        itemObj.MoveToTarget(playItemSlotDic[_playSlotIndex].transform.position).Forget();

        var targetTile = UserData.Instance.GetEmptyTile(itemObj.ItemData.x, itemObj.ItemData.y);
        var prevItemData = UserData.Instance.MoveItem(prevItemUID, targetTile.Item1, targetTile.Item2);
        prevUIitemObj.MoveToTarget(GetItemPos(prevItemData)).Forget();
        UserData.Instance.SaveLocalData();
    }

}
