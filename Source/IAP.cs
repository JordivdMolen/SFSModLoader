using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

public class IAP : MonoBehaviour, IStoreListener
{
	private void Start()
	{
		IAP.main = this;
		this.InitializePurchasing();
		if (Saving.LoadSetting(Saving.SettingKey.hasHackedDLC) && !Ref.hasPartsExpansion)
		{
			this.msgText.text = "Invalid purchase receipt.   If your purchase was legit please contact the developer: games.morojna@gmail.com";
			this.msgText.gameObject.SetActive(true);
		}
	}

	public void InitializePurchasing()
	{
		if (this.IsInitialized())
		{
			return;
		}
		ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(), new IPurchasingModule[0]);
		configurationBuilder.AddProduct(IAP.PARTS_EXPANSION_ID, ProductType.NonConsumable);
		UnityPurchasing.Initialize(this, configurationBuilder);
		Product product = IAP.m_StoreController.products.WithID(IAP.PARTS_EXPANSION_ID);
		if (product != null && product.hasReceipt)
		{
			Ref.hasPartsExpansion = true;
			Saving.SaveSetting(Saving.SettingKey.hasPartDLC, true);
		}
	}

	private bool IsInitialized()
	{
		return IAP.m_StoreController != null && IAP.m_StoreExtensionProvider != null;
	}

	public void BuyPartsDLC()
	{
		this.BuyProductID(IAP.PARTS_EXPANSION_ID);
	}

	private void BuyProductID(string productId)
	{
		if (this.IsInitialized())
		{
			Product product = IAP.m_StoreController.products.WithID(productId);
			if (product != null && product.availableToPurchase)
			{
				this.msgText.text = "Purchasing product asychronously: " + product.definition.id;
				IAP.m_StoreController.InitiatePurchase(product);
			}
			else
			{
				this.msgText.text = "BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase";
			}
		}
		else
		{
			this.msgText.text = "BuyProductID FAIL. Not initialized.";
		}
	}

	public void RestorePurchases()
	{
		if (!this.IsInitialized())
		{
			this.msgText.text = "RestorePurchases FAIL. Not initialized.";
			return;
		}
		if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
		{
			this.msgText.text = "RestorePurchases started ...";
			IAppleExtensions extension = IAP.m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
			extension.RestoreTransactions(delegate(bool result)
			{
				this.msgText.text = "RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.";
			});
		}
		else
		{
			this.msgText.text = "RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform;
		}
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		IAP.m_StoreController = controller;
		IAP.m_StoreExtensionProvider = extensions;
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		this.msgText.text = "OnInitializeFailed InitializationFailureReason:" + error;
	}

	private void Update()
	{
		if (this.invoke)
		{
			this.invoke = false;
			this.onComplete.Invoke();
			this.msgText.gameObject.SetActive(false);
		}
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{
		if (string.Equals(args.purchasedProduct.definition.id, IAP.PARTS_EXPANSION_ID, StringComparison.Ordinal))
		{
			bool flag = true;
			if (flag)
			{
				Saving.SaveSetting(Saving.SettingKey.hasPartDLC, true);
				Saving.SaveSetting(Saving.SettingKey.hasHackedDLC, false);
				Ref.hasPartsExpansion = true;
				IAP.main.invoke = true;
			}
		}
		else
		{
			this.msgText.text = string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id);
		}
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		this.msgText.text = string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason);
	}

	private static IStoreController m_StoreController;

	private static IExtensionProvider m_StoreExtensionProvider;

	public UnityEvent onComplete;

	public bool invoke;

	public Text msgText;

	public static IAP main;

	public static string PARTS_EXPANSION_ID = "parts_expansion";
}
