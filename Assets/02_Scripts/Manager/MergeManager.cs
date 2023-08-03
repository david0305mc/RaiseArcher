using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Game;

public class MergeManager : SingletonMono<MergeManager>
{
    [SerializeField] private UITileObj tileObjPref;
    [SerializeField] private UIItemObj itemObjPref;
    [SerializeField] private GameObject tileRoot;

    private Dictionary<int, Dictionary<int, UITileObj>> tileObjDic;
    // Start is called before the first frame update
    void Start()
    {
        tileObjDic = new Dictionary<int, Dictionary<int, UITileObj>>();
        
        for (int y = 0; y < GameConfig.Tile_Row; y++)
        {
            tileObjDic[y] = new Dictionary<int, UITileObj>();
            for (int x = 0; x < GameConfig.Tile_Col; x++)
            {
                var tileObj = Lean.Pool.LeanPool.Spawn(tileObjPref, tileRoot.transform);
                tileObj.y = y;
                tileObj.x = x;
                tileObjDic[y][x] = tileObj;
            }
        }

        //Enumerable.Range(0, 10).ToList().ForEach(i => {
        //    var tileObj = tileObjDic.ElementAt(i);
        //    var itemObj = Lean.Pool.LeanPool.Spawn(itemObjPref, );

        //});

    }

    public void AddItem(ItemData itemData)
    {
        UIItemObj itemObj = Lean.Pool.LeanPool.Spawn(itemObjPref, tileObjDic[itemData.y][itemData.x].transform);
        itemObj.SetData(itemData.uid);
    }
    
}
