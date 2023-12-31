
using UnityEngine;
using Game;
public partial class UserData : Singleton<UserData>
{

    public EnemyData AddEnemy(int _uid)
    {
        EnemyData data = EnemyData.Create(_uid, -1, -1);
        EnemyDataDic.Add(_uid, data);
        return data;
    }

    public void RemoveEnemy(int _uid)
    {
        if (EnemyDataDic.ContainsKey(_uid))
        {
            EnemyDataDic.Remove(_uid);
        }
    }

    public (int, int) GetEmptyTile(int _includeX = -1, int _includeY = -1)
    {
        for (int y = 0; y < GameConfig.Tile_Row; y++)
        {
            for (int x = 0; x < GameConfig.Tile_Col; x++)
            {
                if (x == _includeX && y == _includeY)
                {
                    return (x, y);
                }

                if (LocalData.GetItem(x, y) == default)
                {
                    return (x, y);
                }
            }
        }
        return (-1, -1);
    }

    public PlaySlotData AddTank(int _index, int _itemUID)
    {
        PlaySlotData data = PlaySlotData.Create(_index, _itemUID);
        LocalData.playSlotDataDic[_index] = data;
        return data;
    }
    public void SetPlayItemSlot(int _index, int _uid)
    {
        LocalData.playSlotDataDic[_index].itemUID = _uid;
        LocalData.itemDataDic[_uid].playerSlotIndex = _index;
    }

    public ItemData AddItemData(int _tid, int _x, int _y, int _playerSlotIndex = -1)
    {
        int uid = GenerateUID();
        ItemData data = ItemData.Create(uid, _tid, _x, _y, _playerSlotIndex);
        LocalData.itemDataDic[uid] = data;
        if (_playerSlotIndex >= 0)
        {
            LocalData.playSlotDataDic[_playerSlotIndex] = PlaySlotData.Create(_playerSlotIndex, data.uid);
        }
        return data;
    }

    public void RemoveItemData(int _uid)
    {
        if (LocalData.itemDataDic.ContainsKey(_uid))
            LocalData.itemDataDic.Remove(_uid);
    }

    public ItemData MoveItem(int _uid, int _x, int _y)
    {
        LocalData.itemDataDic[_uid].x = _x;
        LocalData.itemDataDic[_uid].y = _y;
        LocalData.itemDataDic[_uid].playerSlotIndex = -1;
        return LocalData.itemDataDic[_uid];
    }
}
