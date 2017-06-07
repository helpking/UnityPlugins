using System;
using System.Collections.Generic;

namespace IAP
{
	public class IAPBase : IIAP
	{
		public event Action<string, Int32, string, string> onTransactionCompleted { 
			add { transactionCompleted += value; } remove {transactionCompleted -= value; } 
		}
		public event Action<string, string, int> onTransactionFailed {
			add { transactionFailed += value; } remove {transactionFailed -= value; }
		}
		public event Action<string> onTransactionCancelled {
			add { transactionCancelled += value; } remove {transactionCancelled -= value; }
		}
		public event Action<string, Int32, string, string> onTransactionRestored {
			add { transactionRestored += value; } remove { transactionRestored -= value; }
		}
		public event Action<string> onTransactionStarted {
			add { transactionStarted += value; } remove { transactionStarted -= value; }
		}
		public event Action<string> onTransactionDeferred {
			add { transactionDeferred += value; } remove { transactionDeferred -= value; }
		}
		public event Action<bool> onProductsValidated {
			add { productsValidated += value; } remove { productsValidated -= value; }
		}
		public event Action<string, Int32> onProductsValidationFailed {
			add { productsValidationFailed += value; } remove { productsValidationFailed -= value; }
		}
		public event Action onFinishedRestoring {
			add { finishedRestoring += value; } remove { finishedRestoring -= value; }
		}
		public event Action<string, int> onFinishedRestoringWithError {
			add { finishedRestoringWithError += value; } remove { finishedRestoringWithError -= value; }
		}
		public event Action<string[]> onStartedProductsValidation {
			add { startedProductsValidation += value; } remove { startedProductsValidation -= value; }
		}
		public event Action onStartedRestoring {
			add { startedRestoring += value; } remove { startedRestoring -= value; }
		}

		/// <summary>
		/// 验证所有产品信息.
		/// </summary>
		/// <param name="iProducts">产品列表.</param>
		public void ValidateProducts(string[] iProducts)
		{
			if (iProducts == null) {
				throw new ArgumentNullException ("products", "Array of product identifiers must not be null");
			}
			if (iProducts.Length == 0) {
				throw new ArgumentException ("Array of product identifiers must not be empty", "products");
			}
			startedProductsValidation(iProducts);
			ValidateIAPProducts(iProducts);
		}

		/// <summary>
		/// 开始交易进程.
		/// </summary>
		public void StartProcessing() 
		{
			StartIAPProcessing();
		}

		/// <summary>
		/// 停止交易进程.
		/// </summary>
		public void StopProcessing() 
		{
			StopIAPProcessing();
		}

		/// <summary>
		/// 取得产品验证列表.
		/// </summary>
		/// <returns>产品验证列表.</returns>
		public IEnumerable<IIAPProduct> GetValidatedProducts()
		{
			return _validatedProducts.Values;
		}

		/// <summary>
		/// 取得产品验证数.
		/// </summary>
		/// <returns>产品验证数.</returns>
		public Int32 GetValidatedProductsCount()
		{
			return _validatedProducts.Count;
		}

		/// <summary>
		/// 取得产品验证信息.
		/// </summary>
		/// <returns>产品验证数.</returns>
		/// <param name="iProductId">产品ID.</param>
		public IIAPProduct GetValidatedProduct(string iProductId)
		{
			IIAPProduct result = null;
			_validatedProducts.TryGetValue(iProductId, out result);
			return result;
		}
	
		/// <summary>
		/// 开始交易（数量1）.
		/// </summary>
		/// <param name="iProductId">产品ID.</param>
		public void StartTransaction(string iProductId)
		{
			StartTransaction(iProductId, 1);
		}
			
		/// <summary>
		/// 开始交易.
		/// </summary>
		/// <param name="iProductId">产品ID.</param>
		/// <param name="iQuantity">产品数量.</param>
		public void StartTransaction(string iProductId, Int32 iQuantity)
		{
			if (string.IsNullOrEmpty (iProductId) == true) {
				throw new ArgumentNullException ("product", "Product identifier must not be null");
			}
			if (iQuantity <= 0) {
				throw new ArgumentNullException ("product", "The quantity of product is invalid!!!(<= 0)");
			}
			BuyIAPWithProductID(iProductId, iQuantity);
		}

		/// <summary>
		/// 交易恢复完成.
		/// </summary>
		public void RestoreCompletedTransactions()
		{
			startedRestoring();
			RestoreCompletedIAPTransactions();
		}

		/// <summary>
		/// 结束交易（务必在向玩家交付完商品后，结束）.
		/// </summary>
		/// <param name="iTransactionID">交易ID.</param>
		public void FinalizeTransaction(string iTransactionID)
		{
			if (string.IsNullOrEmpty (iTransactionID) == true) {
				throw new ArgumentNullException ("transactionID", "Transaction identifier must not be null or empty!");
			}
			FinalizeIAPTransaction(iTransactionID);
		}

		/// <summary>
		/// 检测是否能够购买.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		public bool IsEnabled()
		{
			return IsIAPEnabled();
		}

		#region Implementation

		protected static Dictionary<string,IIAPProduct> _validatedProducts = new Dictionary<string,IIAPProduct>();
		protected static Dictionary<string,IIAPProduct> _updatedProducts = new Dictionary<string,IIAPProduct>();

		protected static void finishProductsValidation()
		{
			bool updated = false;
			if (_updatedProducts.Count == _validatedProducts.Count)
			{
				foreach (IIAPProduct product in _updatedProducts.Values)
				{
					if (!_validatedProducts.ContainsKey(product.productIdentifier))
					{
						updated = true;
						break;
					}
				}
			}
			else if (_updatedProducts.Count > 0)
			{
				updated = true;
			}
			if (updated)
			{
				_validatedProducts = _updatedProducts;
			}
			_updatedProducts = new Dictionary<string,IIAPProduct>();
			productsValidated(updated);
		}

		protected virtual void ValidateIAPProducts(string[] products) {}

		protected virtual void StartIAPProcessing() {}

		protected virtual void StopIAPProcessing() {}

		protected virtual void FinalizeIAPTransaction(string id) {}

		protected virtual void BuyIAPWithProductID(string productId, Int32 quantity) {}

		protected virtual bool IsIAPEnabled() { return false; }

		protected virtual void RestoreCompletedIAPTransactions() {}

		protected static Action<string, Int32, string, string> transactionCompleted = (x, y, z, w) => {};
		protected static Action<string, string, int> transactionFailed = (x, y, z) => {};
		protected static Action<string> transactionCancelled = x => {};
		protected static Action<string, Int32, string, string> transactionRestored = (x, y, z, w) => {};
		protected static Action<string> transactionStarted = x => {};
		protected static Action<string> transactionDeferred = x => {};
		protected static Action<bool> productsValidated = x => {};
		protected static Action<string, Int32> productsValidationFailed = (x, y) => {};
		protected static Action finishedRestoring = () => {};
		protected static Action<string, int> finishedRestoringWithError = (x, y) => {};
		protected static Action<string[]> startedProductsValidation = x => {};
		protected static Action startedRestoring = () => {};

		#endregion
	}
}