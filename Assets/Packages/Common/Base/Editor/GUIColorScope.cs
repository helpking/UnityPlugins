using UnityEngine;

namespace Packages.Common.Base.Editor
{

	/// <summary>
	/// GUI颜色
	///
	/// </summary>
	public class ColorScope : GUI.Scope
	{
		private readonly Color color;
		public ColorScope(Color color)
		{
			this.color = GUI.color;
			GUI.color = color;
		}


		protected override void CloseScope()
		{
			GUI.color = color;
		}
	}

	/// <summary>
	/// GUI颜色背景区域
	///
	/// 注意：对文本无效
	/// </summary>
	public class BackgroundColorScope : GUI.Scope
	{
		private readonly Color color;
		public BackgroundColorScope(Color color)
		{
			this.color = GUI.backgroundColor;
			GUI.backgroundColor = color;
		}


		protected override void CloseScope()
		{
			GUI.backgroundColor = color;
		}
	}

}


