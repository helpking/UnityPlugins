using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Common {

	#region XCObjectBase

	public enum TXCBool {
		Yes,
		No
	}

	public abstract class XCObjectBase {
		public XCObjectBase() {
			this.Init ();
		}
		public virtual void Init () { }

		public abstract void Clear ();
		public abstract void Reset () ;
	}

	public abstract class XCObjectItem<T> : System.Object  {
		public string Key = null;
		public T Debug = default(T);
		public T Release = default(T);


		protected virtual void InitItem(string iKey, T iDebugValue, T iReleaseValue){
			this.Key = iKey;
			this.Debug = iDebugValue;
			this.Release = iReleaseValue;
		}

		public virtual void Clear() {
			Debug = default(T);
			Release = default(T);
		}
		public abstract void Reset ();
	}

	[System.Serializable]
	public class XCBoolItem : XCObjectItem<TXCBool>  {
		protected override void InitItem (string iKey, TXCBool iDebugValue, TXCBool iReleaseValue)
		{
			base.InitItem (iKey, iDebugValue, iReleaseValue);
		}
		public override void Reset () {
			this.InitItem (this.Key, TXCBool.No, TXCBool.No);
		}
	}

	[System.Serializable]
	public class XCStringItem : XCObjectItem<string>  {
		protected override void InitItem (string iKey, string iDebugValue, string iReleaseValue)
		{
			base.InitItem (iKey, iDebugValue, iReleaseValue);
		}
		public override void Reset () {
			this.InitItem (this.Key, null, null);
		}
	}

	[System.Serializable]
	public class XCListItem : XCObjectItem<List<string>>  {
		
		protected override void InitItem(string iKey, List<string> iDebugValue, List<string> iReleaseValue){
			base.InitItem (iKey, iDebugValue, iReleaseValue);

			if (this.Debug == null) {
				this.Debug = new List<string> ();
			}
			if ((iDebugValue != null) && (iDebugValue.Count > 0)) {
				this.Debug.AddRange (iDebugValue);
			}

			if (this.Release == null) {
				this.Release = new List<string> ();
			}
			if ((iReleaseValue != null) && (iReleaseValue.Count > 0)) {
				this.Release.AddRange (iReleaseValue);
			}
		}

		public override void Clear() {
			if (Debug != null) {
				Debug.Clear ();
			}
			if (Release != null) {
				Release.Clear ();
			}
		}

		public void AddDebugVaule(string iValue) {
			if ((this.Debug == null) || (string.IsNullOrEmpty(iValue) == true)) {
				return;
			}
			bool isExist = false;
			foreach (string loop in this.Debug) {
				if (iValue.Equals (loop) == true) {
					isExist = true;
					break;
				}
			}
			if (isExist == false) {
				this.Debug.Add (iValue);
			}
		}

		public void AddReleaseVaule(string iValue) {
			if ((this.Release == null) || (string.IsNullOrEmpty(iValue) == true)) {
				return;
			}
			bool isExist = false;
			foreach (string loop in this.Release) {
				if (iValue.Equals (loop) == true) {
					isExist = true;
					break;
				}
			}
			if (isExist == false) {
				this.Release.Add (iValue);
			}
		}
			
		public override void Reset () {
			this.InitItem (this.Key, null, null);
			if (this.Debug != null) {
				this.Debug.Clear();
			}
			if (this.Release != null) {
				this.Release.Clear();
			}
		}

	}

	#endregion

	#region Language 

	/// <summary>
	/// Objective-C Automatic Reference Counting.
	/// </summary>
	[System.Serializable]
	public class AutomaticReferenceCounting : XCBoolItem {
		public AutomaticReferenceCounting(){
		}
		public AutomaticReferenceCounting(TXCBool iDebugValue, TXCBool iReleaseValue) {
			this.InitItem ("CLANG_ENABLE_OBJC_ARC", iDebugValue, iReleaseValue);
		}
		public override void Reset () {
			this.Key = "CLANG_ENABLE_OBJC_ARC";
			this.InitItem (this.Key, TXCBool.No, TXCBool.No);
		}
	}

	[System.Serializable]
	public class XCObjectiveC : XCObjectBase {

		/// <summary>
		/// Objective-C Automatic Reference Counting.
		/// </summary>
		[SerializeField]
		public AutomaticReferenceCounting AutomaticReferenceCounting = new AutomaticReferenceCounting ();

		public override void Clear() {
			if (AutomaticReferenceCounting != null) {
				AutomaticReferenceCounting.Clear();
			}
		}

		public override void Reset() {
			if (AutomaticReferenceCounting != null) {
				AutomaticReferenceCounting.Reset();
			}
		}
	}

	/// <summary>
	/// Enable C++ Exceptions.
	/// </summary>
	[System.Serializable]
	public class EnableCppExceptions : XCBoolItem {
		public EnableCppExceptions(){
		}
		public EnableCppExceptions(TXCBool iDebugValue, TXCBool iReleaseValue) {
			this.InitItem ("GCC_ENABLE_OBJC_EXCEPTIONS", iDebugValue, iReleaseValue);
		}

		public override void Reset () {
			this.Key = "GCC_ENABLE_OBJC_EXCEPTIONS";
			this.InitItem (this.Key, TXCBool.Yes, TXCBool.Yes);
		}
	}

	[System.Serializable]
	public class XCCpp : XCObjectBase {

		/// <summary>
		/// Enable C++ Exceptions.
		/// </summary>
		[SerializeField]
		public EnableCppExceptions EnableCppExceptions = new EnableCppExceptions ();

		public override void Clear() {
			if (EnableCppExceptions != null) {
				EnableCppExceptions.Clear ();
			}
		}

		public override void Reset() {
			if (EnableCppExceptions != null) {
				EnableCppExceptions.Reset();
			}
		}
	}

	[System.Serializable]
	public class XCLanguage : XCObjectBase {

		/// <summary>
		/// C++.
		/// </summary>
		[SerializeField]
		public XCCpp Cpp = new XCCpp ();
		
		/// <summary>
		/// Objective-C.
		/// </summary>
		[SerializeField]
		public XCObjectiveC ObjectC = new XCObjectiveC ();

		public override void Clear() {
			if (Cpp != null) {
				Cpp.Clear ();
			}
			if (ObjectC != null) {
				ObjectC.Clear ();
			}
		}

		public override void Reset() {

			if (Cpp != null) {
				Cpp.Reset ();
			}
			if (ObjectC != null) {
				ObjectC.Reset ();
			}
		}
	}

	#endregion

	#region XCLinking 

	/// <summary>
	/// Other Linker Flags.
	/// </summary>
	[System.Serializable]
	public class OtherLinkerFlags : XCListItem {

		public OtherLinkerFlags(){
		}

		public OtherLinkerFlags(List<string> iDebugValue, List<string> iReleaseValue) {
			List<string> debugValues = null;
			if ((iDebugValue != null) && (iDebugValue.Count > 0)) {
				debugValues = new List<string> ();
				debugValues.AddRange (iDebugValue);
			}

			List<string> releaseValues = null;
			if ((iReleaseValue != null) && (iReleaseValue.Count > 0)) {
				releaseValues = new List<string> ();
				releaseValues.AddRange (iReleaseValue);
			}
			this.InitItem ("OTHER_LDFLAGS", debugValues, releaseValues);
		}
		public void Init() {
			if (this.Debug == null) {
				this.Debug = new List<string> ();
			}
			this.AddDebugVaule ("-ObjC");
			this.AddDebugVaule ("-lz");

			if (this.Release == null) {
				this.Release = new List<string> ();
			}
			this.AddReleaseVaule ("-ObjC");
			this.AddReleaseVaule ("-lz");
		}

		public override void Reset() {
			this.InitItem ("OTHER_LDFLAGS", null, null);
			this.Clear ();
			this.Init ();
		}
	}

	[System.Serializable]
	public class XCLinking : XCObjectBase {
		[SerializeField]
		public OtherLinkerFlags OtherLinkerFlags = new OtherLinkerFlags ();

		public override void Init () { 
			base.Init ();
			if (OtherLinkerFlags != null) {
				OtherLinkerFlags.Init();
			}
		}

		public override void Clear() {
			if (OtherLinkerFlags != null) {
				OtherLinkerFlags.Clear ();
			}
		}

		public override void Reset() {
			if (OtherLinkerFlags != null) {
				OtherLinkerFlags.Reset ();
			}
		}
	}

	#endregion

	#region Build Options

	[System.Serializable]
	public class EnableBitCode : XCBoolItem {
		public EnableBitCode() {
		}
		public EnableBitCode(TXCBool iDebugValue, TXCBool iReleaseValue) {
			this.InitItem ("ENABLE_BITCODE", iDebugValue, iReleaseValue);
		}
		public override void Reset () {
			this.Key = "ENABLE_BITCODE";
			base.Reset ();
		}
	}

	[System.Serializable]
	public class XCBuildOptions : XCObjectBase {
		[SerializeField]
		public EnableBitCode EnableBitCode = new EnableBitCode ();

		public override void Clear() {
			if (EnableBitCode != null) {
				EnableBitCode.Clear ();
			}
		}

		public override void Reset() {
			if (EnableBitCode != null) {
				EnableBitCode.Reset ();
			}
		}
		
	}

	#endregion

	#region User-Defined 

	[System.Serializable]
	public class XCUserDefined : XCObjectBase {		
		public override void Clear() {
		}

		public override void Reset() {
		}
	}

	#endregion

	#region Deployment

	[System.Serializable]
	public class DeploymentPostprocessing : XCBoolItem {
		public DeploymentPostprocessing(){
		}
		public DeploymentPostprocessing(TXCBool iDebugValue, TXCBool iReleaseValue) {
			this.InitItem ("DEPLOYMENT_POSTPROCESSING", iDebugValue, iReleaseValue);
		}
		public override void Reset () {
			this.Key = "DEPLOYMENT_POSTPROCESSING";
			this.InitItem (this.Key, TXCBool.Yes, TXCBool.Yes);
		}
	}

	[System.Serializable]
	public class Deployment : XCObjectBase {

		/// <summary>
		/// Deployment Postprocessing.
		/// </summary>
		[SerializeField]
		public DeploymentPostprocessing DeploymentPostprocessing = new DeploymentPostprocessing();

		public override void Clear () {
			if (this.DeploymentPostprocessing != null) {
				this.DeploymentPostprocessing.Clear ();
			}
		}

		public override void Reset () {
			if (this.DeploymentPostprocessing != null) {
				this.DeploymentPostprocessing.Reset ();
			}
		}

	}	

	#endregion

	#region Signing

	[System.Serializable]
	public sealed class DevelopmentTeam : XCStringItem {
		public DevelopmentTeam() {
		}
		public DevelopmentTeam(string iDebugValue, string iReleaseValue) {
			this.InitItem ("DEVELOPMENT_TEAM", iDebugValue, iReleaseValue);
		}
		public override void Reset () {
			this.Key = "DEVELOPMENT_TEAM";
			this.InitItem (this.Key, null, null);
		}
	}

	[System.Serializable]
	public sealed class XCSigning : XCObjectBase {

		/// <summary>
		/// Development Team.
		/// </summary>
		[SerializeField]
		public DevelopmentTeam DevelopmentTeam = new DevelopmentTeam();

		public override void Clear () {
			if (this.DevelopmentTeam != null) {
				this.DevelopmentTeam.Clear ();
			}
		}

		public override void Reset () {
			if (this.DevelopmentTeam != null) {
				this.DevelopmentTeam.Reset ();
			}
		}
	}

	#endregion

	#region Other-Setting

	[System.Serializable]
	public class XCOtherSetting : XCObjectBase {

		/// <summary>
		/// 布尔值对象列表.
		/// </summary>
		[SerializeField]
		public List<XCBoolItem> BoolTargets = new List<XCBoolItem>();

		/// <summary>
		/// 字符串对象列表.
		/// </summary>
		[SerializeField]
		public List<XCStringItem> StringTargets = new List<XCStringItem>();

		/// <summary>
		/// 列表对象列表.
		/// </summary>
		[SerializeField]
		public List<XCListItem> ListTargets = new List<XCListItem>();

		public override void Clear() {
			if (BoolTargets != null) {
				BoolTargets.Clear ();
			}
			if (StringTargets != null) {
				StringTargets.Clear ();
			}
			if (ListTargets != null) {
				ListTargets.Clear ();
			}
		}

		public override void Reset() {
			if (BoolTargets != null) {
				foreach (XCBoolItem loop in BoolTargets) {
					loop.Reset ();
				}
			}
			if (StringTargets != null) {
				foreach (XCStringItem loop in StringTargets) {
					loop.Reset ();
				}
			}
			if (ListTargets != null) {
				foreach (XCListItem loop in ListTargets) {
					loop.Reset ();
				}
			}
		}
	}

	#endregion

	[System.Serializable]
	public sealed class XCodeSetting : XCObjectBase {

		/// <summary>
		/// 替换对象列表.
		/// </summary>
		[SerializeField]
		public List<string> ReplaceTargets = new List<string>();

		/// <summary>
		/// FrameWorks.
		/// </summary>
		[SerializeField]
		public List<string> FrameWorks = new List<string> ();

		/// <summary>
		/// Libraries.
		/// </summary>
		[SerializeField]
		public List<string> Libraries = new List<string> ();

		/// <summary>
		/// Files.
		/// </summary>
		[SerializeField]
		public List<string> IncludeFiles = new List<string> ();

		/// <summary>
		/// Folders.
		/// </summary>
		[SerializeField]
		public List<string> IncludeFolders = new List<string> ();

		/// <summary>
		/// Linking.
		/// </summary>
		[SerializeField]
		public XCLinking Linking = new XCLinking();

		/// <summary>
		/// 语言设定.
		/// </summary>
		[SerializeField]
		public XCLanguage Language = new XCLanguage();

		/// <summary>
		/// Deployment.
		/// </summary>
		[SerializeField]
		public Deployment Deployment = new Deployment();

		/// <summary>
		/// BuildOptions.
		/// </summary>
		[SerializeField]
		public XCBuildOptions BuildOptions = new XCBuildOptions();

		/// <summary>
		/// Signing.
		/// </summary>
		[SerializeField]
		public XCSigning Signing = new XCSigning();

		/// <summary>
		/// User-Defined.
		/// </summary>
		[SerializeField]
		public XCUserDefined UserDefined = new XCUserDefined();

		/// <summary>
		/// 其他设定.
		/// </summary>
		[SerializeField]
		public XCOtherSetting OtherSetting = new XCOtherSetting();

		/// <summary>
		/// 清空函数.
		/// </summary>
		public override void Clear() {
			if (ReplaceTargets != null) {
				ReplaceTargets.Clear ();
			}
			if (FrameWorks != null) {
				FrameWorks.Clear ();
			}
			if (Libraries != null) {
				Libraries.Clear ();
			}
			if (IncludeFiles != null) {
				IncludeFiles.Clear ();
			}
			if (IncludeFolders != null) {
				IncludeFolders.Clear ();
			}
			if (Linking != null) {
				Linking.Clear ();
			}
			if (Language != null) {
				Language.Clear ();
			}
			if (Deployment != null) {
				Deployment.Clear ();
			}
			if (BuildOptions != null) {
				BuildOptions.Clear ();
			}
			if (Signing != null) {
				Signing.Clear ();
			}
			if (UserDefined != null) {
				UserDefined.Clear ();
			}
			if (OtherSetting != null) {
				OtherSetting.Clear ();
			}
		}

		public override void Reset() {
			
			if (ReplaceTargets != null) {
				ReplaceTargets.Clear ();
			}

			if (FrameWorks != null) {
				FrameWorks.Clear ();
				// IAP相关
				FrameWorks.Add ("StoreKit.framework");
				FrameWorks.Add ("Security.framework");
			}
			if (Libraries != null) {
				Libraries.Clear ();
			}
			if (IncludeFiles != null) {
				IncludeFiles.Clear ();
			}
			if (IncludeFolders != null) {
				IncludeFolders.Clear ();
			}
			if (Linking != null) {
				Linking.Reset ();
			}
			if (Language != null) {
				Language.Reset ();
			}
			if (Deployment != null) {
				Deployment.Reset ();
			}
			if (BuildOptions != null) {
				BuildOptions.Reset ();
			}
			if (Signing != null) {
				Signing.Reset ();
			}
			if (UserDefined != null) {
				UserDefined.Reset ();
			}
			if (OtherSetting != null) {
				OtherSetting.Reset ();
			}
		}
	}

	[System.Serializable]
	public sealed class XCodeSettings : XCObjectBase {

		/// <summary>
		/// 目标工程.
		/// </summary>
		[SerializeField]
		public string Target = null;

		/// <summary>
		/// 设定情报.
		/// </summary>
		[SerializeField]
		public XCodeSetting Settings = new XCodeSetting ();

		/// <summary>
		/// 清空函数.
		/// </summary>
		public override void Clear() {
			Target = null;
			if (Settings != null) {
				Settings.Clear ();
			}
		}

		public override void Reset() {
			Target = "Unity-iPhone";
			if (Settings != null) {
				Settings.Reset ();
			}
		}

	}

}
