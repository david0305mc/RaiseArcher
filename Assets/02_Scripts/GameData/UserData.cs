using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UniRx;

public partial class UserData : Singleton<UserData>
{

    private static readonly string LocalFilePath = Path.Combine(Application.persistentDataPath, "LocalData");
    public LocalData LocalData { get; set; }

    public  ReactiveProperty<bool> IsEnemyItemSelected { get; set; }

    public int ShopSelectedItem { get; set; } 
    public void InitData()
    {
        ShopSelectedItem = -1;
        IsEnemyItemSelected = new ReactiveProperty<bool>(false);
    }

    public void RemoveObj(int uid)
    {
        LocalData.BaseObjDic.Remove(uid);
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
        }
        
    }

    public void SaveLocalData()
    {
        var saveData = JsonUtility.ToJson(LocalData);
        //saveData = Utill.EncryptXOR(saveData);
        Utill.SaveFile(LocalFilePath, saveData);
    }

}
