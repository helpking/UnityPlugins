using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace IAP
{
	public class IAPLogging : IIAP
	{
		private readonly IIAP _concrete;
		
		public IAPLogging(IIAP concreteInstance)
		{
			_concrete = concreteInstance;

			_concrete.onTransactionCompleted += (x, y, z, w) => Debug.Log("onTransactionCompleted: product=" + x + ", quantity=" + y + ", transactionId=" + z + ", receipt=" + w);
			_concrete.onTransactionFailed += (x, y, z) => Debug.LogError("onTransactionFailed: product=" + x + ", error=" + y + ", errorCode=" + z);
			_concrete.onTransactionCancelled += x => Debug.Log("onTransactionCancelled: product=" + x);
			_concrete.onTransactionRestored += (x, y, z, w) => Debug.Log("onTransactionRestored: product=" + x + ", quantity=" + y + ", transactionId=" + z + ", receipt=" + w);
			_concrete.onTransactionStarted += x => Debug.Log("onTransactionStarted: product=" + x);
			_concrete.onTransactionDeferred += x => Debug.Log("onTransactionDeferred: product=" + x);
			_concrete.onProductsValidated += x => Debug.Log ("onProductsValidated: changed=" + x);
			_concrete.onProductsValidationFailed += (x, y) => Debug.LogError ("onProductsValidationFailed: error=" + x + ", errorCode=" + y);
			_concrete.onFinishedRestoring += () => Debug.Log ("onFinishedRestoring:");
			_concrete.onFinishedRestoringWithError += (x, y) => Debug.LogError ("onFinishedRestoringWithError: error=" + x + ", errorCode=" + y);
			_concrete.onStartedProductsValidation += x => { 
				StringBuilder builder = new StringBuilder();
				builder.Append("onStartedProductsValidation: products=");
				foreach(string product in x) { builder.Append('\n').Append(product); }
				Debug.Log(builder.ToString());
			};
			_concrete.onStartedRestoring += () => Debug.Log ("onStartedRestoring:");
		}

		#region IIAP implementation

		public event Action<string, Int32, string, string> onTransactionCompleted { 
			add { _concrete.onTransactionCompleted += value; } remove {_concrete.onTransactionCompleted -= value; } 
		}
		public event Action<string, string, int> onTransactionFailed {
			add { _concrete.onTransactionFailed += value; } remove {_concrete.onTransactionFailed -= value; }
		}
		public event Action<string> onTransactionCancelled {
			add { _concrete.onTransactionCancelled += value; } remove {_concrete.onTransactionCancelled -= value; }
		}
		public event Action<string, Int32, string, string> onTransactionRestored {
			add { _concrete.onTransactionRestored += value; } remove { _concrete.onTransactionRestored -= value; }
		}
		public event Action<string> onTransactionStarted {
			add { _concrete.onTransactionStarted += value; } remove { _concrete.onTransactionStarted -= value; }
		}
		public event Action<string> onTransactionDeferred {
			add { _concrete.onTransactionDeferred += value; } remove { _concrete.onTransactionDeferred -= value; }
		}
		public event Action<bool> onProductsValidated {
			add { _concrete.onProductsValidated += value; } remove { _concrete.onProductsValidated -= value; }
		}
		public event Action<string, Int32> onProductsValidationFailed {
			add { _concrete.onProductsValidationFailed += value; } remove { _concrete.onProductsValidationFailed -= value; }
		}
		public event Action onFinishedRestoring {
			add { _concrete.onFinishedRestoring += value; } remove { _concrete.onFinishedRestoring -= value; }
		}
		public event Action<string, int> onFinishedRestoringWithError {
			add { _concrete.onFinishedRestoringWithError += value; } remove { _concrete.onFinishedRestoringWithError -= value; }
		}
		public event Action<string[]> onStartedProductsValidation {
			add { _concrete.onStartedProductsValidation += value; } remove { _concrete.onStartedProductsValidation -= value; }
		}
		public event Action onStartedRestoring {
			add { _concrete.onStartedRestoring += value; } remove { _concrete.onStartedRestoring -= value; }
		}

		public void StartProcessing() 
		{
			Debug.Log("StartProcessing()");
			_concrete.StartProcessing();
		}

		public void StopProcessing() 
		{
			Debug.Log("StopProcessing()");
			_concrete.StopProcessing();
		}

		public void ValidateProducts(string[] products)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("ValidateProducts(");
			foreach(string product in products) { builder.Append('\n').Append(product); }
			builder.Append("\n)");
			Debug.Log(builder.ToString());

			_concrete.ValidateProducts(products);
		}

		public IEnumerable<IIAPProduct> GetValidatedProducts()
		{
			Debug.Log("GetValidatedProducts()");
			return _concrete.GetValidatedProducts();
		}

		public Int32 GetValidatedProductsCount()
		{
			int count = _concrete.GetValidatedProductsCount();
			Debug.Log("GetValidatedProductsCount() returns " + count);
			return count;
		}

		public IIAPProduct GetValidatedProduct(string productId)
		{
			Debug.Log("GetValidatedProduct(" + productId + ")");
			return _concrete.GetValidatedProduct(productId);
		}

		public void StartTransaction(string productId)
		{
			Debug.Log("StartTransaction(" + productId + ")");
			_concrete.StartTransaction(productId);
		}

		public void StartTransaction(string product, Int32 quantity)
		{
			Debug.Log("StartTransaction(" + product + ", " + quantity+  ")");
			_concrete.StartTransaction(product, quantity);
		}

		public void RestoreCompletedTransactions()
		{
			Debug.Log("RestoreCompletedTransactions()");
			_concrete.RestoreCompletedTransactions();
		}

		public void FinalizeTransaction(string transactionID)
		{
			Debug.Log("FinalizeTransaction(" + transactionID + ")");
			_concrete.FinalizeTransaction(transactionID);
		}

		public bool IsEnabled()
		{
			bool isEnabled = _concrete.IsEnabled();
			Debug.Log("IsEnabled() returns" + isEnabled);
			return isEnabled;
		}

		#endregion
	}
}

