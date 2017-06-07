namespace IAP
{
	/// <summary>
	/// 产品接口定义.
	/// </summary>
	public interface IIAPProduct 
	{
		/// <summary>
		/// 标题.
		/// </summary>
		string title {get; }	
		/// <summary>
		/// 描述.
		/// </summary>
		string description {get; }
		/// <summary>
		/// 产品ID.
		/// </summary>
		string productIdentifier {get; }
		/// <summary>
		/// 价格.
		/// </summary>
		decimal price {get; }
		/// <summary>
		/// 价格描述.
		/// </summary>
		string priceAsString {get; }
		/// <summary>
		/// 货币符号.
		/// </summary>
		string currencySymbol {get; }
		/// <summary>
		/// 货币代码.
		/// </summary>
		string currencyCode {get; }
		/// <summary>
		/// 本地ID.
		/// </summary>
		string localeIdentifier{get; }
		/// <summary>
		/// 国家代码.
		/// </summary>
		string countryCode {get;}
	}

	/// <summary>
	/// 产品定义.
	/// </summary>
	public class Product : IIAPProduct
	{
		/// <summary>
		/// 标题.
		/// </summary>
		public string title {get; set; }	
		/// <summary>
		/// 描述.
		/// </summary>
		public string description {get; set; }
		/// <summary>
		/// 产品ID.
		/// </summary>
		public string productIdentifier {get; set; }	
		/// <summary>
		/// 价格.
		/// </summary>
		public decimal price {get; set; }
		/// <summary>
		/// 价格描述.
		/// </summary>
		public string priceAsString {get; set; }
		/// <summary>
		/// 货币符号.
		/// </summary>
		public string currencySymbol {get; set; }
		/// <summary>
		/// 货币代码.
		/// </summary>
		public string currencyCode {get; set; }
		/// <summary>
		/// 本地ID.
		/// </summary>
		public string localeIdentifier{get; set; }
		/// <summary>
		/// 国家代码.
		/// </summary>
		public string countryCode {get; set; }

		public override bool Equals(System.Object righthand)
		{
			if (righthand.GetType() == this.GetType()) 
			{
				Product other = (Product)righthand;
				if (other == null) {
					return false;
				}
				return this.description == other.description &&
					this.title == other.title &&
					this.productIdentifier == other.productIdentifier &&
					this.price == other.price &&
					this.priceAsString == other.priceAsString &&
					this.currencySymbol == other.currencySymbol &&
					this.currencyCode == other.currencyCode &&
					this.localeIdentifier == other.localeIdentifier &&
					this.countryCode == other.countryCode;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return productIdentifier != null ? productIdentifier.GetHashCode() : 0;
		}
	}
}