using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class ShrineShopController : MonoBehaviour
{
    [SerializeField] private MMFeedbacks UnlockFeedbacks;
    [SerializeField] private MMFeedbacks BuyUpgradeFeedbacks;

    private IntVariable statToUpgrade;
    private bool isLocked = true;

    public void AttemptShop()
    {
        if (isLocked) return;
        
        //Spend trophy to get upgrade
        if (UpgradeManager.Instance.TrySpendTrophy())
        {
            statToUpgrade.Value += 1;
            BuyUpgradeFeedbacks.PlayFeedbacks();
        }
    }

    public void UnlockShop()
    {
        isLocked = false;
        statToUpgrade = UpgradeManager.Instance.GetRandomStatWithRemoval();
        UnlockFeedbacks.PlayFeedbacks();
    }
    
}
