using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBase : MonoBehaviour
{
    [SerializeField] private Animator openAnim = null;
    private System.Action hideAction;
    private System.Action completeOpenAni;

    private void Start()
    {
        //openAnim.Play("PopupOpen");
    }
    public virtual void InitPopup(System.Action _hideAction)
    {
        hideAction = _hideAction;

        if (openAnim != null && openAnim.enabled)
        {
            openAnim.Update(0);
            //StartCoroutine(CompleteOpenAnim());
            
            UniTask.Create(async () => {
                await UniTask.WaitUntil(() => !openAnim.IsInTransition(0));
                await UniTask.WaitUntil(() => openAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
                completeOpenAni?.Invoke();
            });
        }
        else
        {
            completeOpenAni?.Invoke();
        }
    }

    public void Hide()
    {
        hideAction?.Invoke();
        hideAction = null;
    }

    public void OnClickCloseBtn()
    {
        Hide();
    }
    IEnumerator CompleteOpenAnim()
    {
        yield return new WaitUntil(() => !openAnim.IsInTransition(0));

        while (openAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            yield return null;

        completeOpenAni?.Invoke();
    }

}
