using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Packages.Common.Editor;
using Packages.Dynamic.Terrains.Morton;
using Packages.Utils;

namespace Packages.Dynamic.Editor
{
	[CustomEditor(typeof(TerrainsSettings))]
	public class TerrainsSettingsEditor : AssetEditorReadOnlyBase<TerrainsSettings, TerrainsData>
	{

		/// <summary>
		/// 是否为2维莫顿码
		/// </summary>
		private bool isMorton2DPreview = false;

		/// <summary>
		/// 是否逆算语言莫顿码
		/// </summary>
		private bool isMortonPreviewDisplay = false;
		private bool isMortonInversePreview = true;
		private uint display2dInverseMorton = 0;
		private Vector2 display2dMorton = Vector2.zero;
		private uint display3dInversedMorton = 0;
		private Vector3 display3dMorton = Vector3.zero;
		
#region Inspector - Title
		
		/// <summary>
		/// 取得地形场景名列表
		/// </summary>
		/// <param name="iDataRootDir"></param>
		/// <returns></returns>
		private string[] GetTerrainsSceneNames(string iDataRootDir)
		{
			var info = new DirectoryInfo(iDataRootDir);
			var subDirs = info.GetDirectories();
			var list = new List<string>();
			for (var i = 0; i < subDirs.Length; ++i)
			{
				list.Add(subDirs[i].Name);
			}
			return list.ToArray();
		}

		/// <summary>
		/// 初始化标题信息.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitTitleInfo(TerrainsSettings iTarget)
		{
			base.InitTitleInfo(iTarget);
			if(!string.IsNullOrEmpty(iTarget.data.DataDir))
			{
				var sceneNames = GetTerrainsSceneNames(iTarget.data.DataDir);
				if (null != sceneNames && 0 < sceneNames.Length)
				{
					DrawSelectList(0, "SceneName", sceneNames, ref iTarget.SceneName);
				}
				else
				{
					DrawLabel(0, "SceneName", Color.red, "No Terrains Data Exist !!");
				}
			}
			
			DrawSingleToggle(0, "莫顿码信息预览", isMortonPreviewDisplay,
				delegate(bool iValue) { isMortonPreviewDisplay = iValue; });
			if (isMortonPreviewDisplay)
			{
				var boolValues = new bool[2];
				boolValues[0] = isMorton2DPreview;
				boolValues[1] = isMortonInversePreview;
				DrawMiddleMultiplyToggle(1, new string[]{"2D/3D", "顺/逆算"}, ref boolValues);
				isMorton2DPreview = boolValues[0];
				isMortonInversePreview = boolValues[1];
			
				// 莫顿码测试预览
				if (isMorton2DPreview)
				{
					if (isMortonInversePreview)
					{
						DrawInverseMorton2DPreView(2, ref display2dInverseMorton);
					}
					else
					{
						DrawMorton2DPreView(2, ref display2dMorton);
					}
				}
				else
				{
					if (isMortonInversePreview)
					{
						DrawInverseMorton3DPreView(1, ref display3dInversedMorton);
					}
					else
					{
						DrawMorton3DPreView(2, ref display3dMorton);
					}
				}
			}
			
		}

		/// <summary>
		/// 初始化顶部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitTopButtons(TerrainsSettings iTarget)
		{

			// 清空按钮
			if(GUILayout.Button("Clear"))
			{
				Clear();
			}
			
			// 导入按钮
			if(GUILayout.Button("Import"))
			{
				Import();
			}

			// 导出按钮
			if(GUILayout.Button("Export"))
			{
				Export();
			}
		}

		/// <summary>
		/// 初始化底部按钮列表.
		/// </summary>
		/// <param name="iTarget">目标信息.</param>
		protected override void InitBottomButtons(TerrainsSettings iTarget) {

			// 清空按钮
			if(GUILayout.Button("Clear"))
			{
				Clear();
			}
			
			// 导入按钮
			if(GUILayout.Button("Import"))
			{
				Import();
			}

			// 导出按钮
			if(GUILayout.Button("Export"))
			{
				Export();
			}
		}

#endregion

#region Inspector - Buttons

		/// <summary>
		/// 导入.
		/// </summary>
		protected override void Import()
		{
			// 导入
			AssetSetting.ImportFromSceneName();
		}

		/// <summary>
		/// 导出.
		/// </summary>
		protected override void Export()
		{
			// 导出
			AssetSetting.ExportBySceneName();
		}

#endregion

#region DrawInspector - Morton

		/// <summary>
		/// 莫顿码逆算预览(2D)
		///  备注
		///     x : x轴方向
		///     y : z轴方向
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iMorton">莫顿码</param>
		private void DrawInverseMorton2DPreView(int iLevel, ref uint iMorton)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			var level = iLevel;
			
			// 莫顿码输入值获取
			var morton = (int) iMorton;
			DrawIntField(level, "莫顿码预览(2D)", ref morton);
			iMorton = (uint)(morton >= 0 ? morton : -1);
			if (0 <= morton)
			{
				// 莫顿码逆算
				var display = Vector2.zero;
				UtilsMorton.InverseMorton2d(ref display.x, ref display.y, iMorton);
				// 显示结果
				DrawVector2Field(level + 1, "反算预览", ref display);
			}
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}
		
		/// <summary>
		/// 莫顿码顺算预览(2D)
		///  备注
		///     x : x轴方向
		///     y : z轴方向
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iMorton">莫顿码</param>
		private void DrawMorton2DPreView(int iLevel, ref Vector2 iMorton)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			var level = iLevel;
			
			// 莫顿码输入值获取
			iMorton.x = iMorton.x >= 0 ? iMorton.x : 0;
			iMorton.y = iMorton.y >= 0 ? iMorton.y : 0;
			DrawVector2Field(level + 1, "预览", ref iMorton);
			var morton = UtilsMorton.Morton2d(iMorton.x, iMorton.y);
			// 莫顿码
			DrawLabel(level, "莫顿码", Color.yellow,morton.ToString());

			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}
		
		/// <summary>
		/// 莫顿码预览(3个纬度)
		///  备注
		///     x : x轴方向
		///     y : 层级
		///     z : z轴方向
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iMorton">莫顿码</param>
		private void DrawInverseMorton3DPreView(int iLevel, ref uint iMorton)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			var level = iLevel;
			
			// 莫顿码输入值获取
			var morton = (int) iMorton;
			DrawIntField(level, "莫顿码预览(3D)", ref morton);
			iMorton = (uint)(morton >= 0 ? morton : -1);
			if (0 <= morton)
			{
				// 莫顿码逆算
				var display = Vector3.zero;
				UtilsMorton.InverseMorton3d(ref display.x, ref display.y, ref display.z, iMorton);
				// 显示结果
				DrawVector3Field(level + 1, "反算预览", ref display);
			}
			
			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}
		
		/// <summary>
		/// 莫顿码顺算预览(3D)
		///  备注
		///     x : x轴方向
		///     y : z轴方向
		/// </summary>
		/// <param name="iLevel">层级(影响到左边缩进的余白)</param>
		/// <param name="iMorton">莫顿码</param>
		private void DrawMorton3DPreView(int iLevel, ref Vector3 iMorton)
		{
			// 设定缩进
			for (var i = 0; i < iLevel; ++i)
			{
				++EditorGUI.indentLevel;
			}
			var level = iLevel;
			
			// 莫顿码输入值获取
			iMorton.x = iMorton.x >= 0 ? iMorton.x : 0;
			iMorton.y = iMorton.y >= 0 ? iMorton.y : 0;
			DrawVector3Field(level + 1, "预览", ref iMorton);
			var morton = UtilsMorton.Morton3d(iMorton.x, iMorton.y, iMorton.z);
			// 莫顿码
			DrawLabel(level, "莫顿码", Color.yellow,morton.ToString());

			// 恢复缩进
			for (var i = 0; i < iLevel; ++i)
			{
				--EditorGUI.indentLevel;
			}
		}

#endregion

#region Create
		
		/// <summary>
		/// 创建打包信息配置文件.
		/// </summary>
		[MenuItem("Assets/Create/TerrainsSettings")]
		static TerrainsSettings MCreate () {	
			return CreateAsset();  
		}
		
#endregion

#region File - Json

		/// <summary>
		/// 从JSON文件导入打包配置信息(TerrainsSettings).
		/// </summary>
		[MenuItem("Assets/TerrainsSettings/File/Json/Import", false, 600)]
		static void MImport() {

			var info = TerrainsSettings.GetInstance ();
			if (null != info) {
				info.ImportFromJsonFile();
			}

			UtilsAsset.AssetsRefresh ();
		}
		
		/// <summary>
		/// 将打包配置信息导出为JSON文件(TerrainsSettings).
		/// </summary>
		[MenuItem("Assets/TerrainsSettings/File/Json/Export", false, 600)]
		static void MExport() {

			var info = TerrainsSettings.GetInstance ();
			if (null != info) {
				info.ExportToJsonFile();
			}

			UtilsAsset.AssetsRefresh ();
		}

		/// <summary>
		/// 清空.
		/// </summary>
		[MenuItem("Assets/TerrainsSettings/Clear", false, 600)]
		static void MClear() {
			var info = TerrainsSettings.GetInstance ();
			if (null != info) {
				info.Clear ();
			}
			UtilsAsset.AssetsRefresh ();
		}

#endregion
	}

}

