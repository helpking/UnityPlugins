using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Packages.BuildSystem.iOS {
	
#region XCObjectBase

	/// <summary>
	/// 布尔类型
	/// </summary>
	public enum TxcBool {
		Yes,
		No
	}

	/// <summary>
	/// Xcode对象基类
	/// </summary>
	public abstract class XcObjectBase {
		
		[Header("是否忽略当前设定")]
		public bool Ignore = false;
		
		public XcObjectBase() {
			Init ();
		}
		public virtual void Init () { }

		public abstract void Clear ();
		public abstract void Reset () ;
	}

	public abstract class XcObjectItem<T> : object  {
		
		[Header("是否忽略当前设定")]
		public bool Ignore = false;
		
		[Header("PBXProject的Key")]
		public string Key;
		
		[Header("打包设定")]
		public T Debug;
		public T Release;
		public T ReleaseForProfiling;
		public T ReleaseForRunning;
		
		public virtual void Clear() {
			Debug = default(T);
			Release = default(T);
			ReleaseForProfiling = default(T);
			ReleaseForRunning = default(T);
		}

		protected abstract void InitItem(
			string iKey,
			T iDebug, T iRelease,
			T iReleaseForProfiling, T iReleaseForRunning);
		public abstract void Reset ();
	}

	[System.Serializable]
	public class XcBoolItem : XcObjectItem<TxcBool>  {

		protected override void InitItem(
			string iKey,
			TxcBool iDebug, TxcBool iRelease,
			TxcBool iReleaseForProfiling, TxcBool iReleaseForRunning)
		{
			Key = iKey;
			Debug = iDebug;
			Release = iRelease;
			ReleaseForProfiling = iReleaseForProfiling;
			ReleaseForRunning = iReleaseForRunning;
		}

		public override void Reset () {
		}
	}

	[System.Serializable]
	public class XcStringItem : XcObjectItem<string>  {
		protected override void InitItem(
			string iKey,
			string iDebug, string iRelease,
			string iReleaseForProfiling, string iReleaseForRunning)
		{
			Key = iKey;
			Debug = iDebug;
			Release = iRelease;
			ReleaseForProfiling = iReleaseForProfiling;
			ReleaseForRunning = iReleaseForRunning;
		}
		
		public override void Reset () {
		}
	}

	[System.Serializable]
	public class XcListItem : XcObjectItem<List<string>>  {
		
		protected override void InitItem(
			string iKey, 
			List<string> iDebug, List<string> iRelease,
			List<string> iReleaseForProfiling, List<string> iReleaseForRunning){
			if (Debug == null) {
				Debug = new List<string> ();
			}
			else
			{
				Debug.Clear();
			}
			Debug.AddRange (iDebug);

			if (Release == null) {
				Release = new List<string> ();
			}
			else
			{
				Release.Clear();
			}
			Release.AddRange (iRelease);

			if (ReleaseForProfiling == null) {
				ReleaseForProfiling = new List<string> ();
			}
			else
			{
				ReleaseForProfiling.Clear();
			}
			ReleaseForProfiling.AddRange (iReleaseForProfiling);

			if (ReleaseForRunning == null) {
				ReleaseForRunning = new List<string> ();
			}
			else
			{
				ReleaseForRunning.Clear();
			}
			ReleaseForRunning.AddRange (iReleaseForProfiling);
		}

		public override void Clear()
		{
			Debug?.Clear();
			Release?.Clear();
		}

		public void AddDebug(string iValue) {
			if (Debug == null || string.IsNullOrEmpty(iValue)) {
				return;
			}
			var isExist = false;
			foreach (var loop in Debug) {
				if (iValue.Equals(loop) != true) continue;
				isExist = true;
				break;
			}
			if (isExist == false) {
				Debug.Add (iValue);
			}
		}

		public void AddRelease(string iValue) {
			if (Release == null || string.IsNullOrEmpty(iValue)) {
				return;
			}
			var isExist = false;
			foreach (var loop in Release) {
				if (!iValue.Equals(loop)) continue;
				isExist = true;
				break;
			}
			if (isExist == false) {
				Release.Add (iValue);
			}
		}

		public void AddReleaseForProfiling(string iValue) {
			if (ReleaseForProfiling == null || string.IsNullOrEmpty(iValue)) {
				return;
			}
			var isExist = false;
			foreach (var loop in ReleaseForProfiling) {
				if (!iValue.Equals(loop)) continue;
				isExist = true;
				break;
			}
			if (isExist == false) {
				ReleaseForProfiling.Add (iValue);
			}
		}

		public void AddReleaseForRunning(string iValue) {
			if (ReleaseForRunning == null || string.IsNullOrEmpty(iValue)) {
				return;
			}
			var isExist = false;
			foreach (var loop in ReleaseForRunning) {
				if (!iValue.Equals(loop)) continue;
				isExist = true;
				break;
			}
			if (isExist == false) {
				ReleaseForRunning.Add (iValue);
			}
		}
			
		public override void Reset () {
			InitItem (Key, null, null, null, null);
			Debug?.Clear();
			Release?.Clear();
		}

	}

#endregion

#region Language 

	/// <summary>
	/// Objective-C Automatic Reference Counting.
	/// </summary>
	[System.Serializable]
	public class AutomaticReferenceCounting : XcBoolItem {
		public AutomaticReferenceCounting(){
			Key = "CLANG_ENABLE_OBJC_ARC";
		}
		public AutomaticReferenceCounting(
			TxcBool iDebug, TxcBool iRelease,
			TxcBool iReleaseForProfiling, TxcBool iReleaseForRunning)
		{
			Debug = iDebug;
			Release = iRelease;
			ReleaseForProfiling = iReleaseForProfiling;
			ReleaseForRunning = iReleaseForRunning;
		}
		public override void Reset () {
			Key = "CLANG_ENABLE_OBJC_ARC";
		}
	}

	[System.Serializable]
	public class XcObjectiveC : XcObjectBase {

		/// <summary>
		/// Objective-C Automatic Reference Counting.
		/// </summary>
		[Header("Objective-C Automatic Reference Counting")]
		public AutomaticReferenceCounting automaticReferenceCounting = new AutomaticReferenceCounting ();

		public override void Clear()
		{
			automaticReferenceCounting?.Clear();
		}

		public override void Reset()
		{
			automaticReferenceCounting?.Reset();
		}
	}

	/// <summary>
	/// Enable C++ Exceptions.
	/// </summary>
	[System.Serializable]
	public class EnableCppExceptions : XcBoolItem {
		public EnableCppExceptions(){
			Key = "GCC_ENABLE_OBJC_EXCEPTIONS";
		}
		public EnableCppExceptions(
			TxcBool iDebug, TxcBool iRelease,
			TxcBool iReleaseForProfiling, TxcBool iReleaseForRunning)
		{
			Debug = iDebug;
			Release = iRelease;
			ReleaseForProfiling = iReleaseForProfiling;
			ReleaseForRunning = iReleaseForRunning;
		}

		public override void Reset () {
			Key = "GCC_ENABLE_OBJC_EXCEPTIONS";
		}
	}

	[System.Serializable]
	public class XcCpp : XcObjectBase {

		/// <summary>
		/// Enable C++ Exceptions.
		/// </summary>
		[Header("Enable C++ Exceptions")]
		public EnableCppExceptions enableCppExceptions = new EnableCppExceptions ();

		public override void Clear()
		{
			enableCppExceptions?.Clear();
		}

		public override void Reset()
		{
			enableCppExceptions?.Reset();
		}
	}

	[System.Serializable]
	public class XcLanguage : XcObjectBase {

		/// <summary>
		/// C++.
		/// </summary>
		[Header("Apple Clang - Language - C++")]
		public XcCpp cpp = new XcCpp ();
		
		/// <summary>
		/// Objective-C.
		/// </summary>
		[Header("Apple Clang - Language - Objective-C")]
		public XcObjectiveC objectC = new XcObjectiveC ();

		public override void Clear()
		{
			cpp?.Clear();

			objectC?.Clear();
		}

		public override void Reset()
		{
			cpp?.Reset();

			objectC?.Reset();
		}
	}

#endregion

#region XCLinking 

	/// <summary>
	/// Other Linker Flags.
	/// </summary>
	[System.Serializable]
	public class OtherLinkerFlags : XcListItem {

		public OtherLinkerFlags(){
			Key = "OTHER_LDFLAGS";
		}

		public OtherLinkerFlags(
			List<string> iDebug, List<string> iRelease,
			List<string> iReleaseForProfiling, List<string> iReleaseForRunning) {
			List<string> debugValues = null;
			if (iDebug != null && iDebug.Count > 0) {
				debugValues = new List<string> ();
				debugValues.AddRange (iDebug);
			}

			List<string> releaseValues = null;
			if (iRelease != null && iRelease.Count > 0) {
				releaseValues = new List<string> ();
				releaseValues.AddRange (iRelease);
			}
			
			List<string> releaseForProfiling = null;
			if (iReleaseForProfiling != null && iReleaseForProfiling.Count > 0) {
				releaseForProfiling = new List<string> ();
				releaseForProfiling.AddRange (iReleaseForProfiling);
			}
			
			List<string> releaseForRunning = null;
			if (iReleaseForRunning != null && iReleaseForRunning.Count > 0) {
				releaseForRunning = new List<string> ();
				releaseForRunning.AddRange (iReleaseForRunning);
			}
			InitItem ("OTHER_LDFLAGS", 
				debugValues, releaseValues, 
				releaseForProfiling, releaseForRunning);
		}
		public void Init() {
			if (Debug == null) {
				Debug = new List<string> ();
			}
			AddDebug ("-ObjC");
			AddDebug ("-lz");

			if (Release == null) {
				Release = new List<string> ();
			}
			AddRelease ("-ObjC");
			AddRelease ("-lz");

			if (ReleaseForProfiling == null) {
				ReleaseForProfiling = new List<string> ();
			}
			AddReleaseForProfiling("-ObjC");
			AddReleaseForProfiling ("-lz");

			if (ReleaseForRunning == null) {
				ReleaseForRunning = new List<string> ();
			}
			AddReleaseForRunning("-ObjC");
			AddReleaseForRunning ("-lz");
		}

		public override void Reset() {
			InitItem ("OTHER_LDFLAGS", null, null, null, null);
			Clear ();
			Init ();
		}
	}

	[System.Serializable]
	public class XcLinking : XcObjectBase {
		public OtherLinkerFlags otherLinkerFlags = new OtherLinkerFlags ();

		public override void Init ()
		{
			base.Init ();
			otherLinkerFlags?.Init();
		}

		public override void Clear()
		{
			otherLinkerFlags?.Clear();
		}

		public override void Reset()
		{
			otherLinkerFlags?.Reset();
		}
	}

#endregion

#region Build Options

	[System.Serializable]
	public class EnableBitCode : XcBoolItem {
		public EnableBitCode() {
			Key = "ENABLE_BITCODE";
		}
		public EnableBitCode(
			TxcBool iDebug, TxcBool iRelease, 
			TxcBool iReleaseForProfiling, TxcBool iReleaseForRunning)
		{
			Debug = iDebug;
			Release = iRelease;
			ReleaseForProfiling = iReleaseForProfiling;
			ReleaseForRunning = iReleaseForRunning;
		}
		public override void Reset () {
			Key = "ENABLE_BITCODE";
			base.Reset ();
		}
	}

	[System.Serializable]
	public class XcBuildOptions : XcObjectBase {
		/// <summary>
		/// Enable Bitcode
		/// </summary>
		[Header("Enable Bitcode")]
		public EnableBitCode enableBitCode = new EnableBitCode ();

		public override void Clear()
		{
			enableBitCode?.Clear();
		}

		public override void Reset()
		{
			enableBitCode?.Reset();
		}
		
	}

#endregion

#region User-Defined 

	[System.Serializable]
	public class XcUserDefined : XcObjectBase {		
		public override void Clear() {
		}

		public override void Reset() {
		}
	}
	
#endregion

#region Deployment

	[System.Serializable]
	public class DeploymentPostprocessing : XcBoolItem {
		public DeploymentPostprocessing(){
			Key = "DEPLOYMENT_POSTPROCESSING";
		}
		public DeploymentPostprocessing(
			TxcBool iDebug, TxcBool iRelease,
			TxcBool iReleaseForProfiling, TxcBool iReleaseForRunning)
		{
			Debug = iDebug;
			Release = iRelease;
			ReleaseForProfiling = iReleaseForProfiling;
			ReleaseForRunning = iReleaseForRunning;
		}
		public override void Reset () {
			Key = "DEPLOYMENT_POSTPROCESSING";
		}
	}

	[System.Serializable]
	public class Deployment : XcObjectBase {

		/// <summary>
		/// Deployment Postprocessing.
		/// </summary>
		[Header("Deployment Postprocessing")]
		public DeploymentPostprocessing deploymentPostprocessing = new DeploymentPostprocessing();

		public override void Clear ()
		{
			deploymentPostprocessing?.Clear();
		}

		public override void Reset ()
		{
			deploymentPostprocessing?.Reset();
		}

	}	

#endregion

#region Signing

	/// <summary>
	/// 签名风格
	/// </summary>
	public enum SignStyle
	{
		/// <summary>
		/// 手动
		/// </summary>
		Manual,
		/// <summary>
		/// 自动
		/// </summary>
		Automatic
	}

	[System.Serializable]
	public sealed class CodeSignIdentity : XcStringItem
	{
		public CodeSignIdentity() {
			Key = "CODE_SIGN_IDENTITY";
		}

		public override void Reset()
		{
			Key = "CODE_SIGN_IDENTITY";
		}
	}

	[System.Serializable]
	public sealed class CodeSignStyle : XcObjectItem<SignStyle> {
		public CodeSignStyle() {
			Key = "CODE_SIGN_STYLE";
		}

		protected override void InitItem(
			string iKey,
			SignStyle iDebug, SignStyle iRelease,
			SignStyle iReleaseForProfiling, SignStyle iReleaseForRunning)
		{
			Key = iKey;
			Debug = iDebug;
			Release = iRelease;
			ReleaseForProfiling = iReleaseForProfiling;
			ReleaseForRunning = iReleaseForRunning;
		}

		public override void Reset () {
			Key = "CODE_SIGN_STYLE";
		}
	}

	[System.Serializable]
	public sealed class ProvisioningProfile : XcStringItem
	{
		public ProvisioningProfile() {
			Key = "PROVISIONING_PROFILE_SPECIFIER";
		}
		public override void Reset()
		{
			Key = "PROVISIONING_PROFILE_SPECIFIER";
		}
	}

	[System.Serializable]
	public sealed class DevelopmentTeam : XcStringItem {
		public DevelopmentTeam() {
			Key = "DEVELOPMENT_TEAM";
		}
		public DevelopmentTeam(
			string iDebug, string iRelease,
			string iReleaseForProfiling, string iReleaseForRunning)
		{
			Debug = iDebug;
			Release = iRelease;
			ReleaseForProfiling = iReleaseForProfiling;
			ReleaseForRunning = iReleaseForRunning;
		}
		public override void Reset () {
			Key = "DEVELOPMENT_TEAM";
		}
	}

	[System.Serializable]
	public sealed class XcSigning : XcObjectBase
	{
		
		/// <summary>
		/// Code Sign Identity 
		/// </summary>
		[Header("Code Sign Identity")]
		public CodeSignIdentity codeSignIdentity = new CodeSignIdentity();
		
		/// <summary>
		/// Code Sign Style.
		/// </summary>
		[Header("Code Sign Style")]
		public CodeSignStyle codeSignStyle = new CodeSignStyle();
		
		/// <summary>
		/// Provisioning Profile
		/// </summary>
		[Header("Provisioning Profile(.mobileprovision文件)")]
		public ProvisioningProfile provisioningProfile = new ProvisioningProfile();

		/// <summary>
		/// Development Team.
		/// </summary>
		[Header("Development Team")]
		public DevelopmentTeam developmentTeam = new DevelopmentTeam();

		public override void Clear ()
		{
			codeSignIdentity?.Clear();
			developmentTeam?.Clear();
			provisioningProfile?.Clear();
			codeSignStyle?.Clear();
		}

		public override void Reset ()
		{
			codeSignIdentity?.Reset();
			developmentTeam?.Reset();
			provisioningProfile?.Reset();
			codeSignStyle?.Reset();
		}
	}

#endregion

#region Other-Setting

	[System.Serializable]
	public class XcOtherSetting : XcObjectBase {

		/// <summary>
		/// 布尔值对象列表.
		/// </summary>
		public List<XcBoolItem> boolTargets = new List<XcBoolItem>();

		/// <summary>
		/// 字符串对象列表.
		/// </summary>
		public List<XcStringItem> stringTargets = new List<XcStringItem>();

		/// <summary>
		/// 列表对象列表.
		/// </summary>
		public List<XcListItem> listTargets = new List<XcListItem>();

		public override void Clear() {
			boolTargets?.Clear();
			stringTargets?.Clear();
			listTargets?.Clear();
		}

		public override void Reset() {
			if (boolTargets != null) {
				foreach (var loop in boolTargets) {
					loop.Reset ();
				}
			}
			if (stringTargets != null) {
				foreach (var loop in stringTargets) {
					loop.Reset ();
				}
			}

			if (listTargets == null) return;
			{
				foreach (var loop in listTargets) {
					loop.Reset ();
				}
			}
		}
	}

	#endregion

	[System.Serializable]
	public sealed class XCodeSetting : XcObjectBase {

		/// <summary>
		/// 替换对象列表.
		/// </summary>
		[Header("源码替换列表")]
		public List<string> replaceTargets = new List<string>();

		/// <summary>
		/// FrameWorks.
		/// </summary>
		[Header("框架")]
		public List<string> frameWorks = new List<string> ();

		/// <summary>
		/// Libraries.
		/// </summary>
		[Header("外部库")]
		public List<string> libraries = new List<string> ();

		/// <summary>
		/// Files.
		/// </summary>
		[Header("包含文件列表")]
		public List<string> includeFiles = new List<string> ();

		/// <summary>
		/// Folders.
		/// </summary>
		[Header("包含目录列表")]
		public List<string> includeFolders = new List<string> ();

		/// <summary>
		/// Linking.
		/// </summary>
		[Header("Linking")]
		public XcLinking linking = new XcLinking();

		/// <summary>
		/// 语言设定.
		/// </summary>
		[Header("Apple Clang - Language")]
		public XcLanguage language = new XcLanguage();

		/// <summary>
		/// Deployment.
		/// </summary>
		[Header("Deployment")]
		public Deployment deployment = new Deployment();

		/// <summary>
		/// BuildOptions.
		/// </summary>
		[Header("Build Options")]
		public XcBuildOptions buildOptions = new XcBuildOptions();

		/// <summary>
		/// Signing.
		/// </summary>
		[Header("Signing")]
		public XcSigning signing = new XcSigning();

		/// <summary>
		/// User-Defined.
		/// </summary>
		[Header("用户自定义")]
		public XcUserDefined userDefined = new XcUserDefined();

		/// <summary>
		/// 其他设定.
		/// </summary>
		[Header("其他")]
		public XcOtherSetting otherSetting = new XcOtherSetting();

		/// <summary>
		/// 清空函数.
		/// </summary>
		public override void Clear() {
			replaceTargets?.Clear();
			frameWorks?.Clear();
			libraries?.Clear();
			includeFiles?.Clear();
			includeFolders?.Clear();
			linking?.Clear();
			language?.Clear();
			deployment?.Clear();
			buildOptions?.Clear();
			signing?.Clear();
			userDefined?.Clear();
			otherSetting?.Clear();
		}

		public override void Reset() {
			replaceTargets?.Clear();

			if (frameWorks != null) {
				frameWorks.Clear ();
				// IAP相关
				frameWorks.Add ("StoreKit.framework");
				frameWorks.Add ("Security.framework");
			}

			libraries?.Clear();
			includeFiles?.Clear();
			includeFolders?.Clear();
			linking?.Reset();
			language?.Reset();
			deployment?.Reset();
			buildOptions?.Reset();
			signing?.Reset();
			userDefined?.Reset();
			otherSetting?.Reset();
		}
	}

	[System.Serializable]
	public sealed class XCodeSettings : XcObjectBase {

		/// <summary>
		/// 目标工程.
		/// </summary>
		[Header("XCode工程名")]
		public string xcodeSchema;

		/// <summary>
		/// 设定情报.
		/// </summary>
		[Header("详细设定")]
		public XCodeSetting settings = new XCodeSetting ();

		/// <summary>
		/// 清空函数.
		/// </summary>
		public override void Clear()
		{
			xcodeSchema = null;
			settings?.Clear();
		}

		public override void Reset()
		{
			xcodeSchema = "Unity-iPhone";
			settings?.Reset();
		}

	}

}
