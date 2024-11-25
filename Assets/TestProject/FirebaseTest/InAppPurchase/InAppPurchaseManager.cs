using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Purchasing;
using Cysharp.Threading.Tasks;
using System.Threading;
using static NetworkAPI;


public class InAppPurchaseManager : SingletonMono<InAppPurchaseManager>, IStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    public static string productID = "gem_01_60"; // Play Console에 등록된 SKU
    public static int shopID = 2003002;


    private BillingOrderRes billingOrderRes;
    private UniTaskCompletionSource<BillingReceiptRes> purchaseUcs;
    private CancellationTokenSource cts;
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        cts?.Clear();
    }
    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        cts = new CancellationTokenSource();
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(productID, ProductType.Consumable); // Consumable, NonConsumable, Subscription 중 선택
        purchaseUcs = null;
        billingOrderRes = null;
        UnityPurchasing.Initialize(this, builder);
        if (purchaseUcs != null)
        {
            Debug.Log("purchaseUcs != null");
        }
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public async UniTask BuyProduct()
    {
        if (!IsInitialized())
        {
            Debug.LogError("Purchasing is not initialized.");
            return;
        }

        Product product = storeController.products.WithID(productID);
        if (product == null)
        {
            Debug.LogError("BuyProduct: Product is not available for purchase.");
            return;
        }

        if (!product.availableToPurchase)
        {
            Debug.LogError("BuyProduct: Product is not available for purchase.");
            return;
        }



        Debug.Log($"Buying product: {product.definition.id}");

        try
        {
            TouchBlockManager.Instance.AddLock();
            billingOrderRes = await BillingMakeOrder(EStoreType.Android, shopID, productID, product.metadata.isoCurrencyCode, default);
            purchaseUcs = new UniTaskCompletionSource<BillingReceiptRes>();
            storeController.InitiatePurchase(product);
            var res = await purchaseUcs.Task;
        }
        finally
        {
            TouchBlockManager.Instance.RemoveLock();
            purchaseUcs = null;
            billingOrderRes = null;
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.LogError("RestorePurchases: Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("Restoring purchases...");
            var appleExtensions = storeExtensionProvider.GetExtension<IAppleExtensions>();
            appleExtensions.RestoreTransactions((result) =>
            {
                Debug.Log("RestorePurchases: " + (result ? "Succeeded" : "Failed"));
            });
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: Successful.");
        storeController = controller;
        storeExtensionProvider = extensions;
        RestorePurchases();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"OnInitializeFailed: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Retrieve the purchased product
        var product = args.purchasedProduct;
        BackEndValidation(product).Forget();

        //We return Pending, informing IAP to keep the transaction open while we validate the purchase on our side.
        return PurchaseProcessingResult.Pending;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, {failureReason}");

        if (purchaseUcs != null)
        {
            if (failureReason == PurchaseFailureReason.UserCancelled)
            {
                purchaseUcs.TrySetCanceled();
            }
            else
            {
                purchaseUcs.TrySetException(new Exception(failureReason.ToString()));
            }

        }
    }

    async UniTask BackEndValidation(Product product)
    {
        //UpdateUI();

        //Mock backend validation. Here you would call your own backend and wait for its response.
        //If the app is closed during this time, ProcessPurchase will be called again for the same purchase once the app is opened again.
        const int waitSeconds = 5;
        Debug.Log($"Purchase Pending, Waiting for confirmation for {waitSeconds} seconds - Product: {product.definition.id}");

        await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: cts.Token);

        //EStoreType storeType, string bid, string product_id, int shop_idx, CancellationTokenSource cts
        string bid = billingOrderRes?.bid ?? "";
        BillingReceiptRes res = await BillingVerifyReceipt(EStoreType.Android, bid, product.definition.id, shopID, product, default);

        //UpdateUI();

        Debug.Log($"Confirming purchase of {product.definition.id}");

        //Once we have done the validation in our backend, we confirm the purchase.

        storeController.ConfirmPendingPurchase(product);

        //We can now add the purchased product to the players inventory
        //if (product.definition.id == goldProductId)
        //{
        //    AddGold();
        //}
        if (purchaseUcs != null)
        {
            Debug.Log("purchaseUcs != null");
            if (res == null)
            {
                Debug.Log("res == null");
            }
            purchaseUcs?.TrySetResult(res);
        }
        UserDataManager.Instance.baseData.gold.Value += 1000;
        SaveToServer();
    }

}

public class UnityProduct
{
    private Product _product;
    private Receipt _receipt;

    public Product product { get { return _product; } }
    public string productID { get { return _product.definition.id; } }
    public string transactionID { get { return _product.transactionID; } }

    public Receipt receipt
    {
        get
        {
            if (_product == null || !_product.hasReceipt)
                return null;

            if (_receipt == null)
                _receipt = JsonUtility.FromJson<Receipt>(_product.receipt);

            return _receipt;
        }
    }


    public UnityProduct(Product product) => _product = product;
    public static UnityProduct Get(Product product) => new UnityProduct(product);
}

[Serializable]
public class Receipt
{
    public string Store;
    public string TransactionID;
    public string Payload;

    private PayloadInfo _payLoadInfo;
    public PayloadInfo PayLoadInfo
    {
        get
        {
            if (Store != GooglePlay.Name)
                return null;

            if (string.IsNullOrEmpty(Payload))
                return null;

            if (_payLoadInfo == null)
                _payLoadInfo = JsonUtility.FromJson<PayloadInfo>(Payload);

            return _payLoadInfo;
        }
    }

    public override string ToString() => JsonUtility.ToJson(this);
}

[Serializable]
public class PayloadInfo
{
    public string json;
    public string signature;

    private PayloadData _data;
    public PayloadData data
    {
        get
        {
            if (string.IsNullOrEmpty(json))
                return null;

            if (_data == null)
                _data = JsonUtility.FromJson<PayloadData>(json);

            return _data;
        }
    }
}

[Serializable]
public class PayloadData
{
    public string orderId;
    public string packageName;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
    public string developerPayload;
    public override string ToString() => JsonUtility.ToJson(this);
}
