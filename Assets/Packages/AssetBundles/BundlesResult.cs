using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Packages.Common.Base;
using Packages.Common.Extend;
using Packages.Utils;

namespace Packages.AssetBundles {

#if UNITY_EDITOR

	/// <summary>
	/// 上传方式
	/// </summary>
	public enum UploadWay {
		/// <summary>
		/// Ftp
		/// </summary>
		Ftp,
		
		/// <summary>
		/// Curl
		/// </summary>
		Curl,
		
		/// <summary>
		/// 默认模式：Curl
		/// </summary>
		Default = Curl
	}

#endif
	
	/// <summary>
	/// 压缩格式(保留).
	///    暂时压缩/解压的
	/// </summary>
	public enum CompressFormat {
		None,
		/// <summary>
		/// Zip
		/// </summary>
		Zip,
		
		/// <summary>
		/// LZ4
		/// </summary>
		LZ4
	}

	/// <summary>
	/// AssetBundle文件类型.
	/// </summary>
	public enum BundleFileType {
		/// <summary>
		/// Manifest(Main).
		/// </summary>
		MainManifest,
		/// <summary>
		/// Manifest(Normal).
		/// </summary>
		NormalManifest,
		/// <summary>
		/// Bundle.
		/// </summary>
		Bundle
	}

	/// <summary>
	/// 上传信息.
	/// </summary>
	[Serializable]
	public class BundlesResultItem : JsonDataBase<BundlesResultItem> {

		/// <summary>
		/// bundle no.
		/// </summary>
		[FormerlySerializedAs("No")] 
		public int no;

		/// <summary>
		/// ID.
		/// </summary>
		[FormerlySerializedAs("ID")] 
		public string id;

		/// <summary>
		/// bundle类型.
		/// </summary>
		[FormerlySerializedAs("BundleType")] 
		public BundleType bundleType = BundleType.Normal;

		/// <summary>
		/// 上传文件类型.
		/// </summary>
		[FormerlySerializedAs("FileType")] 
		public BundleFileType fileType = BundleFileType.Bundle;

		/// <summary>
		/// 数据大小.
		/// </summary>
		[FormerlySerializedAs("DataSize")] 
		public string dataSize;

		/// <summary>
		/// 验证码（具体什么类型的验证码，与UnloadList指定的验证模式相关）.
		/// </summary>
		[FormerlySerializedAs("CheckCode")] 
		public string checkCode;

		/// <summary>
		/// 已上传标志位.
		/// </summary>
		[FormerlySerializedAs("Uploaded")] 
		public bool uploaded;

		/// <summary>
		/// 废弃标志位.
		/// </summary>
		/// <value><c>true</c> 废弃; 不废弃, <c>false</c>.</value>
		[FormerlySerializedAs("Scraped")] 
		public bool scraped;
	}

	/// <summary>
	/// 上传列表数据.
	/// </summary>
	[Serializable]
	public class BundlesResultData : JsonDataBase<BundlesResultData> {

		/// <summary>
		/// 打包名.
		/// </summary>
		[FormerlySerializedAs("BuildName")] 
		public string buildName;

		/// <summary>
		/// 文件后缀名.
		/// </summary>
		[FormerlySerializedAs("FileSuffix")] 
		public string fileSuffix;

		/// <summary>
		/// 检测模式.
		/// </summary>
		[FormerlySerializedAs("CheckMode")] 
		public CheckMode checkMode = CheckMode.CustomMd5;

		/// <summary>
		/// 平台类型.
		/// </summary>
		/// <value>平台类型.</value>
		[FormerlySerializedAs("BuildTarget")] 
		public string buildTarget;

		/// <summary>
		/// App版本号.
		/// </summary>
		[FormerlySerializedAs("AppVersion")] 
		public string appVersion;

		/// <summary>
		/// 压缩格式.
		/// </summary>
		[FormerlySerializedAs("CompressFormat")] 
		public CompressFormat compressFormat;

		/// <summary>
		/// Manifest上传标志位.
		/// </summary>
		[FormerlySerializedAs("ManifestUpload")] 
		public bool manifestUpload;

#if UNITY_EDITOR

		/// <summary>
		/// 上传方式
		/// </summary>
		[FormerlySerializedAs("UploadWay")] 
		public UploadWay uploadWay = UploadWay.Default;

#endif
		/// <summary>
		/// 目标列表.
		/// </summary>
		[FormerlySerializedAs("Targets")] 
		public List<BundlesResultItem> targets = new List<BundlesResultItem> ();

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Clear() {
			base.Clear();

			buildName = null;
			fileSuffix = null;
			checkMode = CheckMode.CustomMd5;
			buildTarget = null;
			appVersion = null;
			compressFormat = CompressFormat.None;
			manifestUpload = false;
			targets.Clear ();

#if UNITY_EDITOR
			uploadWay = UploadWay.Default;
#endif
		}
	}

	/// <summary>
	/// 上传列表.
	/// </summary>
	public class BundlesResult : AssetBase<BundlesResult, BundlesResultData> {

		/// <summary>
		/// 打包名.
		/// </summary>
		public string BuildName {
			get
			{
				return data?.buildName;
			}
			set {  
				if (null != data) {
					data.buildName = value;
				}
			}
		}

		/// <summary>
		/// 文件后缀名.
		/// </summary>
		public string FileSuffix {
			get
			{
				return data?.fileSuffix;
			}
			set {  
				if (null != data) {
					data.fileSuffix = value;
				}
			}
		}

		/// <summary>
		/// 检测模式.
		/// </summary>
		public CheckMode CheckMode {
			get
			{
				return data?.checkMode ?? CheckMode.CustomMd5;
			}
			set {  
				if (null != data) {
					data.checkMode = value;
				}
			}
		}

		/// <summary>
		/// 平台类型.
		/// </summary>
		/// <value>平台类型.</value>
		public string BuildTarget {
			get
			{
				return data?.buildTarget;
			}
			set {  
				if (null != data) {
					data.buildTarget = value;
				}
			}
		}

		/// <summary>
		/// App版本号.
		/// </summary>
		public string AppVersion {
			get
			{
				return data?.appVersion;
			}
			set {  
				if (null != data) {
					data.appVersion = value;
				}
			}
		}

		/// <summary>
		/// 压缩格式.
		/// </summary>
		public CompressFormat CompressFormat {
			get
			{
				return data?.compressFormat ?? CompressFormat.None;
			}
			set {  
				if (null != data) {
					data.compressFormat = value;
				}
			}
		}
		
#if UNITY_EDITOR
		
		/// <summary>
		/// 上传方式.
		/// </summary>
		public UploadWay UploadWay {
			get
			{
				return data?.uploadWay ?? UploadWay.Default;
			}
			set {  
				if (null != data) {
					data.uploadWay = value;
				}
			}
		}
		
#endif

		/// <summary>
		/// Manifest上传标志位.
		/// </summary>
		public bool ManifestUpload {
			get
			{
				return null != data && data.manifestUpload;
			}
			set {  
				if (null != data) {
					data.manifestUpload = value;
				}
			}
		}

		/// <summary>
		/// 目标列表.
		/// </summary>
		public List<BundlesResultItem> Targets => data?.targets;

		/// <summary>
		/// AssetBundle打包输出目录.
		/// </summary>
		public string BundlesOutputDir {
			get;
			private set;
		}

		/// <summary>
		/// 一般的Assetbundle的Main Manifest bundle ID(StreamingAssets).
		/// </summary>
		public static string AssetBundleDirNameOfNormal => BundleType.Normal.ToString();

		/// <summary>
		/// Scene打包成AssetBundle，存放的文件夹名.
		/// </summary>
		public static string AssetBundleDirNameOfScenes => BundleType.Scene.ToString();

		/// <summary>
		/// 一般的AssetBundle打包输出路径.
		/// </summary>
		public string BundlesOutputDirOfNormal {
			get { 
				var dir = $"{BundlesOutputDir}/{AssetBundleDirNameOfNormal}";
				if (Directory.Exists (dir) == false) {
					Directory.CreateDirectory (dir);
				}
				return dir;
			}
		}

		/// <summary>
		/// Scenes的AssetBundle打包输出路径
		/// </summary>
		public string BundlesOutputDirOfScene {
			get { 
				var dir = $"{BundlesOutputDir}/{AssetBundleDirNameOfScenes}";
				if (Directory.Exists (dir) == false) {
					Directory.CreateDirectory (dir);
				}
				return dir;
			}
		}

		/// <summary>
		/// 取得BundleNo.
		/// </summary>
		/// <returns>BundleNo.</returns>
		private int GetBundleNo() {
			if (Targets == null || Targets.Count <= 0) {
				return 1;
			}
			return Targets.Count + 1;
		}

		/// <summary>
		/// 创建UploadItem.
		/// </summary>
		/// <returns>UploadItem.</returns>
		/// <param name="iTargetId">目标ID.</param>
		/// <param name="iBundleType">Bundle类型.</param>
		/// <param name="iFileType">文件类型.</param>
		private BundlesResultItem CreateBundleItem(
			string iTargetId, BundleType iBundleType, 
			BundleFileType iFileType) {
			var objRet = new BundlesResultItem
			{
				no = GetBundleNo(),
				id = iTargetId,
				bundleType = iBundleType,
				fileType = iFileType,
				uploaded = false
			};
			Targets.Add (objRet);
			return objRet;
		}

		/// <summary>
		/// 添加MainManifest对象.
		/// </summary>
		public void AddMainManifestAssetsTarget() {

			var manifestBundleId = AssetBundleDirNameOfNormal;
			if (string.IsNullOrEmpty (manifestBundleId)) {
				return;
			}

			var path = GetLocalBundleFilePath(
				manifestBundleId, BundleFileType.MainManifest, false);
			if (File.Exists (path) == false) {
				return;
			}

			var item = new BundleMap {id = manifestBundleId, type = BundleType.Normal};
			var index = path.IndexOf (manifestBundleId, StringComparison.Ordinal);
			item.path = path.Substring (0, index);

			// 添加对象
			AddTarget (item, BundleFileType.MainManifest);

		}

		/// <summary>
		/// 添加对象.
		/// </summary>
		/// <param name="iTarget">目标.</param>
		private void AddTarget(BundlesResultItem iTarget) {
			if (iTarget == null) {
				return;
			}
			Targets.Add (iTarget);
		}

		/// <summary>
		/// 添加对象.
		/// </summary>
		/// <param name="iTarget">对象.</param>
		/// <param name="iFileType">上传文件类型.</param>
		/// <param name="iHashCode">HashCode(Unity3d打包生成).</param>
		public void AddTarget(
			BundleMap iTarget, BundleFileType iFileType, string iHashCode = null) {
			if (iTarget == null) {
				return;
			}
			BundlesResultItem item;
			var filePath = GetLocalBundleFilePath (
				iTarget.id, iFileType, (BundleType.Scene == iTarget.type));
			string checkCode = null;

			string dataSize = null;
			if (false == string.IsNullOrEmpty(filePath) && 
				File.Exists (filePath)) {
				checkCode = CheckMode.Unity3dHash128 == CheckMode ? iHashCode : UtilsTools.GetMd5ByFilePath (filePath);
				var fileInfo = new FileInfo (filePath);
				dataSize = fileInfo.Length.ToString();

			} else {
				this.Warning("AddTarget()::Target File is not exist!!!(ProjectName:{0})", filePath);
			}

			var exist = IsTargetExist (iTarget.id, iFileType, out item);
			if (false == exist) {
				item = CreateBundleItem (iTarget.id, iTarget.type, iFileType);
				item.checkCode = checkCode;
				item.dataSize = dataSize;
			} else {
				if (false == string.IsNullOrEmpty(checkCode) && 
					false == checkCode.Equals (item.checkCode)) {
					item.checkCode = checkCode;
					item.dataSize = dataSize;
					item.uploaded = false;
				}
			}
			UtilsAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 取得本地上传用的输入文件名.
		/// </summary>
		/// <returns>上传用的输入文件名.</returns>
		/// <param name="iBundleId">Bundle ID.</param>
		/// <param name="iFileType">文件类型.</param>
		public static string GetLocalBundleFileName(
			string iBundleId, BundleFileType iFileType) {

			var fileName = iBundleId;
			switch (iFileType) {
			case BundleFileType.Bundle:
				{
					var fileSuffix = GetInstance ().FileSuffix;
					if (string.IsNullOrEmpty (fileSuffix) == false) {
						fileName = $"{fileName}.{fileSuffix}";
					}
				}
				break;
			case BundleFileType.NormalManifest:
				{
					var fileSuffix = GetInstance ().FileSuffix;
					if (string.IsNullOrEmpty (fileSuffix) == false) {
						fileName = $"{fileName}.{fileSuffix}";
					}
					fileName = $"{fileName}.manifest";
				}
				break;
			case BundleFileType.MainManifest:
				break;
			default:
				{
					
				}
				break;
			}
			return fileName;
		}

		/// <summary>
		/// 取得场景Bundle的文件路径.
		/// </summary>
		/// <returns>场景Bundle的文件路径.</returns>
		/// <param name="iBundleId">BundleId.</param>
		public static string GetLocalSceneBundleFilePath(string iBundleId) {
			var fileName = GetLocalBundleFileName(iBundleId, BundleFileType.Bundle);
			return $"{GetInstance().BundlesOutputDirOfScene}/{fileName}";
		}

		/// <summary>
		/// 取得本地上传用的输入文件地址.
		/// </summary>
		/// <returns>上传用的输入文件地址.</returns>
		/// <param name="iBundleId">Bundle ID.</param>
		/// <param name="iFileType">文件类型.</param>
		/// <param name="iIsScene">场景标志位.</param>
		private static string GetLocalBundleFilePath(
			string iBundleId, BundleFileType iFileType, bool iIsScene) {
		
			var fileName = GetLocalBundleFileName(iBundleId, iFileType);
			return
				$"{(iIsScene ? GetInstance().BundlesOutputDirOfScene : GetInstance().BundlesOutputDirOfNormal)}/{fileName}";
		}

		/// <summary>
		/// 取得Bundle的相对路径（下载/上传用）.
		/// 路径：bundles/<BuildTarget:(iOS/Android)><BuildMode:(Debug/Release/Store)><UploadDatetime>
		/// </summary>
		/// <returns>Bundle的相对路径.</returns>
		/// <param name="iIsScene">场景标志位.</param>
		public static string GetBundleRelativePath(bool iIsScene)
		{
			return
				$"bundles/{GetInstance().BuildTarget}/{GetInstance().AppVersion}/{(iIsScene ? AssetBundleDirNameOfScenes : AssetBundleDirNameOfNormal)}";
		}

#region Implement

		/// <summary>
		/// 初始化Asset.
		/// </summary>
		protected override bool InitAsset () {
			BundlesOutputDir = Application.streamingAssetsPath;

			return base.InitAsset ();
		}

		/// <summary>
		/// 应用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空.</param>
		protected override void ApplyData(BundlesResultData iData, bool iForceClear = true) {
			if (null == iData) {
				return;
			}

			// 清空
			if (iForceClear) {
				Clear ();
			}

			data.buildName = iData.buildName;
			data.fileSuffix = iData.fileSuffix;
			data.checkMode = iData.checkMode;
			data.appVersion = iData.appVersion;
			data.buildTarget = iData.buildTarget;
			data.manifestUpload = iData.manifestUpload;
			data.compressFormat = iData.compressFormat;

			// 添加资源信息
			foreach(var loop in iData.targets) {
				AddTarget (loop);
			}
			UtilsAsset.SetAssetDirty (this);

		}

		/// <summary>
		/// 清空.
		/// </summary>
		/// <param name="iIsFileDelete">删除数据文件标志位.</param>
		/// <param name="iDirPath">Asset存放目录文件（不指定：当前选定对象所在目录）.</param>
		public override void Clear(bool iIsFileDelete = false, string iDirPath = null) {
			
			base.Clear (iIsFileDelete, iDirPath);

			// 清空列表
			UtilsAsset.SetAssetDirty (this);
		}

#endregion


		/// <summary>
		/// 判断目标是否存在.
		/// </summary>
		/// <returns><c>true</c>,存在, <c>false</c> 不存在.</returns>
		/// <param name="iTargetId">目标ID.</param>
		/// <param name="iFileType">文件类型.</param>
		/// <param name="iTarget">目标信息.</param>
		private bool IsTargetExist(string iTargetId, BundleFileType iFileType, out BundlesResultItem iTarget) {
			iTarget = null;

			var targets = Targets
				.Where (iO => iTargetId.Equals(iO.id) && 
				              iFileType == iO.fileType)
				.OrderBy (iO => iO.no)
				.ToArray ();
			if (targets.Length <= 0) {
				return false;
			}
			if (1 != targets.Length) {
				this.Warning("isTargetExist()::There is duplicate id exist in upload list!!!(Bundle ID:{0})", iTargetId);
			}
			iTarget = targets [0];
			return true;
		}
		
	}
}
