using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using System.Linq;

public partial class GameManager : SingletonMono<GameManager>
{
    [SerializeField] private UITileObj tileObjPref;
    [SerializeField] private UIItemObj itemObjPref;
    [SerializeField] private UITankSlotObj tankSlotObjPref;
    [SerializeField] private GameObject tileRoot;
    [SerializeField] private GameObject itemRoot;
    [SerializeField] private GameObject tankSlotRoot;

    private Dictionary<int, UITankSlotObj> tankSlotDic;
    private Dictionary<int, Dictionary<int, UITileObj>> tileObjDic;

    private void InitMergeTile()
    {
        tileObjDic = new Dictionary<int, Dictionary<int, UITileObj>>();

        for (int y = 0; y < GameConfig.Tile_Row; y++)
        {
            for (int x = 0; x < GameConfig.Tile_Col; x++)
            {
                if (!tileObjDic.ContainsKey(x))
                {
                    tileObjDic[x] = new Dictionary<int, UITileObj>();
                }

                var tileObj = Lean.Pool.LeanPool.Spawn(tileObjPref, tileRoot.transform);
                tileObj.x = x;
                tileObj.y = y;
                tileObjDic[x][y] = tileObj;
            }
        }

        tankSlotDic = new Dictionary<int, UITankSlotObj>();
        Enumerable.Range(0, 8).ToList().ForEach(i =>
        {
            var tankSlotObj = Lean.Pool.LeanPool.Spawn(tankSlotObjPref, tankSlotRoot.transform);
            tankSlotObj.index = i;
            tankSlotDic[i] = tankSlotObj;
        });
    }

    public void AddItem()
    {
        var targetTile = UserData.Instance.GetEmptyTile();
        var randomItemData = DataManager.Instance.GetRandomItem();
        var itemData = UserData.Instance.AddItemData(randomItemData.id, targetTile.Item1, targetTile.Item2);

        UIItemObj itemObj = Lean.Pool.LeanPool.Spawn(itemObjPref, tileObjDic[itemData.x][itemData.y].transform.position, Quaternion.identity, itemRoot.transform);
        itemObj.SetData(itemData.uid, (pointerEventData) => {
            var hitObj = RaycastUtilities.UIRaycast(pointerEventData, GameConfig.TileLayer);
            if (hitObj != null)
            {
                UITileObj uiTileObj = hitObj.GetComponent<UITileObj>();
                if (uiTileObj != null)
                {
                    UserData.Instance.MoveItem(itemData.uid, uiTileObj.x, uiTileObj.y);
                    itemObj.MoveToTarget(uiTileObj.transform.position).Forget();
                }
                else
                {
                    UITankSlotObj uiTankSlotObj = hitObj.GetComponent<UITankSlotObj>();
                    if (uiTankSlotObj != null)
                    {
                        GameManager.Instance.SetTankSlot(uiTankSlotObj.index, itemData.uid);
                        itemObj.MoveToTarget(uiTankSlotObj.transform.position).Forget();
                        //UserData.Instance.MoveItem(itemData.uid, uiTileObj.x, uiTileObj.y);
                    }
                }
            }
            else
            {
                UITileObj uiTileObj = tileObjDic[itemData.x][itemData.y];
                itemObj.MoveToTarget(uiTileObj.transform.position).Forget();
            }
        });
    }

}
