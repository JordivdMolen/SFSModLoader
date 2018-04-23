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
		bool flag = Ref.hasHackedExpansion && !Ref.hasPartsExpansion;
		if (flag)
		{
			this.msgText.text = "Invalid purchase receipt. If your purchase was legit, please contact the developer.";
			this.msgText.gameObject.SetActive(true);
		}
	}

	public void InitializePurchasing()
	{
		bool flag = this.IsInitialized();
		if (!flag)
		{
			ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(), new IPurchasingModule[0]);
			configurationBuilder.AddProduct(IAP.PARTS_EXPANSION_ID, (ProductType)1);
			UnityPurchasing.Initialize(this, configurationBuilder);
			Product product = IAP.m_StoreController.products.WithID(IAP.PARTS_EXPANSION_ID);
			bool flag2 = product != null && product.hasReceipt;
			if (flag2)
			{
				Ref.hasPartsExpansion = true;
				Saving.SaveSetting(Saving.SettingKey.hasPartDLC, true);
			}
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
		bool flag = this.IsInitialized();
		if (flag)
		{
			Product product = IAP.m_StoreController.products.WithID(productId);
			bool flag2 = product != null && product.availableToPurchase;
			if (flag2)
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
		bool flag = !this.IsInitialized();
		if (flag)
		{
			this.msgText.text = "RestorePurchases FAIL. Not initialized.";
		}
		else
		{
			bool flag2 = Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer;
			if (flag2)
			{
				this.msgText.text = "RestorePurchases started ...";
				IAppleExtensions extension = IAP.m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
				extension.RestoreTransactions(delegate(bool result)
				{
					this.msgText.text = "RestorePurchases continuing: " + result.ToString() + ". If no further messages, no purchases available to restore.";
				});
			}
			else
			{
				this.msgText.text = "RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform;
			}
		}
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		this.msgText.text = "OnInitialized: PASS";
		IAP.m_StoreController = controller;
		IAP.m_StoreExtensionProvider = extensions;
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		this.msgText.text = "OnInitializeFailed InitializationFailureReason:" + error;
	}

	private void Update()
	{
		bool flag = this.invoke;
		if (flag)
		{
			this.invoke = false;
			this.onComplete.Invoke();
		}
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{
		bool flag = string.Equals(args.purchasedProduct.definition.id, IAP.PARTS_EXPANSION_ID, StringComparison.Ordinal);
		if (flag)
		{
			bool flag2 = true;
			bool flag3 = flag2;
			if (flag3)
			{
				Saving.SaveSetting(Saving.SettingKey.hasPartDLC, true);
				Ref.hasPartsExpansion = true;
				IAP.main.onComplete.Invoke();
				Saving.SaveSetting(Saving.SettingKey.hasHackedDLC, false);
				Ref.hasHackedExpansion = false;
				this.msgText.gameObject.SetActive(false);
			}
		}
		else
		{
			this.msgText.text = string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id);
		}
		return 0;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		this.msgText.text = string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason);
	}

	public IAP()
	{
	}

	static IAP()
	{
		// Note: this type is marked as 'beforefieldinit'.
	}

	private static IStoreController m_StoreController;

	private static IExtensionProvider m_StoreExtensionProvider;

	public UnityEvent onComplete;

	public bool invoke;

	public Text msgText;

	public static IAP main;

	public static string PARTS_EXPANSION_ID = "parts_expansion";
}
