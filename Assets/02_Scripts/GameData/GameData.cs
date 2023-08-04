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
    public SerializableDictionary<int, BaseObjData> BaseObjDic;
    public SerializableDictionary<int, ItemData> itemDataDic;
    public SerializableDictionary<int, TankData> tankDataDic;
    
    public LocalData()
    {
        uidSeed = 0;
        Gold = new ReactiveProperty<long>(0);
        BaseObjDic = new SerializableDictionary<int, BaseObjData>();
        itemDataDic = new SerializableDictionary<int, ItemData>();
        tankDataDic = new SerializableDictionary<int, TankData>();
    }

    public bool HasObj(int uid)
    {
        return BaseObjDic.ContainsKey(uid);
    }
    public void UpdateRefData()
    {
        foreach (var item in BaseObjDic)
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
        if (tankDataDic.ContainsKey(_index))
            return tankDataDic[_index].itemUID;
        return -1;
    }
}

[System.Serializable]
public class BaseObjData
{
    public bool IsEnemy;
    public int UID;
    public int TID;
    public int X;
    public int Y;
    public GameType.Direction Direction;
    public DataManager.ObjTable RefObjData;
    public void UpdateRefData()
    {
        RefObjData = DataManager.Instance.GetObjTableData(TID);
    }

    public static BaseObjData Create(int uid, int tid, int x, int y, bool isEnemy)
    {
        BaseObjData data = new BaseObjData();
        data.UID = uid;
        data.TID = tid;
        data.X = x;
        data.Y = y;
        data.IsEnemy = isEnemy;
        data.Direction = GameType.Direction.BOTTOM_RIGHT;
        data.UpdateRefData();
        return data;
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

    public static ItemData Create(int _uid, int _tid, int _x, int _y, int _playerSlotIndex)
    {
        ItemData newData = new ItemData() { uid = _uid, tid = _tid, x = _x, y = _y, playerSlotIndex = _playerSlotIndex };
        return newData;
    }
}

[System.Serializable]
public class TankData
{
    public int index;
    public int itemUID;
    public static TankData Create(int _index, int _itemTID)
    {
        TankData newData = new TankData() { index = _index, itemUID = _itemTID };
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