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

    public void MoveItemEvent(int _itemUID, int _gridX, int _gridY)
    {
        UserData.Instance.MoveItem(_itemUID, _gridX, _gridY);
        UserData.Instance.SaveLocalData();
    }

}
