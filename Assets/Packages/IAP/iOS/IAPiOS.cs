#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace IAP
{
	public class IAPiOS : IAPBase
	{
		protected override void ValidateIAPProducts(string[] products)
		{
			IAPValidateProducts(products, (Int16)products.Length);
		}

		protected override void StartIAPProcessing() 
		{
			IAPStartProcessing();
		}

		protected override void StopIAPProcessing() 
		{
			IAPStopProcessing();
		}

		protected override void BuyIAPWithProductID(string productId, Int32 quantity)
		{
			IAPBuyWithProductID(productId, quantity);
		}

		protected override void RestoreCompletedIAPTransactions()
		{
			IAPRestoreTransactions();
		}

		protected override void FinalizeIAPTransaction(string transactionID)
		{
			IAPFinalizeTransaction(transactionID);
		}

		protected override bool IsIAPEnabled()
		{
			return IAPIsEnabled();
		}

		#region Implementation
		/// <summary>
		/// 交易状态.
		/// </summary>
		enum TransactionState {
			/// <summary>
			/// 购买中.
			/// </summary>
			Purchasing = 0,
			/// <summary>
			/// 购买完成.
			/// </summary>
			Purchased,
			/// <summary>
			/// 失败.
			/// </summary>
			Failed,
			/// <summary>
			/// 购买恢复.
			/// </summary>
			Restored,
			/// <summary>
			/// 购买延迟.
			/// </summary>
			Deferred,
			/// <summary>
			/// 未知.
			/// </summary>
			Unknown
		}
		
		// Callback types for the native -> managed calls
		delegate void CreateProductCallback (string title,string description,string identifier,float price,string priceString,string currency,
			string code,string locale, string country);

		delegate void TransactionUpdatedCallback(string productID, TransactionState state, string transactionID, string receipt, Int32 quantity,
			Int32 errorCode, string error);

		delegate void RestoreFinishedCallback (string error, Int32 errorCode);
	
		static IAPiOS()
		{
			IAPSetCallbacks(CreateProduct, TransactionUpdated, RestoreFinished);
		}

		[MonoPInvokeCallback (typeof (TransactionUpdatedCallback))]
		static void TransactionUpdated(string productID, TransactionState state, string transactionID, string receipt, Int32 quantity,
			Int32 errorCode, string error)
		{
			switch(state)
			{
			case TransactionState.Purchasing:
				transactionStarted(productID);
				break;
			case TransactionState.Purchased:
//				if (IAPVerifyTransaction (transactionID)) 
				{
					transactionCompleted(productID, quantity, transactionID, receipt);
				} 
//				else 
//				{
//					IAPFinalizeTransaction(transactionID);
//					transactionFailed(productID, "Transaction verification failed", -1);
//				}
				break;
			case TransactionState.Failed:
				if (errorCode == 2) 
				{
					transactionCancelled(productID);
				} 
				else 
				{
					transactionFailed(productID, error, errorCode);
				}
				break;
			case TransactionState.Restored:
				if (IAPVerifyTransaction (transactionID)) 
				{
					transactionRestored(productID, quantity, transactionID, receipt);
				} 
				else
				{
					IAPFinalizeTransaction(transactionID);
					transactionFailed(productID, "Transaction verification failed", -1);
				}
				break;
			case TransactionState.Deferred:
				transactionDeferred(productID);
				break;
			}
		}

		[MonoPInvokeCallback (typeof (CreateProductCallback))]
		static void CreateProduct(string title, string description, string identifier, float price, string priceString, 
			string currency, string code, string locale, string country)
		{
			if (identifier != null) 
			{	// this is an actual product info
				Product received = new Product {title = title, description = description, productIdentifier = identifier,
					price = (decimal)price, priceAsString = priceString, currencySymbol = currency, currencyCode = code,
					localeIdentifier = locale, countryCode = country};
				_updatedProducts.Add (identifier, received);
			}
			else 
			{	// special case to mark the end of the product list or a validation error
				if (description == null) 
				{
					finishProductsValidation ();
				}
				// description contains error message, and price contains error code
				else 
				{
					productsValidationFailed (description, (Int32)price);
				}
			}
		}

		[MonoPInvokeCallback (typeof (RestoreFinishedCallback))]
		static void RestoreFinished(string error, Int32 errorCode)
		{
			if (error != null)
			{
				finishedRestoringWithError(error, errorCode);
			}
			else
			{
				finishedRestoring();
			}
		}

		[DllImport ("__Internal")]
		static extern void IAPSetCallbacks(CreateProductCallback callback, TransactionUpdatedCallback callback2, RestoreFinishedCallback callback3);

		[DllImport ("__Internal")]
		static extern void IAPValidateProducts(string[] productIDs, Int16 count);
		
		[DllImport ("__Internal")]
		static extern void IAPBuyWithProductID(string product, Int32 quantity);

		[DllImport ("__Internal")]
		static extern void IAPRestoreTransactions();

		[DllImport ("__Internal")]
		static extern void IAPFinalizeTransaction(string transactionID);

		[DllImport ("__Internal")]
		static extern bool IAPVerifyTransaction(string transactionID);
		
		[DllImport ("__Internal")]
		static extern void IAPStartProcessing();
		
		[DllImport ("__Internal")]
		static extern void IAPStopProcessing();
		
		[DllImport ("__Internal")]
		static extern bool IAPIsEnabled();
		#endregion
	}
}
#endif