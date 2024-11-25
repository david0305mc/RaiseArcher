using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;


public class AdManager : Singleton<AdManager>
{

#if UNITY_ANDROID
    //private string _adUnitId = "ca-app-pub-3940256099942544/5224354917";
    private string _adUnitId = "ca-app-pub-9673687584530511/1466634455";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
  private string _adUnitId = "unused";
#endif

    private Queue<RewardedAd> adQueue = new Queue<RewardedAd>();
    private bool isInit = false;
    private readonly int queueSize = 2;
    public void InitAD()
    {
        if (!isInit)
        {
            MobileAds.Initialize(
                initStatus => {
                    isInit = true;
                    Debug.Log("MobileAds.Initialize Complete");
                    InitAdQueue();
                });
        }
    }

    private void InitAdQueue()
    {
        for (int i = 0; i < queueSize; i++)
        {
            LoadRewardedAd().Forget();
        }
    }
    public async UniTask LoadRewardedAd()
    {
        UniTaskCompletionSource<RewardedAd> ucs = new UniTaskCompletionSource<RewardedAd>();
        var adRequest = new AdRequest();
        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
                ucs.TrySetResult(null);
                return;
            }

            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

            ucs.TrySetResult(ad);
        });

        var result = await ucs.Task;
        if (result != null)
        {
            if (result.CanShowAd())
            {
                adQueue.Enqueue(result);
                RegisterReloadHandler(result);
                
                Debug.Log($"Enqueue {adQueue.Count}");
            }
            else
            {
                Debug.LogError("LoadRewardedAd Fail Can not Show");
                result.Destroy();
                await UniTask.Yield();
                await LoadRewardedAd();
            }
        }
        else
        {
            Debug.LogError("LoadRewardedAd Fail ad null");
            await UniTask.Yield();
            await LoadRewardedAd();
        }
    }

    public void ShowRewardedAd()
    {
        if (adQueue.Count > 0)
        {
            var rewardedAD = adQueue.Dequeue();
            ShowRewardedAd(rewardedAD);
        }
        else
        {
            Debug.Log("adQueue.Count == 0");
        }
    }

    public void ShowRewardedAd(RewardedAd _rewardedAd)
    {
        const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";
        _rewardedAd.Show((Reward reward) =>
        {
            // TODO: Reward the user.
            Debug.Log(string.Format(rewardMsg, reward.Type, reward.Amount));
            _rewardedAd.Destroy();
        });
    }

    private void RegisterReloadHandler(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
    }


}
