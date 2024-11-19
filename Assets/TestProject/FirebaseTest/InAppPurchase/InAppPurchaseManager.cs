using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class InAppPurchaseManager : SingletonMono<InAppPurchaseManager>, IStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    public static string productID = "gem_01_60"; // Play Console에 등록된 SKU
    

    void Start()
    {
        if (storeController == null)
        {
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

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
        else
        {
            Debug.LogError("RestorePurchases is not supported on this platform.");
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: Successful.");
        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"OnInitializeFailed: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == productID)
        {
            Debug.Log($"Purchase successful: {args.purchasedProduct.definition.id}");
            // 구매 성공 처리 (예: 아이템 지급)
        }
        else
        {
            Debug.LogError($"Purchase failed: {args.purchasedProduct.definition.id}");
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, {failureReason}");
    }
}
