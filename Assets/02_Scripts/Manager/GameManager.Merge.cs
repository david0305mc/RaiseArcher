using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using System.Linq;

public partial class GameManager : SingletonMono<GameManager>
{
    [SerializeField] private UITileObj tileObjPref;
    [SerializeField] private UIItemObj itemObjPref;
    [SerializeField] private UIPlayItemSlot playItemSlotPref;
    [SerializeField] private GameObject tileRoot;
    [SerializeField] private GameObject itemRoot;
    [SerializeField] private GameObject playItemSlotRoot;

    private Dictionary<int, UIPlayItemSlot> playItemSlotDic;
    private Dictionary<int, Dictionary<int, UITileObj>> tileObjDic;

    private void InitMergeTile()
    {
        // Merge Tile
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

        // PlayItemSlot
        playItemSlotDic = new Dictionary<int, UIPlayItemSlot>();
        Enumerable.Range(0, GameConfig.MaxHeroCount).ToList().ForEach(i =>
        {
            var playItemSlotObj = Lean.Pool.LeanPool.Spawn(playItemSlotPref, playItemSlotRoot.transform);
            playItemSlotObj.index = i;
            playItemSlotDic[i] = playItemSlotObj;
        });
    }
    public void InitPlayItemSlot()
    {
        foreach (var item in UserData.Instance.LocalData.playSlotDataDic)
        {
            AddItem(item.Value.itemUID);
        }
    }

    private Vector2 GetItemPos(ItemData itemData)
    {
        if (itemData.playerSlotIndex >= 0)
        {
            return playItemSlotDic[itemData.playerSlotIndex].transform.position;
        }
        return tileObjDic[itemData.x][itemData.y].transform.position;
    }
    private void AddItem(int _itemUID)
    {
        var itemData = UserData.Instance.LocalData.GetItem(_itemUID);
        Vector2 pos = GetItemPos(itemData);

        UIItemObj itemObj = Lean.Pool.LeanPool.Spawn(itemObjPref, pos, Quaternion.identity, itemRoot.transform);
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
                    UIPlayItemSlot uiTankSlotObj = hitObj.GetComponent<UIPlayItemSlot>();
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
    public void AddRandomItem()
    {
        var targetTile = UserData.Instance.GetEmptyTile();
        var randomItemData = DataManager.Instance.GetRandomItem();
        var itemData = UserData.Instance.AddItemData(randomItemData.id, targetTile.Item1, targetTile.Item2);
        AddItem(itemData.uid);
    }

}
