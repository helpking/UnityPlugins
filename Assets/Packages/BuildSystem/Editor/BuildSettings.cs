using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using Common;


namespace BuildSystem {

	/// <summary>
	/// XCode工程设定信息类型.
	/// </summary>
	public enum TXCSettingInfoType {
		None,
		ReplaceSource,
		FrameWorks,
		Libraries,
		IncludeFiles,
		IncludeFolders,
		Bool,
		String,
		List
	}

	[System.Serializable]
	public class XCSettingItemValue {
		public TXCBool BValue = TXCBool.No;
		public string SValue = null;
		public List<string> LValue = new List<string>();
	}

	[System.Serializable]
	public class XCSettingItem {
		public XCSettingItem() {
			for (int i = 0; i< Values.Length; ++i) {
				if (Values[i] == null) {
					Values[i] = new XCSettingItemValue ();
				}
			}
		}
		public int No = 0;
		public TXCSettingInfoType Type = TXCSettingInfoType.None; 
		public string Key = null;
		private XCSettingItemValue[] Values = new XCSettingItemValue[((int)TBuildMode.Max)];
		public XCSettingItemValue Value {
			get { 
				return Values[((int)TBuildMode.Release)];
			} 
			set { 
				Values [((int)TBuildMode.Release)] = value;
			}
		}
		public XCSettingItemValue Debug {
			get { 
				return Values[((int)TBuildMode.Debug)];
			} 
			set { 
				Values [((int)TBuildMode.Debug)] = value;
			}
		}
		public XCSettingItemValue Release {
			get { 
				return Values[((int)TBuildMode.Release)];
			} 
			set { 
				Values [((int)TBuildMode.Release)] = value;
			}
		}
		public XCSettingItemValue Store {
			get { 
				return Values[((int)TBuildMode.Store)];
			} 
			set { 
				Values[((int)TBuildMode.Store)] = value;
			}
		}
	}

	/// <summary>
	/// 工程设置器.
	/// </summary>
	public class BuildSettings : AssetBase {

		private static readonly string _assetFileDir = "Assets/Packages/BuildSystem/Conf";
		private static readonly string _jsonFileDir = string.Format("{0}/Json",_assetFileDir);

		/// <summary>
		/// XCode工程设定情报.
		/// </summary>
		[SerializeField]
		public List<XCodeSettings> XCodeSeetings = new List<XCodeSettings> ();

		private int XCSettingItemNo = 0;

		/// <summary>
		/// 实例.
		/// </summary>
		private static BuildSettings _instance = null;

		/// <summary>
		/// 取得实例.
		/// </summary>
		/// <returns>实例.</returns>
		public static BuildSettings GetInstance() {

			if (_instance == null) {
				_instance = UtilityAsset.Read<BuildSettings>(_assetFileDir);
				if (_instance == null) {
					Debug.LogError ("BuildSettings GetInstance Failed!!!");
					return null;
				}
				_instance.Init ();
			}
			return _instance;
		}

		private XCodeSettings GetXCodeSetting(string iTargetName) {
		
			XCodeSettings settings = null;
			foreach (XCodeSettings loop in XCodeSeetings) {
				if (iTargetName.Equals (loop.Target) == false) {
					continue;
				}
				settings = loop;
				break;
			}
			return settings;
		}

		/// <summary>
		/// 清空(XCode).
		/// </summary>
		public void XCodeClear() {

			foreach (XCodeSettings loop in XCodeSeetings) {
				loop.Clear();
			}
			XCodeSeetings.Clear ();

			// 清空列表
			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 重置(XCode).
		/// </summary>
		public void XCodeReset() {

			foreach (XCodeSettings loop in XCodeSeetings) {
				loop.Reset();
			}

			// 清空列表
			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 取得导出XCode工程用的设定信息一览.
		/// </summary>
		/// <returns>导出XCode工程用的设定信息一览.</returns>
		/// <param name="iProjectName">工程名.</param>
		public XCSettingItem[] GetXCSettingInfo(string iProjectName) {
			List<XCSettingItem> objRet = null;
			XCodeSettings xcode = this.GetXCodeSetting (iProjectName);
			if (xcode == null) {
				return null;
			}
			objRet = new List<XCSettingItem> ();
			this.XCSettingItemNo = 0;

			// Replace Targets
			if(xcode.Settings.ReplaceTargets != null) {
				XCSettingItem item = this.CreateXCSettingItem(
					TXCSettingInfoType.ReplaceSource, xcode.Settings.ReplaceTargets);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// FrameWorks
			if(xcode.Settings.FrameWorks != null) {
				XCSettingItem item = this.CreateXCSettingItem(
					TXCSettingInfoType.FrameWorks, xcode.Settings.FrameWorks);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// libraries
			if(xcode.Settings.Libraries != null) {
				XCSettingItem item = this.CreateXCSettingItem(
					TXCSettingInfoType.Libraries, xcode.Settings.Libraries);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// IncludeFiles
			if(xcode.Settings.IncludeFiles != null) {
				XCSettingItem item = this.CreateXCSettingItem(
					TXCSettingInfoType.IncludeFiles, xcode.Settings.IncludeFiles);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// IncludeFolders
			if(xcode.Settings.IncludeFiles != null) {
				XCSettingItem item = this.CreateXCSettingItem(
					TXCSettingInfoType.IncludeFolders, xcode.Settings.IncludeFolders);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// Linking
			if (xcode.Settings.Linking != null) {

				// OtherLinkerFlags
				if (xcode.Settings.Linking.OtherLinkerFlags != null) {
					XCSettingItem item = this.CreateXCSettingItem(
						TXCSettingInfoType.List, 
						xcode.Settings.Linking.OtherLinkerFlags.Key,
						xcode.Settings.Linking.OtherLinkerFlags.Debug, 
						xcode.Settings.Linking.OtherLinkerFlags.Release);
					if (item != null) {
						objRet.Add (item);
					}
				}
			}

			// Language
			if (xcode.Settings.Language != null) {
				// C++
				if (xcode.Settings.Language.Cpp != null) {
					// Enable C++ Exceptions
					if (xcode.Settings.Language.Cpp.EnableCppExceptions != null) {
						XCSettingItem item = this.CreateXCSettingItem(
							TXCSettingInfoType.Bool, 
							xcode.Settings.Language.Cpp.EnableCppExceptions.Key,
							xcode.Settings.Language.Cpp.EnableCppExceptions.Debug, 
							xcode.Settings.Language.Cpp.EnableCppExceptions.Release);
						if (item != null) {
							objRet.Add (item);
						}
					}
				}

				// Objective-C
				if (xcode.Settings.Language.ObjectC != null) {
					
					// Objective-C Automatic Reference Counting
					if (xcode.Settings.Language.ObjectC.AutomaticReferenceCounting != null) {
						XCSettingItem item = this.CreateXCSettingItem(
							TXCSettingInfoType.Bool, 
							xcode.Settings.Language.ObjectC.AutomaticReferenceCounting.Key,
							xcode.Settings.Language.ObjectC.AutomaticReferenceCounting.Debug, 
							xcode.Settings.Language.ObjectC.AutomaticReferenceCounting.Release);
						if (item != null) {
							objRet.Add (item);
						}
					}
				}
			}

			// Deployment
			if (xcode.Settings.Deployment != null) {
				
				// Deployment Postprocessing
				if (xcode.Settings.Deployment.DeploymentPostprocessing != null) {
					XCSettingItem item = this.CreateXCSettingItem(
						TXCSettingInfoType.Bool, 
						xcode.Settings.Deployment.DeploymentPostprocessing.Key,
						xcode.Settings.Deployment.DeploymentPostprocessing.Debug, 
						xcode.Settings.Deployment.DeploymentPostprocessing.Release);
					if (item != null) {
						objRet.Add (item);
					}
				}
			}

			// Build Options
			if (xcode.Settings.BuildOptions != null) {
				// Enable BitCode
				if (xcode.Settings.BuildOptions.EnableBitCode != null) {
					XCSettingItem item = this.CreateXCSettingItem(
						TXCSettingInfoType.Bool, 
						xcode.Settings.BuildOptions.EnableBitCode.Key,
						xcode.Settings.BuildOptions.EnableBitCode.Debug, 
						xcode.Settings.BuildOptions.EnableBitCode.Release);
					if (item != null) {
						objRet.Add (item);
					}
				}
			}

			// Signing
			if (xcode.Settings.Signing != null) {
				// Development Team
				if (xcode.Settings.Signing.DevelopmentTeam != null) {
					XCSettingItem item = this.CreateXCSettingItem(
						TXCSettingInfoType.String, 
						xcode.Settings.Signing.DevelopmentTeam.Key,
						xcode.Settings.Signing.DevelopmentTeam.Debug, 
						xcode.Settings.Signing.DevelopmentTeam.Release);
					if (item != null) {
						objRet.Add (item);
					}
				}
			}

			// User-Defined
			if (xcode.Settings.UserDefined != null) {
			}

			// Other Setting
			if(xcode.Settings.OtherSetting != null) {

				// Bool Targets
				foreach(XCBoolItem boolItem in xcode.Settings.OtherSetting.BoolTargets) {
					XCSettingItem item = this.CreateXCSettingItem(
						TXCSettingInfoType.Bool, 
						boolItem.Key, boolItem.Debug, boolItem.Release);
					if (item != null) {
						objRet.Add (item);
					}
				}

				// String Targets
				foreach(XCStringItem stringItem in xcode.Settings.OtherSetting.StringTargets) {
					XCSettingItem item = this.CreateXCSettingItem(
						TXCSettingInfoType.String, 
						stringItem.Key, stringItem.Debug, stringItem.Release);
					if (item != null) {
						objRet.Add (item);
					}
				}

				// List Targets
				foreach(XCListItem listItem in xcode.Settings.OtherSetting.ListTargets) {
					XCSettingItem item = this.CreateXCSettingItem(
						TXCSettingInfoType.List, 
						listItem.Key, listItem.Debug, listItem.Release);
					if (item != null) {
						objRet.Add (item);
					}
				}
			}

			if ((objRet == null) || (objRet.Count <= 0)) {
				return null;
			} else {
				return objRet.OrderBy (o => o.No).ToArray ();
			}
		}



		private XCSettingItem CreateXCSettingItem(TXCSettingInfoType iType) {
			XCSettingItem item = new XCSettingItem ();
			if (item == null) {
				return null;
			}
			++this.XCSettingItemNo;
			item.No = this.XCSettingItemNo;
			item.Type = iType;
			return item;
		}

		private XCSettingItem CreateXCSettingItem(TXCSettingInfoType iType, string iKey) {
			XCSettingItem item = CreateXCSettingItem (iType);
			if (item == null) {
				return null;
			}
			item.Key = iKey;
			return item;
		}
			
		private XCSettingItem CreateXCSettingItem(TXCSettingInfoType iType, string iKey, TXCBool iDebug, TXCBool iRelease) {
			XCSettingItem item = CreateXCSettingItem (iType, iKey);
			if (item == null) {
				return null;
			}
			item.Debug.BValue = iDebug;
			item.Release.BValue = iRelease;
			return item;
		}

		private XCSettingItem CreateXCSettingItem(TXCSettingInfoType iType, string iKey, string iDebug, string iRelease) {
			XCSettingItem item = CreateXCSettingItem (iType, iKey);
			if (item == null) {
				return null;
			}
			item.Debug.SValue = iDebug;
			item.Release.SValue = iRelease;
			return item;
		}

		private XCSettingItem CreateXCSettingItem(TXCSettingInfoType iType, List<string> iValue) {
			XCSettingItem item = CreateXCSettingItem (iType);
			if (item == null) {
				return null;
			}
			item.Value.LValue = iValue;
			return item;
		}

		private XCSettingItem CreateXCSettingItem(TXCSettingInfoType iType, string iKey, List<string> iDebug, List<string> iRelease) {
			XCSettingItem item = CreateXCSettingItem (iType, iKey);
			if (item == null) {
				return null;
			}
			item.Debug.LValue = iDebug;
			item.Release.LValue = iRelease;
			return item;
		}

		#region Implement

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init () {

			//			UtilityAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		protected override void ApplyData(AssetBase iData) {
			if (iData == null) {
				return;
			}

			BuildSettings data = iData as BuildSettings;
			if (data == null) {
				return;
			}
			// 清空
			this.Clear ();
			this.XCodeSeetings.AddRange (data.XCodeSeetings);

			// 添加以后信息

			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {

			UtilityAsset.Clear<BuildSettings> ();

			if (this.XCodeSeetings != null) {
				this.XCodeSeetings.Clear ();
			}

			// 清空列表
			UtilityAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 从JSON文件，导入打包配置信息.
		/// </summary>
		public override void ImportFromJsonFile() {			

			BuildSettings jsonData = UtilityAsset.ImportFromJsonFile<BuildSettings> (_jsonFileDir);
			if (jsonData != null) {
				this.ApplyData (jsonData);
			}
		}

		/// <summary>
		/// 导出成JSON文件.
		/// </summary>
		/// <returns>导出路径.</returns>
		public override string ExportToJsonFile() {
			if (Directory.Exists (_jsonFileDir) == false) {
				Directory.CreateDirectory (_jsonFileDir);
			}
			return UtilityAsset.ExportToJsonFile<BuildSettings> (this, _jsonFileDir);
		}

		#endregion
	}
}
