using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using UniRx;
using System.Linq;

[System.Serializable]
public class LocalData
{
    public int uidSeed;
    public ReactiveProperty<long> Gold;
    public SerializableDictionary<int, ItemData> itemDataDic;
    public SerializableDictionary<int, PlaySlotData> playSlotDataDic;
    
    public LocalData()
    {
        uidSeed = 0;
        Gold = new ReactiveProperty<long>(0);
        itemDataDic = new SerializableDictionary<int, ItemData>();
        playSlotDataDic = new SerializableDictionary<int, PlaySlotData>();
        LoadDefaultData();
    }

    private void LoadDefaultData()
    {
        Enumerable.Range(0, GameConfig.MaxHeroCount).ToList().ForEach(i =>
        {
            playSlotDataDic[i].index = i;
            playSlotDataDic[i].itemUID = GameConfig.DefaultItemID;
        });
    }

    public void UpdateRefData()
    {
        foreach (var item in itemDataDic)
            item.Value.UpdateRefData();
    }
    public ItemData GetItem(int _x, int _y)
    {
        return itemDataDic.FirstOrDefault(data => data.Value.x == _x && data.Value.y == _y).Value;
    }

    public ItemData GetItem(int _uid)
    {
        if (itemDataDic.ContainsKey(_uid))
            return itemDataDic[_uid];
        return default;
    }

    public int GetTankItem(int _index)
    {
        if (playSlotDataDic.ContainsKey(_index))
            return playSlotDataDic[_index].itemUID;
        return -1;
    }
}

[System.Serializable]
public class ItemData
{
    public int uid;
    public int tid;
    public int x;
    public int y;
    public int playerSlotIndex;
    public DataManager.ItemLevel RefItemLevel;

    public void UpdateRefData()
    {
        RefItemLevel = DataManager.Instance.GetItemLevelData(tid);
    }

    public static ItemData Create(int _uid, int _tid, int _x, int _y, int _playerSlotIndex)
    {
        ItemData newData = new ItemData() { uid = _uid, tid = _tid, x = _x, y = _y, playerSlotIndex = _playerSlotIndex };
        newData.UpdateRefData();
        return newData;
    }
}

[System.Serializable]
public class PlaySlotData
{
    public int index;
    public int itemUID;
    public static PlaySlotData Create(int _index, int _itemTID)
    {
        PlaySlotData newData = new PlaySlotData() { index = _index, itemUID = _itemTID };
        return newData;
    }
}




public class EnemyData
{
    public int uid;
    public int tid;
    public int hp;

    public static EnemyData Create(int _uid, int _tid, int _hp)
    {
        EnemyData newData = new EnemyData() { uid = _uid, tid = _tid, hp = _hp };
        return newData;
    }
}