using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UserDataManager : Singleton<UserDataManager>
{
    public ulong Uno { get; set; }
    public BaseData baseData { get; set; } = new BaseData();
    public InventoryData inventoryData { get; set; } = new InventoryData();


}


public class SData
{ 
    public void GetClassName()
    {
        Debug.Log(GetType().Name);
    }
    public T ConvertToObject<T>(string _saveData)
    {
        return JsonUtility.FromJson<T>(_saveData);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}

public class BaseData : SData
{
    public IntReactiveProperty gold = new IntReactiveProperty();
    public int level;
    
    public SerializableDictionary<int, int> dicTest = new SerializableDictionary<int, int>();

    public void AddGold()
    {
        gold.Value++;
    }
    public void AddDicTest(int add)
    {
        GetClassName();
        if (dicTest.ContainsKey(1))
        {
            dicTest[1] = dicTest[1] + add;
        }
        else
        {
            dicTest[1] = add;
        }
        
    }
}

public class InventoryData : SData
{
    public List<int> itemList = new List<int>();
    public void AddItem()
    {
        GetClassName();
        itemList.Add(Random.Range(0, 1000));
    }
}
