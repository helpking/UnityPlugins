using System;
using System.Collections.Generic;
using System.Linq;
using Packages.Common.Base;
using Packages.Settings;
using Packages.Utils;
using Packages.BuildSystem.iOS;
using SObject = System.Object;

namespace Packages.BuildSystem.Settings {

	/// <summary>
	/// XCode工程设定信息类型.
	/// </summary>
	public enum TxcSettingInfoType {
		None,
		ReplaceSource,
		FrameWorks,
		Libraries,
		IncludeFiles,
		IncludeFolders,
		/// <summary>
		/// 布尔型
		/// </summary>
		Bool,
		/// <summary>
		/// 枚举型
		/// </summary>
		Enum,
		/// <summary>
		/// 字符串型
		/// </summary>
		String,
		/// <summary>
		/// 列表类型
		/// </summary>
		List
	}

	public interface IXcSettingItemValue
	{
		SObject Value { get; set; }
		SObject[] Values { get; set; }
	}

	[System.Serializable]
	public class XcSettingItemValue<T> : IXcSettingItemValue {
		protected T _value;

		public SObject Value
		{
			get { return _value as SObject; }
			set { _value = (T)value; }
		}
		
		public virtual SObject[] Values { get; set; }
	}

	public class XcSettingListItemValue : XcSettingItemValue<List<string>>
	{
		protected string[] _values;
		
		public override SObject[] Values
		{
			get { return _values; }
			set
			{
				var length = 0;
				if (null != value && 0 < value.Length)
				{
					length = value.Length;
				}
				_values = new string[length];
				for (var i = 0; i < length; ++i)
				{
					_values[i] = Convert.ToString(value[i]);
				}
			}
		}
	}

	public interface IXcSettingItem
	{
		int no { get; set; }
		string key { get; set; }
		TxcSettingInfoType type { get; set; }
		IXcSettingItemValue Value { get; set; }
		IXcSettingItemValue Debug { get; set; }
		IXcSettingItemValue Release { get; set; }
		IXcSettingItemValue ReleaseForProfiling { get; set; }
		IXcSettingItemValue ReleaseForRunning { get; set; }
		
		/// <summary>
		/// 保留对象：暂时不是使用
		/// </summary>
		IXcSettingItemValue Production { get; set; }
	}

	[Serializable]
	public class XcSettingItem<T> : IXcSettingItem {
		public XcSettingItem() {
			for (var _ = 0; _< _values.Length; ++_) {
				if (_values[_] == null) {
					_values[_] = new XcSettingItemValue<T> ();
				}
			}
		}
		
		protected readonly XcSettingItemValue<T>[] _values = new XcSettingItemValue<T>[(int)BuildMode.Max];

		public int no { get; set; }
		public string key { get; set; }
		public TxcSettingInfoType type { get; set; }
		
		public IXcSettingItemValue Value {
			get { 
				return _values[(int)BuildMode.Release];
			} 
			set { 
				_values [(int)BuildMode.Release] = value as XcSettingItemValue<T>;
			}
		}
		
		public IXcSettingItemValue Debug {
			get { 
				return _values[(int)BuildMode.Debug];
			} 
			set { 
				_values [(int)BuildMode.Debug] = value as XcSettingItemValue<T>;
			}
		}
		
		public IXcSettingItemValue Release {
			get { 
				return _values[(int)BuildMode.Release];
			} 
			set { 
				_values [(int)BuildMode.Release] = value as XcSettingItemValue<T>;
			}
		}
		
		public IXcSettingItemValue ReleaseForProfiling {
			get { 
				return _values[(int)BuildMode.ReleaseForProfiling];
			} 
			set { 
				_values [(int)BuildMode.ReleaseForProfiling] = value as XcSettingItemValue<T>;
			}
		}
		
		public IXcSettingItemValue ReleaseForRunning {
			get { 
				return _values[(int)BuildMode.ReleaseForRunning];
			} 
			set { 
				_values [(int)BuildMode.ReleaseForRunning] = value as XcSettingItemValue<T>;
			}
		}
		
		public IXcSettingItemValue Production {
			get { 
				return _values[(int)BuildMode.Production];
			} 
			set { 
				_values[(int)BuildMode.Production] = value as XcSettingItemValue<T>;
			}
		}
	}

	[Serializable]
	public class XcSettingListItem : XcSettingItem<List<string>>
	{
		public XcSettingListItem() : base() {
			for (var _ = 0; _< _values.Length; ++_) {
				_values[_] = new XcSettingListItemValue ();
			}
		}
	}

	/// <summary>
	/// 打包设定数据.
	/// </summary>
	[System.Serializable]
	public class BuildSettingsData : JsonDataBase<BuildSettingsData> {

		/// <summary>
		/// XCode工程设定情报.
		/// </summary>
		public List<XCodeSettings> xCodeSettings = new List<XCodeSettings> ();

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init()
		{
			xCodeSettings?.Clear();
		}

		/// <summary>
		/// 重置.
		/// </summary>
		public override void Reset() {}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			Init ();
		}

	}

	/// <summary>
	/// 工程设置器.
	/// </summary>
	public class BuildSettings : AssetBase<BuildSettings, BuildSettingsData> {
		public const string AssetFileDir = "Assets/Packages/BuildSystem/Editor/Conf";

		public BuildSettings() {
			_path = AssetFileDir;
		}

		/// <summary>
		/// XCode工程设定情报.
		/// </summary>
		public List<XCodeSettings> XCodeSettings => data?.xCodeSettings;

		private int _xcSettingItemNo;

		private XCodeSettings GetXCodeSetting(string iTargetName) {
		
			XCodeSettings settings = null;
			foreach (var loop in XCodeSettings) {
				if (iTargetName.Equals (loop.xcodeSchema) == false) {
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

			foreach (var loop in XCodeSettings) {
				loop.Clear();
			}
			XCodeSettings.Clear ();

			// 清空列表
			UtilsAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 重置(XCode).
		/// </summary>
		public void XCodeReset() {

			foreach (var loop in XCodeSettings) {
				loop.Reset();
			}

			// 清空列表
			UtilsAsset.SetAssetDirty (this);

		}
		
		/// <summary>
		/// 取得导入路径.
		/// </summary>
		/// <returns>导入路径.</returns>
		public override string GetImportPath () {
			return $"{AssetFileDir}/Json";
		}

		/// <summary>
		/// 取得导出路径.
		/// </summary>
		/// <returns>导出路径.</returns>
		public override string GetExportPath () {
			return $"{AssetFileDir}/Json";
		}

		/// <summary>
		/// 取得导出XCode工程用的设定信息一览.
		/// </summary>
		/// <returns>导出XCode工程用的设定信息一览.</returns>
		/// <param name="iProjectName">工程名.</param>
		public IXcSettingItem[] GetXcSettingInfo(string iProjectName) {
			var xcode = GetXCodeSetting (iProjectName);
			if (xcode == null) {
				return null;
			}
			var objRet = new List<IXcSettingItem> ();
			_xcSettingItemNo = 0;

			// Replace Targets
			if(xcode.settings.replaceTargets != null) {
				var item = CreateXcSettingItem(
					TxcSettingInfoType.ReplaceSource, xcode.settings.replaceTargets);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// FrameWorks
			if(xcode.settings.frameWorks != null) {
				var item = CreateXcSettingItem(
					TxcSettingInfoType.FrameWorks, xcode.settings.frameWorks);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// libraries
			if(xcode.settings.libraries != null) {
				var item = CreateXcSettingItem(
					TxcSettingInfoType.Libraries, xcode.settings.libraries);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// IncludeFiles
			if(xcode.settings.includeFiles != null) {
				var item = CreateXcSettingItem(
					TxcSettingInfoType.IncludeFiles, xcode.settings.includeFiles);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// IncludeFolders
			if(xcode.settings.includeFiles != null) {
				var item = CreateXcSettingItem(
					TxcSettingInfoType.IncludeFolders, xcode.settings.includeFolders);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// Linking
			// OtherLinkerFlags
			if (xcode.settings.linking?.otherLinkerFlags != null && xcode.settings.linking.otherLinkerFlags.Ignore == false) {
				var item = CreateXcSettingItem(
					TxcSettingInfoType.List, 
					xcode.settings.linking.otherLinkerFlags.Key,
					xcode.settings.linking.otherLinkerFlags.Debug, 
					xcode.settings.linking.otherLinkerFlags.Release, 
					xcode.settings.linking.otherLinkerFlags.ReleaseForProfiling, 
					xcode.settings.linking.otherLinkerFlags.ReleaseForRunning);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// Language
			if (xcode.settings.language != null && xcode.settings.language.Ignore == false) {
				// C++
				// Enable C++ Exceptions
				if (xcode.settings.language.cpp?.enableCppExceptions != null && xcode.settings.language.cpp.enableCppExceptions.Ignore == false) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.Bool, 
						xcode.settings.language.cpp.enableCppExceptions.Key,
						xcode.settings.language.cpp.enableCppExceptions.Debug, 
						xcode.settings.language.cpp.enableCppExceptions.Release, 
						xcode.settings.language.cpp.enableCppExceptions.ReleaseForProfiling, 
						xcode.settings.language.cpp.enableCppExceptions.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}

				// Objective-C
				// Objective-C Automatic Reference Counting
				if (xcode.settings.language.objectC?.automaticReferenceCounting != null && xcode.settings.language.objectC.automaticReferenceCounting.Ignore == false) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.Bool, 
						xcode.settings.language.objectC.automaticReferenceCounting.Key,
						xcode.settings.language.objectC.automaticReferenceCounting.Debug, 
						xcode.settings.language.objectC.automaticReferenceCounting.Release, 
						xcode.settings.language.objectC.automaticReferenceCounting.ReleaseForProfiling, 
						xcode.settings.language.objectC.automaticReferenceCounting.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}
			}

			// Deployment
			// Deployment Postprocessing
			if (xcode.settings.deployment?.deploymentPostprocessing != null && xcode.settings.deployment.deploymentPostprocessing.Ignore == false) {
				var item = CreateXcSettingItem(
					TxcSettingInfoType.Bool, 
					xcode.settings.deployment.deploymentPostprocessing.Key,
					xcode.settings.deployment.deploymentPostprocessing.Debug, 
					xcode.settings.deployment.deploymentPostprocessing.Release, 
					xcode.settings.deployment.deploymentPostprocessing.ReleaseForProfiling, 
					xcode.settings.deployment.deploymentPostprocessing.ReleaseForRunning);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// Build Options
			// Enable BitCode
			if (xcode.settings.buildOptions?.enableBitCode != null && xcode.settings.buildOptions.enableBitCode.Ignore == false) {
				var item = CreateXcSettingItem(
					TxcSettingInfoType.Bool, 
					xcode.settings.buildOptions.enableBitCode.Key,
					xcode.settings.buildOptions.enableBitCode.Debug, 
					xcode.settings.buildOptions.enableBitCode.Release, 
					xcode.settings.buildOptions.enableBitCode.ReleaseForProfiling, 
					xcode.settings.buildOptions.enableBitCode.ReleaseForRunning);
				if (item != null) {
					objRet.Add (item);
				}
			}

			// Signing - automatic
			if (xcode.settings.signing != null && xcode.settings.signing.Ignore == false)
			{
				// Code Sign Identity
				if (xcode.settings.signing.codeSignIdentity != null &&
				    xcode.settings.signing.codeSignIdentity.Ignore == false) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.String, 
						xcode.settings.signing.codeSignIdentity.Key,
						xcode.settings.signing.codeSignIdentity.Debug, 
						xcode.settings.signing.codeSignIdentity.Release, 
						xcode.settings.signing.codeSignIdentity.ReleaseForProfiling, 
						xcode.settings.signing.codeSignIdentity.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}
				
				// Code Sign Style
				if (xcode.settings.signing.codeSignStyle != null &&
				    xcode.settings.signing.codeSignStyle.Ignore == false) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.Enum, 
						xcode.settings.signing.codeSignStyle.Key,
						xcode.settings.signing.codeSignStyle.Debug, 
						xcode.settings.signing.codeSignStyle.Release, 
						xcode.settings.signing.codeSignStyle.ReleaseForProfiling, 
						xcode.settings.signing.codeSignStyle.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}
				
				// Provisioning Profile
				if (xcode.settings.signing.provisioningProfile != null &&
				    xcode.settings.signing.provisioningProfile.Ignore == false) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.String, 
						xcode.settings.signing.provisioningProfile.Key,
						xcode.settings.signing.provisioningProfile.Debug, 
						xcode.settings.signing.provisioningProfile.Release, 
						xcode.settings.signing.provisioningProfile.ReleaseForProfiling, 
						xcode.settings.signing.provisioningProfile.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}
				
				// Development Team
				if (xcode.settings.signing.developmentTeam != null &&
				    xcode.settings.signing.developmentTeam.Ignore == false) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.String, 
						xcode.settings.signing.developmentTeam.Key,
						xcode.settings.signing.developmentTeam.Debug, 
						xcode.settings.signing.developmentTeam.Release, 
						xcode.settings.signing.developmentTeam.ReleaseForProfiling, 
						xcode.settings.signing.developmentTeam.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}
			}

			// User-Defined
			if (xcode.settings.userDefined != null && xcode.settings.userDefined.Ignore == false) {
			}

			// Other Setting
			if(xcode.settings.otherSetting != null && xcode.settings.otherSetting.Ignore == false) {

				// Bool Targets
				foreach(var boolItem in xcode.settings.otherSetting.boolTargets) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.Bool, 
						boolItem.Key, boolItem.Debug, boolItem.Release, 
						boolItem.ReleaseForProfiling, boolItem.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}

				// String Targets
				foreach(var stringItem in xcode.settings.otherSetting.stringTargets) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.String, 
						stringItem.Key, stringItem.Debug, stringItem.Release, 
						stringItem.ReleaseForProfiling, stringItem.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}

				// List Targets
				foreach(var listItem in xcode.settings.otherSetting.listTargets) {
					var item = CreateXcSettingItem(
						TxcSettingInfoType.List, 
						listItem.Key, listItem.Debug, listItem.Release,
						listItem.ReleaseForProfiling, listItem.ReleaseForRunning);
					if (item != null) {
						objRet.Add (item);
					}
				}
			}

			if (objRet == null || objRet.Count <= 0) {
				return null;
			}

			return objRet.OrderBy (iO => iO.no).ToArray ();
		}
			
		private IXcSettingItem CreateXcSettingItem<T>(TxcSettingInfoType iType) {
			var item = new XcSettingItem<T> ();
			++_xcSettingItemNo;
			item.no = _xcSettingItemNo;
			item.type = iType;
			return item;
		}
		
		private IXcSettingItem CreateXcSettingListItem(TxcSettingInfoType iType) {
			var item = new XcSettingListItem ();
			++_xcSettingItemNo;
			item.no = _xcSettingItemNo;
			item.type = iType;
			return item;
		}

		private IXcSettingItem CreateXcSettingItem<T>(TxcSettingInfoType iType, string iKey) {
			var item = CreateXcSettingItem<T> (iType);
			if (item == null) {
				return null;
			}
			item.key = iKey;
			return item;
		}
			
		private IXcSettingItem CreateXcSettingItem<T>(
			TxcSettingInfoType iType, string iKey, 
			T iDebug, T iRelease,
			T iReleaseForProfiling, T iReleaseForRunning) {
			var item = CreateXcSettingItem<T> (iType, iKey);
			if (item == null) {
				return null;
			}
			item.Debug.Value = iDebug;
			item.Release.Value = iRelease;
			item.ReleaseForProfiling.Value = iReleaseForProfiling;
			item.ReleaseForRunning.Value = iReleaseForRunning;
			return item;
		}

		private IXcSettingItem CreateXcSettingItem(TxcSettingInfoType iType, List<string> iValue) {
			var item = CreateXcSettingListItem (iType);
			if (item == null) {
				return null;
			}

			if (null == iValue || 0 >= iValue.Count)
			{
				return null;
			}
			
			item.Value.Values = iValue.ToArray();
			return item;
		}

#region Implement

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空.</param>
		protected override void ApplyData(BuildSettingsData iData, bool iForceClear = true) {
			if (iData == null) {
				return;
			}

			// 清空
			if (iForceClear) {
				Clear ();
			}
			XCodeSettings.AddRange (iData.xCodeSettings);

			// 添加以后信息

			UtilsAsset.SetAssetDirty (this);

		}

#endregion
	}
}
