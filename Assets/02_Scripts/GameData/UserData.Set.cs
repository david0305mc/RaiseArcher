
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

    public (int, int) GetEmptyTile()
    {
        for (int y = 0; y < GameConfig.Tile_Row; y++)
        {
            for (int x = 0; x < GameConfig.Tile_Col; x++)
            {
                if (LocalData.GetItem(x, y) == default)
                {
                    return (x, y);
                }
            }
        }
        return (-1, -1);
    }
    public ItemData AddItemData(int _tid, int _x, int _y)
    {
        int uid = UserData.Instance.GenerateUID();
        ItemData data = ItemData.Create(uid, _tid, _x, _y);
        LocalData.itemDataDic[uid] = data;
        return data;
    }

    public void RemoveItemData(int _uid)
    {
        if (LocalData.itemDataDic.ContainsKey(_uid))
            LocalData.itemDataDic.Remove(_uid);

    }
}
