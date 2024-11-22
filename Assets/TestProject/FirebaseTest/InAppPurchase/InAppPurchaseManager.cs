using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Purchasing;
using Cysharp.Threading.Tasks;
using System.Threading;

public class InAppPurchaseManager : SingletonMono<InAppPurchaseManager>, IStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    public static string productID = "gem_01_60"; // Play Console에 등록된 SKU


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
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void BuyProduct()
    {
        if (!IsInitialized())
        {
            Debug.LogError("Purchasing is not initialized.");
            return;
        }

        Product product = storeController.products.WithID(productID);

        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"Buying product: {product.definition.id}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogError("BuyProduct: Product is not available for purchase.");
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
    }

    async UniTask BackEndValidation(Product product)
    {
        //UpdateUI();

        //Mock backend validation. Here you would call your own backend and wait for its response.
        //If the app is closed during this time, ProcessPurchase will be called again for the same purchase once the app is opened again.
        const int waitSeconds = 5;
        Debug.Log($"Purchase Pending, Waiting for confirmation for {waitSeconds} seconds - Product: {product.definition.id}");

        await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: cts.Token);


        //UpdateUI();

        Debug.Log($"Confirming purchase of {product.definition.id}");

        //Once we have done the validation in our backend, we confirm the purchase.

        storeController.ConfirmPendingPurchase(product);

        //We can now add the purchased product to the players inventory
        //if (product.definition.id == goldProductId)
        //{
        //    AddGold();
        //}
        UserDataManager.Instance.baseData.gold.Value += 1000;
        ServerAPI.SaveToServer();
    }

}
