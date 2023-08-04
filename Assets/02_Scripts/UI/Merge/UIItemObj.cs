using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Threading;

public class UIItemObj : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    private ItemData itemData;
    public ItemData ItemData => itemData;
    private System.Action<PointerEventData> endDragAction;

    private CancellationTokenSource cancelToken;

    public void OnBeginDrag(PointerEventData eventData)
    {
        cancelToken?.Cancel();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (itemData.playerSlotIndex >= 0)
            return;

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        endDragAction?.Invoke(eventData);
    }

    public void SetData(int _uid, System.Action<PointerEventData> _endDragAction)
    {
        endDragAction = _endDragAction;        
        itemData = UserData.Instance.LocalData.GetItem(_uid);
        var itemInfo = DataManager.Instance.GetItemLevelData(itemData.tid);
        iconImage.SetSprite(itemInfo.iconpath);
    }

    public async UniTaskVoid MoveToTarget(Vector3 _DestPos)
    {
        cancelToken?.Cancel();
        cancelToken = new CancellationTokenSource();
        while (true)
        {
            transform.position = Vector3.Lerp(transform.position, _DestPos, Time.deltaTime * 10f);
            if (Vector3.Distance(transform.position, _DestPos) < 0.03f)
            {
                transform.position = _DestPos;
                break;
            }
            await UniTask.WaitForFixedUpdate(cancelToken.Token);
        }
    }

}
