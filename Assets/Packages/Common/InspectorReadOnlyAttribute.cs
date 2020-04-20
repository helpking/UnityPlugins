using UnityEngine;

namespace Packages.Common {

	/// <summary>
	/// 只读属性定义.
	/// </summary>
	public class InspectorReadOnlyAttribute : PropertyAttribute {
		
		public Color TextColor;

		public InspectorReadOnlyAttribute()
		{
			TextColor = Color.white;
		}
		
		public InspectorReadOnlyAttribute(Color iColor)
		{
			TextColor = iColor;
		}

		public InspectorReadOnlyAttribute(float iR, float iG, float iB)
		{
			TextColor = new Color(iB, iG, iB);
		}
		
		public InspectorReadOnlyAttribute(float iR, float iG, float iB, float iA)
		{
			TextColor = new Color(iB, iG, iB, iA);
		}
	}
}
