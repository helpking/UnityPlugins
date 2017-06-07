using System;
using System.Collections.Generic;

namespace IAP
{
	public interface IIAP
	{
		/// <summary>
		/// Called when thr transaction succeeds. Parameters are product id, item quantity, transaction id, and base64-encoded receipt.
		/// </summary>
		event Action<string, Int32, string, string> onTransactionCompleted;
		/// <summary>
		/// Called when the transaction fails. Parameters are product id, error message and error code.
		/// </summary>
		event Action<string, string, int> onTransactionFailed;
		event Action<string> onTransactionCancelled;
		event Action<string, Int32, string, string> onTransactionRestored;
		/// <summary>
		/// Occurs when the transaction is being initialised. Parameter is product id.
		/// </summary>
		event Action<string> onTransactionStarted;
		/// <summary>
		/// Occurs when the transaction has been deferred. Do not expect a transaction result soon. Parameter is product id.
		/// </summary>
		event Action<string> onTransactionDeferred;
		/// <summary>
		/// Called when the products list has finished validating. Parameter equals true when validated products list has been changed.
		/// </summary>
		event Action<bool> onProductsValidated;
		/// <summary>
		/// Called when the products list received an error validating products. Parameter contains error message and error code.
		/// </summary>
		event Action<string, Int32> onProductsValidationFailed;
		/// <summary>
		/// Called when the store successfully finished restoring previously made purchases.
		/// </summary>
		event Action onFinishedRestoring;
		/// <summary>
		/// Called when there was an error restoring previously made purchases. Parameters are error message and error code.
		/// </summary>
		event Action<string, int> onFinishedRestoringWithError;
		event Action<string[]> onStartedProductsValidation;
		event Action onStartedRestoring;

		/// <summary>
		/// 开始交易进程.
		/// </summary>
		void StartProcessing();

		/// <summary>
		/// 停止交易进程.
		/// </summary>
		void StopProcessing();

		/// <summary>
		/// 验证所有产品信息.
		/// </summary>
		/// <param name="iProducts">产品列表.</param>
		void ValidateProducts(string[] products);

		/// <summary>
		/// 取得产品验证列表.
		/// </summary>
		/// <returns>产品验证列表.</returns>
		IEnumerable<IIAPProduct> GetValidatedProducts();

		/// <summary>
		/// 取得产品验证数.
		/// </summary>
		/// <returns>产品验证数.</returns>
		Int32 GetValidatedProductsCount();

		/// <summary>
		/// 取得产品验证信息.
		/// </summary>
		/// <returns>产品验证数.</returns>
		/// <param name="iProductId">产品ID.</param>
		IIAPProduct GetValidatedProduct(string productId);

		/// <summary>
		/// 开始交易（数量1）.
		/// </summary>
		/// <param name="iProductId">产品ID.</param>
		void StartTransaction(string product);

		/// <summary>
		/// 开始交易.
		/// </summary>
		/// <param name="iProductId">产品ID.</param>
		/// <param name="iQuantity">产品数量.</param>
		void StartTransaction(string product, Int32 quantity);

		/// <summary>
		/// 交易恢复完成.
		/// </summary>
		void RestoreCompletedTransactions();

		/// <summary>
		/// 结束交易（务必在向玩家交付完商品后，结束）.
		/// </summary>
		/// <param name="iTransactionID">交易ID.</param>
		void FinalizeTransaction(string transactionID);

		/// <summary>
		/// 检测是否能够购买.
		/// </summary>
		/// <returns><c>true</c> 能够购买; 不能购买, <c>false</c>.</returns>
		bool IsEnabled();
	}
}

