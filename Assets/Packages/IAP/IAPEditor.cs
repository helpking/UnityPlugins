using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;

class IAPEmulationTimer : MonoBehaviour
{	
	public static void SetTimer( int delay, System.Action onTimer )
	{
		GameObject go = new GameObject("_IAP_Emulation_Timer");
		GameObject.DontDestroyOnLoad(go);
		IAPEmulationTimer timerBeh = go.AddComponent<IAPEmulationTimer>();
		timerBeh.Setup(delay, onTimer);
	}

	#region Implementation
	Action _onTimer;
	int _delay;

	void Setup( int delay, System.Action onTimer)
	{
		_delay = delay;
		_onTimer = onTimer;
		StartCoroutine(WaiterRoutine());
	}
			
	IEnumerator WaiterRoutine()
	{
		for( int i = 0; i < _delay; ++i )
			yield return null;
		
		_onTimer();
		GameObject.Destroy(gameObject);
	}
	#endregion
}

namespace IAP
{
	public class IAPEditor : IAPBase
	{	
		protected override void ValidateIAPProducts (string[] products)
		{
			Assert.IsFalse (products != null && products.GetLength (0) > 0);
			IAPEmulationTimer.SetTimer(
				DELAY,
				delegate()
				{
					foreach (string s in products)
					{
						_updatedProducts.Add(s, new Product {title = s, description = "Test Product", productIdentifier = s,
							price =  4.99m, priceAsString = "$4.99", currencySymbol = "$", currencyCode = "USD", localeIdentifier = "en_US", countryCode = "US"});
					}
					finishProductsValidation();
				}
			);
		}

		protected override void BuyIAPWithProductID (string productId, Int32 quantity)
		{
			Assert.IsFalse (_validatedProducts.ContainsKey(productId));
			transactionStarted(productId);
			IAPEmulationTimer.SetTimer (DELAY, delegate() {
				int item = -1;
				string foundProductId = null;
				int i = 0;
				foreach(IIAPProduct product in _validatedProducts.Values)
				{
					if (String.Compare (product.productIdentifier, productId, false) == 0)
					{
						item = i;
						foundProductId = product.productIdentifier;
						break;
					}
					++i;
				}

				if (foundProductId == null)
					return;

				switch (item) {
				case -1:
				case 1:
					transactionFailed(productId, "Product auto failed for test purpose", 0);
					break;
				case 2:
					transactionCancelled(productId);
					break;
				case 3:
					transactionDeferred(productId);
					IAPEmulationTimer.SetTimer(
						DELAY,
						() => transactionCompleted(productId, quantity, "", "")
					);
					break;
				default:
					transactionCompleted(productId, quantity, "", "");
					break;
				}
			});
		}

		protected override void RestoreCompletedIAPTransactions ()
		{
			int delay = DELAY;
			foreach (IIAPProduct product in _validatedProducts.Values)
			{
				if (
					product.productIdentifier.Contains("nonconsum") ||
					product.productIdentifier.Contains("durable")
				)
				{
					string productId = product.productIdentifier;
					IAPEmulationTimer.SetTimer(
						delay++,
						() => transactionRestored (productId, 1, "", "")
					);
				}
			}
			if (failRestoring) {
				IAPEmulationTimer.SetTimer (
				delay += DELAY,
				() => finishedRestoringWithError("Failed for testing purposes", -1));
			} else {
				IAPEmulationTimer.SetTimer (
				delay++,
				finishedRestoring);
			}
			failRestoring = !failRestoring;
		}
		
		protected override bool IsIAPEnabled()
		{
			return true;
		}

		const int DELAY = 150;
		static bool failRestoring = false;
	}
}
