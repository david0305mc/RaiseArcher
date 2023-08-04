using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UniRx;
using System.Linq;

public partial class UserData : Singleton<UserData>
{

    private static readonly string LocalFilePath = Path.Combine(Application.persistentDataPath, "LocalData");
    public LocalData LocalData { get; set; }
    public SerializableDictionary<int, EnemyData> EnemyDataDic;

    public  ReactiveProperty<bool> IsEnemyItemSelected { get; set; }

    public int ShopSelectedItem { get; set; }

    public int GenerateUID()
    {
        return LocalData.uidSeed++;
    }
    public void InitData()
    {
        ShopSelectedItem = -1;
        IsEnemyItemSelected = new ReactiveProperty<bool>(false);
        EnemyDataDic = new SerializableDictionary<int, EnemyData>();
    }

    public int GetRandomEnemy()
    {
        if (EnemyDataDic.Count == 0)
            return -1;
        int rand = UnityEngine.Random.Range(0, EnemyDataDic.Count);
        return EnemyDataDic.ElementAt(rand).Value.uid;
    }

    public void LoadLocalData()
    {
        if (File.Exists(LocalFilePath))
        {
            var localData = Utill.LoadFromFile(LocalFilePath);
            //localData = Utill.EncryptXOR(localData);
            LocalData = JsonUtility.FromJson<LocalData>(localData);
            LocalData.UpdateRefData();
        }
        else
        {
            // NewGame
            LocalData = new LocalData();
            LocalData.UpdateRefData();
        }
        
    }

    public void SaveLocalData()
    {
        var saveData = JsonUtility.ToJson(LocalData);
        //saveData = Utill.EncryptXOR(saveData);
        Utill.SaveFile(LocalFilePath, saveData);
    }

}
