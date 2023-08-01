
using UnityEngine;

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
}
