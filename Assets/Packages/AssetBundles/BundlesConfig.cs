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

	/// <summary>
	/// 打包模式.
	/// </summary>
	public enum BundleMode
	{
		/// <summary>
		/// 无.
		/// </summary>
		None,
		/// <summary>
		/// 单一目录.
		/// </summary>
		OneDir = 1,
		/// <summary>
		/// 文件一对一.
		/// </summary>
		FileOneToOne = 2,
		/// <summary>
		/// 顶层目录一对一.
		/// </summary>
		TopDirOneToOne = 3,
		/// <summary>
		/// 场景一对一.
		/// </summary>
		SceneOneToOne = 4,
	}

	/// <summary>
	/// 匹配目录信息.
	/// </summary>
	public class MatchDirInfo {
		/// <summary>
		/// 前一个.
		/// </summary>
		public MatchDirInfo Prev;
		/// <summary>
		/// 后一个.
		/// </summary>
		public MatchDirInfo Next;

		/// <summary>
		/// 名字.
		/// </summary>
		public string Name;
	}

	/// <summary>
	/// 打包资源资源信息
	/// </summary>
	[Serializable]
	public class BundleResource : JsonDataBase<BundleResource>
	{
		/// <summary>
		/// 路径.
		/// </summary>
		[FormerlySerializedAs("AssetPath")] 
		public string path;
		/// <summary>
		/// 模式.
		/// </summary>
		[FormerlySerializedAs("Mode")] 
		public BundleMode mode = BundleMode.OneDir;
		/// <summary>
		/// 忽略列表.
		/// </summary>
		[FormerlySerializedAs("IgnoreList")] 
		public List<string> ignoreList = new List<string>();

		/// <summary>
		/// 初始化.
		/// </summary>
		public override void Init() {
			base.Init ();
			mode = BundleMode.OneDir;
		}

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			path = null;
			mode = BundleMode.None;
			ignoreList.Clear ();
		}

		/// <summary>
		/// 判断忽略目标是否存在.
		/// </summary>
		/// <returns><c>true</c>,存在, <c>false</c> 不存在.</returns>
		/// <param name="iTarget">忽略目标.</param>
		public bool IsIgnoreTargetExist(string iTarget) {

			if (null == ignoreList) {
				ignoreList = new List<string> ();
				return false;
			}
			if (0 >= ignoreList.Count) {
				return false;
			}
			var isExist = false;
			// 检测存不存在
			foreach (var ignore in ignoreList) {
				if (false == iTarget.Equals (ignore)) {
					continue;
				}
				isExist = true;
				break;
			}

			return isExist;
		}

		/// <summary>
		/// 追加忽略目标.
		/// </summary>
		/// <param name="iTarget">忽略目标.</param>
		public void AddIgnoreTarget(string iTarget){
			// 文件夹的场合
			if(Directory.Exists(iTarget)) {
				var targetDir = new DirectoryInfo (iTarget);
				var files = targetDir.GetFiles ();
				var endDir = new DirectoryInfo (Application.dataPath);
				foreach (var file in files) {
					// .DS_Store文件
					if(file.Name.EndsWith(".DS_Store")) {
						continue;
					}
					// *.meta文件
					if(file.Name.EndsWith(".meta")) {
						continue;
					}
					var localFilePath = GetLocalFilePath (file, endDir.Name);
					if (string.IsNullOrEmpty (localFilePath)) {
						continue;
					}
					if (false == IsIgnoreTargetExist (localFilePath)) {
						ignoreList.Add (localFilePath);
					}
				}
			}

			if (true != File.Exists(iTarget)) return;
			if (false == IsIgnoreTargetExist (iTarget)) {
				ignoreList.Add (iTarget);
			}
		}

		/// <summary>
		/// 取得本地地址.
		/// </summary>
		/// <returns>本地地址.</returns>
		/// <param name="iFileInfo">文件信息</param>
		/// <param name="iEndMark">结束位.</param>
		private string GetLocalFilePath(FileInfo iFileInfo, string iEndMark) {
			if (null == iFileInfo) {
				return null;
			}
			var matchedDirLink = CreateMatchDirLink (iFileInfo.Directory, iEndMark);
			string localFilePath = null;
			while (null != matchedDirLink)
			{
				localFilePath = string.IsNullOrEmpty (localFilePath) 
					? matchedDirLink.Name : $"{localFilePath}/{matchedDirLink.Name}";
				matchedDirLink = matchedDirLink.Next;
			}
			return string.IsNullOrEmpty (localFilePath) 
				? null : $"{localFilePath}/{iFileInfo.Name}";
		}

		/// <summary>
		/// 移除忽略目标.
		/// </summary>
		/// <param name="iTarget">忽略目标.</param>
		public void RemoveIgnoreTarget(string iTarget){
			// 文件夹的场合
			if (Directory.Exists (iTarget)) {
				var targetDir = new DirectoryInfo (iTarget);
				var files = targetDir.GetFiles ();
				var endDir = new DirectoryInfo (Application.dataPath);
				foreach (var file in files) {
					// .DS_Store文件
					if(file.Name.EndsWith(".DS_Store")) {
						continue;
					}
					// *.meta文件
					if(file.Name.EndsWith(".meta")) {
						continue;
					}
					var localFilePath = GetLocalFilePath (file, endDir.Name);
					if (string.IsNullOrEmpty (localFilePath)) {
						continue;
					}
					ignoreList.Remove (localFilePath);
				}
			}

			if (File.Exists (iTarget)) {
				ignoreList.Remove (iTarget);
			}
		}

		/// <summary>
		/// 取得与目标匹配度（0.0f ~ 1.0f）.
		/// </summary>
		/// <returns>匹配度.</returns>
		/// <param name="iTarget">I ProjectName.</param>
		public float GetMatch(string iTarget) {
			var match = 0.0f;
			if (string.IsNullOrEmpty (iTarget)) {
				return match;
			}
			// 单一文件模式
			switch(mode) {
			case BundleMode.OneDir:
			case BundleMode.TopDirOneToOne:
				{
					var selfDir = new DirectoryInfo (path);
					var targetDir = new DirectoryInfo (iTarget);
					match = GetMatchFromDir (selfDir, targetDir);
				}
				break;
			case BundleMode.FileOneToOne:
			case BundleMode.SceneOneToOne:
				{
					match = iTarget.Equals (path) ? 1.0f : 0.0f;
				}
				break;
			case BundleMode.None:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}

			return match;
		}

		/// <summary>
		/// 取得与目标匹配度（0.0f ~ 1.0f）.
		/// </summary>
		/// <returns>与目标匹配度（0.0f ~ 1.0f）.</returns>
		/// <param name="iSelf">自身.</param>
		/// <param name="iTarget">目标.</param>
		private float GetMatchFromDir(DirectoryInfo iSelf, DirectoryInfo iTarget) {
			var match = 0.0f;
			if (null == iSelf || null == iTarget) {
				return match;
			}

			var endDir = new DirectoryInfo (Application.dataPath);

			var selfMatch = CreateMatchDirLink (iSelf, endDir.Name);
			var targetMatch = CreateMatchDirLink (iTarget, endDir.Name);
			if (null == selfMatch) {
				return match;
			}

			// 计算匹配度
			var maxCount = 0;
			var matchedCount = 0;
			while (null != targetMatch) {

				if (null != selfMatch &&
					targetMatch.Name.Equals (selfMatch.Name)) {
					++matchedCount;
				}

				targetMatch = targetMatch.Next;
				selfMatch = selfMatch?.Next;
				++maxCount;
			}
			match = 0 >= maxCount ? 0.0f : matchedCount / (float)maxCount;
			return match;
		}

		/// <summary>
		/// 创建目录匹配链表.
		/// </summary>
		/// <returns>目录匹配链表.</returns>
		/// <param name="iDirInfo">目录信息.</param>
		/// <param name="iEndMark">结束标识.</param>
		private static MatchDirInfo CreateMatchDirLink(DirectoryInfo iDirInfo, string iEndMark) {

			if (null == iDirInfo) {
				return null;
			}
			if (string.IsNullOrEmpty(iEndMark)) {
				return null;
			}

			var curDir = new MatchDirInfo {Name = iDirInfo.Name};

			var parent = iDirInfo.Parent;
			while(null != parent) {

				var prevDir = new MatchDirInfo {Name = parent.Name};
				curDir.Prev = prevDir;
				prevDir.Next = curDir;

				parent = iEndMark.Equals (parent.Name) ? null : parent.Parent;
				curDir = prevDir;
			}

			return curDir;
		}
	}

	/// <summary>
	/// 非App资源信息.
	/// 注意：在打包时会自动移除到项目工程外部，以免被打进Apk/Ipa包中
	/// </summary>
	[Serializable]
	public class BundleUnResource : JsonDataBase<BundleUnResource>
	{
		/// <summary>
		/// 路径.
		/// </summary>
		[FormerlySerializedAs("AssetPath")] 
		public string path;

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			path = null;
		}
	}

	/// <summary>
	/// 资源匹配类.
	/// </summary>
	[Serializable]
	public class MatchResource : JsonDataBase<MatchResource> {
		/// <summary>
		/// 匹配度.
		/// </summary>
		[FormerlySerializedAs("Match")] 
		public float match;
		/// <summary>
		/// 目标.
		/// </summary>
		[FormerlySerializedAs("Target")] 
		public BundleResource target;
	}

	/// <summary>
	/// Bundles配置数据.
	/// </summary>
	[Serializable]
	public class BundlesConfigData : JsonDataBase<BundlesConfigData> {

		/// <summary>
		/// 资源列表.
		/// </summary>
		public List<BundleResource> resources = new List<BundleResource>();

		/// <summary>
		/// 非资源列表.
		/// </summary>
		public List<BundleUnResource> unResources = new List<BundleUnResource>();

		/// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
			base.Clear ();
			resources.Clear ();
			unResources.Clear ();
		}

		/// <summary>
		/// 追加资源.
		/// </summary>
		/// <param name="iMode">打包模式.</param>
		/// <param name="iResourcePath">资源路径</param>
		/// <param name="iIgnoreList">忽略列表.</param>
		public BundleResource AddResource(
			BundleMode iMode, 
			string iResourcePath,
			List<string> iIgnoreList) {

			if (string.IsNullOrEmpty (iResourcePath)) {
				return null;
			}

			BundleResource target = null;
			foreach (var loop in resources) {
				if (false == iResourcePath.Equals (loop.path)) {
					continue;
				}
				target = loop;
				break;
			}

			// 不存在
			if (null == target) {
				target = new BundleResource ();
				resources.Add (target);
				target.mode = iMode;
				target.path = iResourcePath;
				target.ignoreList = iIgnoreList;
			} else {

				target.mode = iMode;
				if (null == iIgnoreList || 1 > iIgnoreList.Count) return target;
				foreach (var ignore in iIgnoreList) {
					target.AddIgnoreTarget (ignore);
				}
			}

			return target;
		}

		/// <summary>
		/// 追加资源.
		/// </summary>
		/// <param name="iResourcePath">资源路径</param>
		public void RemoveResource(string iResourcePath) {

			if (string.IsNullOrEmpty (iResourcePath))
			{
				return;
			}

			foreach (var loop in resources) {
				if (false == iResourcePath.Equals (loop.path)) {
					continue;
				}
				resources.Remove(loop);
				break;
			}
		}

		/// <summary>
		/// 追加忽略目标.
		/// </summary>
		/// <param name="iIgnoreTarget">忽略目标.</param>
		public BundleResource AddIgnoreTarget(string iIgnoreTarget) {
			var target = GetMatchTarget (iIgnoreTarget);
			if (null != target) {
				target.AddIgnoreTarget (iIgnoreTarget);
			} else {
				Error ("AddIgnoreTarget():No match ProjectName for ignore!!(Ignore Target:{0})", 
					iIgnoreTarget);
			}
			return target;
		}

		/// <summary>
		/// 移除忽略目标.
		/// </summary>
		/// <param name="iIgnoreTarget">忽略目标.</param>
		public void RemoveIgnoreTarget(string iIgnoreTarget) {
			var target = GetMatchTarget (iIgnoreTarget);
			if (null != target ) {
				target.RemoveIgnoreTarget (iIgnoreTarget);
			} else {
				Error ("RemoveIgnoreTarget():No match ProjectName for ignore!!(Ignore Target:{0})", 
					iIgnoreTarget);
			}
		}

		/// <summary>
		/// 清空目标忽略列表.
		/// </summary>
		/// <param name="iIgnoreTarget">忽略目标.</param>
		public void ClearIgnore(string iIgnoreTarget) {
			var target = GetMatchTarget (iIgnoreTarget);
			if (null != target) {
				target.ignoreList.Clear ();
			} else {
				Error ("ClearIgnore():No match ProjectName for ignore!!(Ignore Target:{0})", 
					iIgnoreTarget);
			}
		}

		/// <summary>
		/// 取得匹配到的对象.
		/// </summary>
		/// <returns>匹配到的对象.</returns>
		/// <param name="iTarget">I ProjectName.</param>
		private BundleResource GetMatchTarget(string iTarget) {

			var matches = new List<MatchResource> ();
			foreach (var target in resources) {
				var match = new MatchResource ();
				matches.Add (match);

				match.match = target.GetMatch (iTarget);
				match.target = target;
			}

			var matchesTmp = matches
				.Where(iO => 0.0f < iO.match)
				.OrderByDescending(iO => iO.match)
				.ToArray ();
			if (0 < matchesTmp.Length) return matchesTmp[0].target;
			Error ("GetMatchTarget():No ProjectName matched!!(Target:{0})", 
				iTarget);
			return null;
		}
	}

	/// <summary>
	/// 资源打包配置信息.
	/// </summary>
	[Serializable]
	public class BundlesConfig : AssetBase<BundlesConfig, BundlesConfigData> {

		/// <summary>
		/// 资源列表.
		/// </summary>
		public List<BundleResource> Resources {
			get
			{
				return data?.resources;
			}
			set { 
				if (null != data) {
					data.resources = value;
				}
			}
		}

		/// <summary>
		/// 非资源列表.
		/// </summary>
		public List<BundleUnResource> UnResources {
			get
			{
				return data?.unResources;
			}
			set { 
				if (null != data) {
					data.unResources = value;
				}
			}
		}

#region Resource

		/// <summary>
		/// 添加资源信息.
		/// </summary>
		/// <param name="iResourceInfo">资源信息.</param>
		public BundleResource AddResource(BundleResource iResourceInfo) {
			if (null == data ) {
				return null;
			}
			if (null == iResourceInfo ) {
				return null;
			}
			return data.AddResource (
				iResourceInfo.mode, iResourceInfo.path, 
				iResourceInfo.ignoreList);
		}

		/// <summary>
		/// 添加资源信息.
		/// </summary>
		/// <param name="iMode">打包模式.</param>
		/// <param name="iResourcePath">资源路径.</param>
		/// <param name="iIgnoreList">忽略列表.</param>
		public BundleResource AddResource(
			BundleMode iMode, string iResourcePath, 
			List<string> iIgnoreList = null) {
		
			if (null == data ) {
				return null;
			}

			return string.IsNullOrEmpty (iResourcePath) ? null : 
				data.AddResource(iMode, iResourcePath, iIgnoreList);
		}

		/// <summary>
		/// 移除资源信息.
		/// </summary>
		/// <returns><c>true</c>, 移除成功, <c>false</c> 移除失败.</returns>
		/// <param name="iResourcePath">资源路径.</param>
		public void RemoveResource(string iResourcePath) {
			if (null == data )
			{
				return;
			}

			if (string.IsNullOrEmpty (iResourcePath))
			{
				return;
			}

			data.RemoveResource(iResourcePath);
		}

		/// <summary>
		/// 清空资源列表.
		/// </summary>
		public void ClearResources() {
			if (Resources == null) {
				return;
			}
			Resources.Clear ();

			UtilsAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 添加忽略列表.
		/// </summary>
		/// <returns>资源信息.</returns>
		/// <param name="iResourcePath">资源路径.</param>
		/// <param name="iIgnoreTarget">忽略对象.</param>
		public BundleResource AddIgnoreTarget(
			string iResourcePath, 
			string iIgnoreTarget)
		{
			if (null == data ) {
				return null;
			}

			if (string.IsNullOrEmpty (iResourcePath)) {
				return null;
			}

			return string.IsNullOrEmpty (iIgnoreTarget) ? null : 
				data.AddIgnoreTarget(iIgnoreTarget);
		}

		/// <summary>
		/// 判断当前文件是否为指定目标的忽略文件.
		/// </summary>
		/// <returns><c>true</c>, 忽略文件, <c>false</c> 非忽略文件.</returns>
		/// <param name="iTarget">目标.</param>
		/// <param name="iTargetFile">目标文件.</param>
		public bool IsIgnoreFile(BundleResource iTarget, string iTargetFile) {
			if (iTarget?.ignoreList == null || string.IsNullOrEmpty(iTargetFile) ) {
				return false;
			}
			return iTarget.IsIgnoreTargetExist(iTargetFile);
		}

		/// <summary>
		/// 清空指定资源对象的忽略列表.
		/// </summary>
		/// <param name="iTarget">目标.</param>
		/// <param name="iIgnoreTarget">忽略目标</param>
		public void RemoveIgnoreInfo(string iTarget, string iIgnoreTarget) {
			if (null == data) {
				return;
			}
			if (string.IsNullOrEmpty(iTarget)) {
				return;
			}
			if (string.IsNullOrEmpty(iIgnoreTarget)) {
				return;
			}
			data.RemoveIgnoreTarget (iIgnoreTarget);
		}

		/// <summary>
		/// 清空指定资源对象的忽略列表.
		/// <param name="iTarget">目标.</param>
		/// </summary>
		public void ClearAllIgnoreInfo(string iTarget)
		{
			data?.ClearIgnore(iTarget);
		}
		
#endregion

#region UnResource

		/// <summary>
		/// 添加非App资源信息.
		/// </summary>
		/// <param name="iUnResourceInfo">非App资源信息</param>
		public BundleUnResource AddUnResource(BundleUnResource iUnResourceInfo)
		{
			return iUnResourceInfo == null ? null : AddUnResource(iUnResourceInfo.path);
		}

		/// <summary>
		/// 添加非App资源信息.
		/// </summary>
		/// <param name="iUnResourcePath">非App资源信息</param>
		public BundleUnResource AddUnResource(string iUnResourcePath) {
			BundleUnResource bur;

			// 不存在存在
			if (IsUnResoureExist (iUnResourcePath, out bur) == false) {
				bur = new BundleUnResource();
				UnResources.Add (bur);
			}
			if (bur != null) {
				bur.path = iUnResourcePath;
				UtilsAsset.SetAssetDirty (this);
			} else {
				this.Error("AddUnResource()::Failed!!!(AssetPath:{0})",
					iUnResourcePath);
			}
			return bur;
		}

		/// <summary>
		/// 移除非资源信息.
		/// </summary>
		/// <returns><c>true</c>, 移除成功, <c>false</c> 移除失败.</returns>
		/// <param name="iUnResourcePath">非资源信息.</param>
		public void RemoveUnResource(string iUnResourcePath) {
			BundleUnResource bur;
			// 不存在存在
			if (!IsUnResoureExist(iUnResourcePath, out bur)) return;
			if (bur == null) return;
			UnResources.Remove (bur);
			UtilsAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 清空非资源列表.
		/// </summary>
		public void ClearUnResources() {
			if (UnResources == null) {
				return;
			}
			UnResources.Clear ();
			UtilsAsset.SetAssetDirty (this);
		}

		/// <summary>
		/// 判断非资源信息是否存在.
		/// </summary>
		/// <returns><c>true</c>, 村贼, <c>false</c> 不存在.</returns>
		/// <param name="iUnResourcePath">非资源路径.</param>
		/// <param name="iBur">非资源信息.</param>
		private bool IsUnResoureExist(string iUnResourcePath, out BundleUnResource iBur) {

			var bolRet = false;
			iBur = null;
			foreach(var bur in UnResources)
			{
				if (bur.path != iUnResourcePath) continue;
				iBur = bur;
				bolRet = true;
				break;
			}

			return bolRet;
		}

#endregion

#region Implement

		/// <summary>
		/// 用用数据.
		/// </summary>
		/// <param name="iData">数据.</param>
		/// <param name="iForceClear">强制清空标志位.</param>
		protected override void ApplyData (BundlesConfigData iData, bool iForceClear = true) {

			if (null == iData) {
				return;
			}

			// 清空
			if (iForceClear) {
				Clear ();
			}

			data.resources = iData.resources;
			data.unResources = iData.unResources;

			UtilsAsset.SetAssetDirty (this);
		}

#endregion
	}

}