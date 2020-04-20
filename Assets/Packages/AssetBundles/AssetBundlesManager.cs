using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Packages.Common.Base;
using Packages.Settings;

namespace Packages.AssetBundles
{

    /// <summary>
    /// Asset bundle 类型.
    /// </summary>
    public enum AssetBundleType
    {
        /// <summary>
        /// 无.
        /// </summary>
        None,
        /// <summary>
        /// 预制体.
        /// </summary>
        Prefab,
        /// <summary>
        /// 音效.
        /// </summary>
        Audio,
        /// <summary>
        /// 材质.
        /// </summary>
        Mat,
        /// <summary>
        /// 精灵/纹理.
        /// </summary>
        SpriteTexture,
        /// <summary>
        /// 图集.
        /// </summary>
        SpriteAtlas,
        /// <summary>
        /// (.asset)文件.
        /// </summary>
        Asset,
        /// <summary>
        /// (.txt)文件.
        /// </summary>
        Txt,
        /// <summary>
        /// (.json)文件.
        /// </summary>
        Json,
        /// <summary>
        /// 场景.
        /// </summary>
        Scene,
    }

    [Serializable]
    public class BundleConf : JsonDataBase<BundleConf> {

        /// <summary>
        /// Bundle类型(0:Normal; 1:Scene)
        /// </summary>
        [FormerlySerializedAs("BundleType")] 
        public BundleType bundleType = BundleType.Normal;

        /// <summary>
        /// 文件类型(0:MainManifest; 1:NormalManifest; 2:Bundle)
        /// </summary>
        [FormerlySerializedAs("FileType")] 
        public BundleFileType fileType = BundleFileType.Bundle;

        /// <summary>
        /// 版本号
        /// </summary>
        [FormerlySerializedAs("Version")] 
        public int version;

        /// <summary>
        /// 文件列表
        /// </summary>
        [FormerlySerializedAs("Files")] 
        public List<string> files = new List<string>();

        public BundleConf()
        {
            version = 0;
        }

        /// <summary>
		/// 清空.
		/// </summary>
		public override void Clear() {
            base.Clear();
            bundleType = BundleType.Normal;
            fileType = BundleFileType.Bundle;
            version = 1;
            files.Clear();
        }
    }


    /// <summary>
    /// 资源管理器.
    /// </summary>
    public class AssetBundlesManager : SingletonBase<AssetBundlesManager>, IDisposable
    {
        /// <summary>
        /// 备份目录.
        /// </summary>
        public static string BackUpDir;

        /// <summary>
        /// 备份目录(Assets).
        /// </summary>
        public static readonly string BackUpDirOfAssets = string.Format("{0}/Assets", BackUpDir);

        /// <summary>
        /// 备份目录(Bundles).
        /// </summary>
        public static readonly string BackUpDirOfBundles = string.Format("{0}/Bundles", BackUpDir);

        /// <summary>
        /// Bundles配置信息列表
        /// </summary>
        /// <typeparam>Bundle ID
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>Bundles配置信息
        ///     <name>BundleConf</name>
        /// </typeparam>
        protected Dictionary<string, BundleConf> BundleConfs;

        /// <summary>
        /// Bundle包的文件依赖关系列表.
        /// <typeparam>文件路径
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> BundlesMap = new Dictionary<string, string>();

        /// <summary>
        /// 场景文件关系列表.
        /// <typeparam>场景名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> ScenesMap = new Dictionary<string, string>();

        /// <summary>
        /// 预制体列表.
        /// <typeparam>预制体名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> PrefabsMap = new Dictionary<string, string>();

        /// <summary>
        /// 音效列表.
        /// <typeparam>音效名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> AudiosMap = new Dictionary<string, string>();

        /// <summary>
        ///	材质列表.
        /// <typeparam>材质名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> MatsMap = new Dictionary<string, string>();

        /// <summary>
        ///	纹理列表.
        /// <typeparam>纹理名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> TexturesMap = new Dictionary<string, string>();

        /// <summary>
        ///	图集列表.
        /// <typeparam>图集名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> SpriteAtlasMap = new Dictionary<string, string>();

        /// <summary>
        ///	(.asset)文件列表.
        /// <typeparam>文件名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> AssetFilesMap = new Dictionary<string, string>();

        /// <summary>
        ///	(.text)文件列表.
        /// <typeparam>文件名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> TextsMap = new Dictionary<string, string>();

        /// <summary>
        ///	(.json)文件列表.
        /// <typeparam>文件名
        ///     <name>string</name>
        /// </typeparam>
        /// <typeparam>BundleID
        ///     <name>string</name>
        /// </typeparam>
        /// </summary>
        public Dictionary<string, string> JsonFilesMap = new Dictionary<string, string>();

        /// <summary>
        /// 已经加载bundles列表.
        /// </summary>
        private Dictionary<string, AssetBundle> _bundles = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// 主Bundle（StreamingAssets）.
        /// </summary>
        private AssetBundle _mainBundle;

        /// <summary>
        /// 主manifest（StreamingAssets）.
        /// </summary>
        private AssetBundleManifest _mainManifest;

        public AssetBundlesManager()
        {
            _mainBundle = null;
        }

        /// <summary>
        /// 释放函数.
        /// </summary>
        public void Dispose()
        {

            // 释放已有Bundle
            ReleaseAllBundle();

            BundlesMap?.Clear();
            ScenesMap?.Clear();
            PrefabsMap?.Clear();
            AudiosMap?.Clear();
            MatsMap?.Clear();
            TexturesMap?.Clear();
            SpriteAtlasMap?.Clear();
            AssetFilesMap?.Clear();
            TextsMap?.Clear();
            JsonFilesMap?.Clear();
            if (_mainBundle != null)
            {
                _mainBundle.Unload(false);
            }
        }

        public override void Reset()
        {
            Init();
        }

        /// <summary>
        /// 初始化.
        /// </summary>
        protected override void Init()
        {
            // 备份目录
            BackUpDir = $"{Application.dataPath}/../BackUp";
        }

        /// <summary>
        /// 推送资源信息
        /// </summary>
        /// <param name="iResources">资源列表信息</param>
        public void PushResources(Dictionary<string, BundleConf> iResources) {
            if(null == BundleConfs) {
                BundleConfs = new Dictionary<string, BundleConf>();
            }
            BundleConfs.Clear();

            // 遍历资源列表
            foreach(var it in iResources) {
                BundleConf conf;
                if (BundleConfs.TryGetValue(it.Key, out conf)) continue;
                conf = new BundleConf();
                BundleConfs.Add(it.Key, conf);

                conf.bundleType = it.Value.bundleType;
                conf.fileType = it.Value.fileType;
                conf.version = it.Value.version;
            }
        }

        /// <summary>
        /// 推送资源依赖信息
        /// </summary>
        /// <param name="iBundles">资源依赖列表</param>
        public void PushBundles(Dictionary<string, BundleConf> iBundles) {

            if(null == BundleConfs) {
                BundleConfs = new Dictionary<string, BundleConf>();
            }

            // 遍历资源依赖列表
            foreach(var it in iBundles) {
                BundleConf conf;
                if(false == BundleConfs.TryGetValue(it.Key, out conf)) {
                    Error("PushBundles()::The bundle info is invalid or not exist!!!(ResourceID:{0})",
                        it.Key);
                    continue;
                }
                if(null == conf) {
                    Error("PushBundles()::The bundle info is invalid or not exist!!!(ResourceID:{0})",
                        it.Key);
                    continue;
                }
                if(it.Value.bundleType == conf.bundleType && 
                    it.Value.fileType == conf.fileType && 
                    it.Value.version == conf.version) {
                    conf.files.Clear();
                    conf.files.AddRange(it.Value.files);
                } else {
                    Error("PushBundles()::The resources info and asset bundle info is not matched!!!(ResourceID:{0})",
                        it.Key);
                }
            }

            if (null == BundleConfs || 0 >= BundleConfs.Count) {
                Error("PushBundles():There is no info in SBundlesMap!!!");
                return;
            }
            // 遍历信息
            foreach(var it in BundleConfs) {
                foreach (var filePath in it.Value.files) {
                    var key = filePath;
                    var assetType = AssetFileHelper.GetAssetTypeByFilePath(filePath);
                    switch (assetType)
                    {
                        case AssetBundleType.Scene:
                            ScenesMap[key] = it.Key;
                            break;
                        case AssetBundleType.Prefab:
                            PrefabsMap[key] = it.Key;
                            break;
                        case AssetBundleType.Audio:
                            AudiosMap[key] = it.Key;
                            break;
                        case AssetBundleType.Mat:
                            MatsMap[key] = it.Key;
                            break;
                        case AssetBundleType.SpriteTexture:
                            TexturesMap[key] = it.Key;
                            break;
                        case AssetBundleType.SpriteAtlas:
                            SpriteAtlasMap[key] = it.Key;
                            break;
                        case AssetBundleType.Asset:
                            AssetFilesMap[key] = it.Key;
                            break;
                        case AssetBundleType.Txt:
                            TextsMap[key] = it.Key;
                            break;
                        case AssetBundleType.Json:
                            JsonFilesMap[key] = it.Key;
                            break;
                        case AssetBundleType.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    BundlesMap[key] = it.Key;
                }
            }
        }

        /// <summary>
        /// 取得Map的Key.
        /// </summary>
        /// <returns>文件名.</returns>
        /// <param name="iFilePath">文件路径.</param>
        /// <param name="iAssetType">Asset类型</param>
        public static string GetKeyOfMapByFilePath(string iFilePath, Type iAssetType)
        {
            return AssetFileHelper.FixedFileSunffix(iFilePath, iAssetType);
        }
        
#region 异步加载

        /// <summary>
        /// 加载预制体(多个).
        /// </summary>
        /// <param name="iPrefabNames">预制体名列表</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        public IEnumerator LoadPrefabsAsync(
            string[] iPrefabNames, Action<bool, string, AssetBundleType, GameObject> iLoadCompleted)
        {

            if (iPrefabNames != null &&
                iPrefabNames.Length > 0)
            {
                foreach (var prefabName in iPrefabNames)
                {
                    yield return LoadPrefabAsync(prefabName, iLoadCompleted);
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        /// <summary>
        /// 加载预制体（单个）.
        /// </summary>
        /// <param name="iPrefabName">预制体名.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadPrefabAsync(
            string iPrefabName,
            Action<bool, string, AssetBundleType, GameObject> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, GameObject>, IEnumerator> iLoadFailed = null)
        {

            if (string.IsNullOrEmpty(iPrefabName) == false)
            {
                yield return LoadFromAssetBundleAsync<GameObject>(
                    iPrefabName, iLoadCompleted, iLoadFailed, AssetBundleType.Prefab);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 加载音效（单个）.
        /// </summary>
        /// <param name="iAudioName">音效名.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadAudioAsync(
            string iAudioName,
            Action<bool, string, AssetBundleType, AudioClip> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, AudioClip>, IEnumerator> iLoadFailed = null)
        {

            if (string.IsNullOrEmpty(iAudioName) == false)
            {
                yield return LoadFromAssetBundleAsync<AudioClip>(
                    iAudioName, iLoadCompleted, iLoadFailed, AssetBundleType.Audio);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 加载音效（多个）.
        /// </summary>
        /// <param name="iAudioNames">音效列表.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadAudiosAsync(
            string[] iAudioNames,
            Action<bool, string, AssetBundleType, AudioClip> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, AudioClip>, IEnumerator> iLoadFailed = null)
        {

            if (iAudioNames != null &&
                iAudioNames.Length > 0)
            {
                foreach (var audioName in iAudioNames)
                {
                    yield return LoadAudioAsync(audioName, iLoadCompleted, iLoadFailed);
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        /// <summary>
        /// 加载材质（单个）.
        /// </summary>
        /// <param name="iMatName">材质名.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadMatAsync(
            string iMatName,
            Action<bool, string, AssetBundleType, Material> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, Material>, IEnumerator> iLoadFailed = null)
        {
            if (string.IsNullOrEmpty(iMatName) == false)
            {
                yield return LoadFromAssetBundleAsync<Material>(
                    iMatName, iLoadCompleted, iLoadFailed, AssetBundleType.Mat);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 加载材质（多个）.
        /// </summary>
        /// <param name="iMatNames">材质列表.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadMatsAsync(
            string[] iMatNames,
            Action<bool, string, AssetBundleType, Material> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, Material>, IEnumerator> iLoadFailed = null)
        {

            if (iMatNames != null &&
                iMatNames.Length > 0)
            {
                foreach (var matName in iMatNames)
                {
                    yield return LoadMatAsync(matName, iLoadCompleted, iLoadFailed);
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        /// <summary>
        /// 加载纹理（单个）.
        /// </summary>
        /// <param name="iTextureName">纹理名.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadTextureAsync(
            string iTextureName,
            Action<bool, string, AssetBundleType, Texture> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, Texture>, IEnumerator> iLoadFailed = null)
        {

            if (string.IsNullOrEmpty(iTextureName) == false)
            {
                yield return LoadFromAssetBundleAsync<Texture>(
                    iTextureName, iLoadCompleted, iLoadFailed, AssetBundleType.SpriteTexture);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 加载纹理（多个）.
        /// </summary>
        /// <param name="iTextureNames">纹理列表</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadTexturesAsync(
            string[] iTextureNames,
            Action<bool, string, AssetBundleType, Texture> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, Texture>, IEnumerator> iLoadFailed = null)
        {

            if (iTextureNames != null &&
                iTextureNames.Length > 0)
            {
                foreach (var textureName in iTextureNames)
                {
                    yield return LoadTextureAsync(textureName, iLoadCompleted, iLoadFailed);
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        /// <summary>
        /// 加载(.asset)文件（单个）.
        /// </summary>
        /// <param name="iAssetFileName">(.asset)文件名.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadAssetFileAsync(
            string iAssetFileName,
            Action<bool, string, AssetBundleType, TextAsset> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, TextAsset>, IEnumerator> iLoadFailed = null)
        {

            if (string.IsNullOrEmpty(iAssetFileName) == false)
            {
                yield return LoadFromAssetBundleAsync<TextAsset>(
                    iAssetFileName, iLoadCompleted, iLoadFailed, AssetBundleType.Asset);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 加载(.asset)文件（多个）.
        /// </summary>
        /// <param name="iAssetFileNames">(.asset)文件列表.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadAssetFilesAsync(
            string[] iAssetFileNames,
            Action<bool, string, AssetBundleType, TextAsset> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, TextAsset>, IEnumerator> iLoadFailed = null)
        {

            if (iAssetFileNames != null &&
                iAssetFileNames.Length > 0)
            {
                foreach (var assetFileName in iAssetFileNames)
                {
                    yield return LoadAssetFileAsync(assetFileName, iLoadCompleted, iLoadFailed);
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        /// <summary>
        /// 加载(.text)文件（单个）.
        /// </summary>
        /// <param name="iTextFileName">(.text)文件名.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadTextFileAsync(
            string iTextFileName,
            Action<bool, string, AssetBundleType, TextAsset> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, TextAsset>, IEnumerator> iLoadFailed = null)
        {

            if (string.IsNullOrEmpty(iTextFileName) == false)
            {
                yield return LoadFromAssetBundleAsync<TextAsset>(
                    iTextFileName, iLoadCompleted, iLoadFailed, AssetBundleType.Txt);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 加载(.text)文件（多个）.
        /// </summary>
        /// <param name="iTextFileNames">(.text)文件列表.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadTextFilesAsync(
            string[] iTextFileNames,
            Action<bool, string, AssetBundleType, TextAsset> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, TextAsset>, IEnumerator> iLoadFailed = null)
        {

            if (iTextFileNames != null &&
                iTextFileNames.Length > 0)
            {
                foreach (var textFileName in iTextFileNames)
                {
                    yield return LoadTextFileAsync(textFileName, iLoadCompleted, iLoadFailed);
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        /// <summary>
        /// 加载(.json)文件（单个）.
        /// </summary>
        /// <param name="iJsonFileName">(.json)文件名.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadJsonFileAsync(
            string iJsonFileName,
            Action<bool, string, AssetBundleType, TextAsset> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, TextAsset>, IEnumerator> iLoadFailed = null)
        {

            if (string.IsNullOrEmpty(iJsonFileName) == false)
            {
                yield return LoadFromAssetBundleAsync<TextAsset>(
                    iJsonFileName, iLoadCompleted, iLoadFailed, AssetBundleType.Json);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 加载(.json)文件（多个）.
        /// </summary>
        /// <param name="iJsonFileNames">(.json)文件列表.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadJsonFilesAsync(
            string[] iJsonFileNames,
            Action<bool, string, AssetBundleType, TextAsset> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, TextAsset>, IEnumerator> iLoadFailed = null)
        {

            if (iJsonFileNames != null &&
                iJsonFileNames.Length > 0)
            {
                foreach (var jsonFileName in iJsonFileNames)
                {
                    yield return LoadJsonFileAsync(jsonFileName, iLoadCompleted, iLoadFailed);
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        /// <summary>
        /// 异步加载asset bundle(多个).
        /// </summary>
        /// <param name="iFilePaths">文件路径列表.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadAssetBundlesAsync(
            string[] iFilePaths,
            Action<bool, string, AssetBundleType, Object> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, Object>, IEnumerator> iLoadFailed = null)
        {

            if (iFilePaths == null ||
                iFilePaths.Length <= 0)
            {
                yield break;
            }

            foreach (var filePath in iFilePaths)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    continue;
                }
                yield return LoadAssetBundleAsync(filePath, iLoadCompleted, iLoadFailed);
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// 异步加载asset bundle(单个).
        /// </summary>
        /// <param name="iFilePath">文件路径列表</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iLoadFailed">加载失败.</param>
        public IEnumerator LoadAssetBundleAsync(
            string iFilePath,
            Action<bool, string, AssetBundleType, Object> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, Object>, IEnumerator> iLoadFailed = null)
        {
            if (string.IsNullOrEmpty(iFilePath) == false)
            {
                yield return LoadFromAssetBundleAsync<Object>(
                    iFilePath, iLoadCompleted, iLoadFailed);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 异步加载asset bundle(单个).(add by zhan)
        /// </summary>
        /// <param name="iFilePath">文件路径列表</param>
        /// <param name="iType">类型</param>
        /// <param name="iLoadSuccess">加载成功.</param>
        public IEnumerator LoadAssetBundleAsync(
            string iFilePath, Type iType,
            Action<string, AssetBundleType, Object> iLoadSuccess)
        {
            if (string.IsNullOrEmpty(iFilePath) == false)
            {
                yield return LoadFromAssetBundleAsync(iFilePath, iType, iLoadSuccess);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 异步加载AssetBundle.
        /// </summary>
        /// <param name="iKey">Key.</param>
        /// <param name="iType">类型</param>
        /// <param name="iAssetType">类型.</param>
        /// <param name="iLoadSuccess">加载成功</param>
        private IEnumerator LoadFromAssetBundleAsync(
            string iKey, Type iType,
            Action<string, AssetBundleType, Object> iLoadSuccess,
            AssetBundleType iAssetType = AssetBundleType.None) {

            var bundleId = GetBundleId(iKey, iAssetType);
            if (string.IsNullOrEmpty(bundleId)) {
                yield break;
            }
            // 预先加载依赖相关的bundle
            BundleConf target;
            if (null == BundleConfs || 
                false == BundleConfs.TryGetValue(bundleId, out target)) {
                yield break;
            }
            yield return LoadAssetBundleAsync(bundleId, target.fileType);
            var bundle = GetBundleById(bundleId);
            if (null == bundle)
            {
                Error("LoadFromAssetBundleAsync()::Failed!!! Bundle Id:{0} AssetPath:{1}",
                    bundleId, iKey);
                yield break;
            }

            Info("LoadFromAssetBundleAsync()::Succeeded. Bundle Id:{0} AssetPath:{1}",
                bundleId, iKey);

            var request = bundle.LoadAssetAsync(iKey, iType);// fix by zhan
            yield return request;
            if (null == request.asset)
            {
                Error("LoadFromAssetBundleAsync()::There is no ProjectName in assetBundle!!!(BundleId:{0} Key:{1})", 
                    bundleId, iKey);
                yield break;
            }

            if (iLoadSuccess != null) iLoadSuccess.Invoke(iKey, iAssetType, request.asset);
            yield return new WaitForEndOfFrame();
        }

        /// <summary>
        /// 异步加载AssetBundle.
        /// </summary>
        /// <param name="iKey">Key.</param>
        /// <param name="iLoadCompleted">加载完成.</param>
        /// <param name="iRetryLoad">加载重试.</param>
        /// <param name="iAssetType">类型.</param>
        public IEnumerator LoadFromAssetBundleAsync<T>(
            string iKey,
            Action<bool, string, AssetBundleType, T> iLoadCompleted,
            Func<string, Action<bool, string, AssetBundleType, T>, IEnumerator> iRetryLoad,
            AssetBundleType iAssetType = AssetBundleType.None) where T : Object
        {

            var bundleId = GetBundleId(iKey, iAssetType);
            if (string.IsNullOrEmpty(bundleId))
            {
                if (null != iRetryLoad)
                {
                    yield return iRetryLoad(iKey, iLoadCompleted);
                } else {
                    iLoadCompleted(false, iKey, iAssetType, null);
                }
            }
            else
            {
                // 预先加载依赖相关的bundle
                BundleConf target;
                if (null == BundleConfs || 
                    false == BundleConfs.TryGetValue(bundleId, out target)) {
                    // 有重试，则重试。无则返回加载失败
                    if (null != iRetryLoad)
                    {
                        yield return iRetryLoad(iKey, iLoadCompleted);
                    } else {
                        iLoadCompleted(false, iKey, iAssetType, null);
                    }
                }
                else
                {
                    if (null != target) {
                        yield return LoadAssetBundleAsync(bundleId, target.fileType);
                        yield return new WaitForEndOfFrame();

                        var bundle = GetBundleById(bundleId);
                        if (null == bundle)
                        {
                            Error("LoadFromAssetBundleAsync():Failed!!! Bundle Id:{0} AssetPath:{1}",
                                bundleId, iKey);
                            if (null != iRetryLoad)
                            {
                                yield return iRetryLoad(iKey, iLoadCompleted);
                            } else {
                                iLoadCompleted(false, iKey, iAssetType, null);
                            }
                        }
                        else
                        {
                            Info("LoadFromAssetBundleAsync():Succeeded. Bundle Id:{0} AssetPath:{1}",
                                bundleId, iKey);

                            var request = bundle.LoadAssetAsync<T>(iKey);
                            yield return request;

                            //获取加载的对象，并创建出来
                            var objTmp = request.asset as T;
                            if (null != objTmp)
                            {
                                if (iLoadCompleted == null) yield break;
                                iLoadCompleted(true, iKey, iAssetType, objTmp);
                                yield return new WaitForEndOfFrame();
                                yield break;
                            }

                            Error("LoadFromAssetBundleAsync():The type of ProjectName is invalided!!!(BundleId:{0} Key:{1} Type:{2})",
                                bundleId, iKey, typeof(T).ToString());
                            if (null != iRetryLoad)
                            {
                                yield return iRetryLoad(iKey, iLoadCompleted);
                            } else {
                                iLoadCompleted(false, iKey, iAssetType, null);
                            }
                        }
                    }
                    else
                    {
                        Warning("LoadFromAssetBundleAsync():There is no ProjectName in download list!!!(BundleId:{0})", bundleId);
                        if (null != iRetryLoad)
                        {
                            yield return iRetryLoad(iKey, iLoadCompleted);
                        }
                    }
                }
            }
            yield return null;
        }

        /// <summary>
        /// 加载AssetBundle(异步).
        /// </summary>
        /// <returns>AssetBundle.</returns>
        /// <param name="iBundleId">BundleId.</param>
        /// <param name="iFileType">文件类型.</param>
        private IEnumerator LoadAssetBundleAsync(
            string iBundleId, BundleFileType iFileType)
        {

            if (string.IsNullOrEmpty(iBundleId))
            {
                yield break;
            }

            if (CheckMainManifest() &&
                _mainManifest != null)
            {
                yield return new WaitForEndOfFrame();

                var dependName = BundlesResult.GetLocalBundleFileName(iBundleId, iFileType);
                if (string.IsNullOrEmpty(dependName))
                {
                    yield break;
                }
                var depends = _mainManifest.GetAllDependencies(dependName);
                foreach (var t in depends)
                {
                    var dependBundleId = ConvertDependNameToBundleId(t);
                    if (string.IsNullOrEmpty(dependBundleId))
                    {
                        continue;
                    }
                    yield return LoadBundleByIdAsync(dependBundleId);
                    yield return new WaitForEndOfFrame();
                }

                yield return LoadBundleByIdAsync(iBundleId);
                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        /// <summary>
        /// 加载Bundle(异步加载).
        /// </summary>
        /// <returns>Bundle.</returns>
        /// <param name="iBundleId">BundleID</param>
        private IEnumerator LoadBundleByIdAsync(string iBundleId)
        {
            if (null == _bundles)
            {
                _bundles = new Dictionary<string, AssetBundle>();
            }

            if (false == _bundles.ContainsKey(iBundleId))
            {
                var path = GetBundleFullPath(iBundleId);
                if (File.Exists(path))
                {
                    var request = AssetBundle.LoadFromFileAsync(path);
                    if(null != request) {
                        _bundles[iBundleId] = request.assetBundle;
                    }
                    yield return request;
                }
                else
                {
                    Error("LoadBundleByIdAsync():The assetbundle file is not exist!!!![AssetPath:{0}]", path);
                }
            }
            yield return null;

        }

#endregion

#region 同步加载

        /// <summary>
        /// 加载场景.
        /// </summary>
        /// <param name="iSceneName">场景名.</param>
        public bool LoadScene(string iSceneName)
        {

            if (string.IsNullOrEmpty(iSceneName))
            {
                return false;
            }

            if (LoadSceneAssetBundle(iSceneName))
            {
                // 加载场景
                SceneManager.LoadScene(iSceneName);

            }

            return true;
        }

        /// <summary>
        /// 加载预制体.
        /// </summary>
        /// <param name="iPrefabName">预制体名.</param>
        /// <param name="iParent">父节点.</param>
        /// <param name="iPosition">坐标.</param>
        /// <param name="iScale">缩放.</param>
        public bool LoadPrefab(
            string iPrefabName,
            GameObject iParent,
            Vector3 iPosition,
            Vector3 iScale)
        {

            if (string.IsNullOrEmpty(iPrefabName))
            {
                return false;
            }

            if (iParent == null)
            {
                return false;
            }

            var objPrefab = LoadPreFab(iPrefabName);
            if (objPrefab == null)
            {
                return false;
            }
            var prefab = Object.Instantiate(objPrefab, iParent.transform, true) as GameObject;
            if (prefab == null)
            {
                return false;
            }

            prefab.transform.localPosition = iPosition;
            prefab.transform.localScale = iScale;

            return true;
        }

        /// <summary>
        /// 加载预制体.
        /// </summary>
        /// <returns>预制体.</returns>
        /// <param name="iPrefabName">预制体名.</param>
        public Object LoadPreFab(string iPrefabName)
        {
            return LoadFromAssetBundle<UnityEngine.Object>(
                iPrefabName, AssetBundleType.Prefab);
        }

        /// <summary>
        /// 加载音效（无需后缀）.
        /// </summary>
        /// <returns>音效.</returns>
        /// <param name="iAudioName">音效名.</param>
        public AudioClip LoadAudio(string iAudioName)
        {
            return LoadFromAssetBundle<AudioClip>(
                iAudioName, AssetBundleType.Audio);
        }

        /// <summary>
        /// 加载材质（无需后缀）.
        /// </summary>
        /// <returns>材质.</returns>
        /// <param name="iMatName">材质名.</param>
        public Material LoadMaterial(string iMatName)
        {
            return LoadFromAssetBundle<Material>(
                iMatName, AssetBundleType.Mat);
        }

        /// <summary>
        /// 加载纹理.
        /// </summary>
        /// <returns>纹理.</returns>
        /// <param name="iTextureName">纹理名.</param>
        public Texture2D LoadTexture(string iTextureName)
        {
            return LoadFromAssetBundle<Texture2D>(
                iTextureName, AssetBundleType.SpriteTexture);
        }

        /// <summary>
        /// 加载图集.
        /// </summary>
        /// <returns>纹理.</returns>
        /// <param name="iTextureName">纹理名.</param>
        public UnityEngine.U2D.SpriteAtlas LoadSpriteAtlas(string iTextureName)
        {
            return LoadFromAssetBundle<UnityEngine.U2D.SpriteAtlas>(
                iTextureName, AssetBundleType.SpriteAtlas);
        }

        /// <summary>
        /// 加载Asset文件(.asset).
        /// </summary>
        /// <returns>Asset文件(.asset).</returns>
        /// <param name="iAssetFileName">Asset文件名(.asset).</param>
        public TextAsset LoadAssetFile(string iAssetFileName)
        {
            return LoadFromAssetBundle<UnityEngine.TextAsset>(
                iAssetFileName, AssetBundleType.Asset);
        }

        /// <summary>
        /// 加载文本文件(.text).
        /// </summary>
        /// <returns>文本文件(.text).</returns>
        /// <param name="iTextFileName">文本文件名(.text).</param>
        public string LoadTextFile(string iTextFileName)
        {
            var objTmp = LoadFromAssetBundle<UnityEngine.TextAsset>(
                iTextFileName, AssetBundleType.Txt);
            return objTmp != null ? objTmp.text : null;
        }

        /// <summary>
        /// 加载Json文件(.json).
        /// </summary>
        /// <returns>Json文件(.json).</returns>
        /// <param name="iJsonFileName">Json文件名(.json)</param>
        public string LoadJsonFile(string iJsonFileName)
        {
            var objTmp = LoadFromAssetBundle<UnityEngine.TextAsset>(
                iJsonFileName, AssetBundleType.Json);
            return objTmp != null ? objTmp.text : null;
        }

        /// <summary>
        /// 加载Json文件(.json).
        /// </summary>
        /// <returns>Json文件(.json).</returns>
        /// <param name="iJsonFileName">Json文件名(.json).</param>
        public T LoadJsonFile<T>(string iJsonFileName)
        {
            var jsonFileStr = LoadJsonFile(iJsonFileName);
            return string.IsNullOrEmpty(jsonFileStr) == false ? JsonUtility.FromJson<T>(jsonFileStr) : default(T);
        }

        /// <summary>
        /// 加载asset bundle.
        /// </summary>
        /// <param name="iFilePath">文件路径.</param>
        /// <param name="iRetType">返回类型</param>
        public Object LoadAssetBundle(string iFilePath, Type iRetType)
        {
            var _fileName = GetKeyOfMapByFilePath(iFilePath, iRetType);
            if (string.IsNullOrEmpty(_fileName))
            {
                return null;
            }

            var objTmp = LoadFromAssetBundle(
                _fileName, iRetType);
            return objTmp != null ? objTmp : null;
        }

        /// <summary>
        /// 加载asset bundle.
        /// </summary>
        /// <param name="iFilePath">文件路径.</param>
        public T LoadAssetBundle<T>(string iFilePath) where T : Object
        {
            var _fileName = GetKeyOfMapByFilePath(iFilePath, typeof(T));
            if (string.IsNullOrEmpty(_fileName))
            {
                return null;
            }

            var objTmp = LoadFromAssetBundle<T>(
                _fileName);
            return objTmp != null ? objTmp : null;
        }

        /// <summary>
        /// 加载AssetBundle.
        /// </summary>
        /// <returns>AssetBundle.</returns>
        /// <param name="iBundleId">BundleId.</param>
        /// <param name="iFileType">文件类型.</param>
        private UnityEngine.AssetBundle LoadAssetBundle(
            string iBundleId, BundleFileType iFileType)
        {

            if (string.IsNullOrEmpty(iBundleId))
            {
                return null;
            }

            if (CheckMainManifest() == false)
            {
                return null;
            }
            if (_mainManifest == null)
            {
                return null;
            }

            var dependName = BundlesResult.GetLocalBundleFileName(iBundleId, iFileType);
            if (string.IsNullOrEmpty(dependName))
            {
                return null;
            }
            var depends = _mainManifest.GetAllDependencies(dependName);
            foreach (var _t in depends)
            {
                var dependBundleId = ConvertDependNameToBundleId(_t);
                if (string.IsNullOrEmpty(dependBundleId))
                {
                    continue;
                }
                var objTemp = GetOrCreateBundle(dependBundleId);
                if (null == objTemp)
                {
                    Error("LoadAssetBundle():GetOrCreateBundle Failed!!! BundleId:{0}", dependBundleId);
                }
            }
            var bundle = GetOrCreateBundle(iBundleId);
            if (null != bundle) return bundle;
            Error("LoadAssetBundle():GetOrCreateBundle Failed!!! BundleId:{0}", iBundleId);
            return null;
        }

        /// <summary>
        /// 加载AssetBundle.
        /// </summary>
        /// <param name="iKey">Key.</param>
        /// <param name="iType">类型.</param>
        public T LoadFromAssetBundle<T>(
            string iKey, AssetBundleType iType = AssetBundleType.None) where T : Object
        {
            var _bundleId = GetBundleId(iKey, iType);
            if (string.IsNullOrEmpty(_bundleId))
            {
                return default(T);
            }
            // 预先加载依赖相关的bundle
            BundleConf _conf;
            if(null == BundleConfs || 
                false == BundleConfs.TryGetValue(_bundleId, out _conf)) {
                return default(T);
            }
            if (null == _conf)
            {
                return default(T);
            }
            var assetBundle = LoadAssetBundle(_bundleId, _conf.fileType);

            if (assetBundle == null)
            {
                Error("LoadFromAssetBundle()::Failed!!! BundleId:{0} Key:{1}", _bundleId, iKey);
                return default(T);
            }
            var objRet = assetBundle.LoadAsset<T>(iKey);
            if (objRet == null)
            {
                Error("LoadFromAssetBundle()::Failed!!! BundleId:{0} Key:{1}", _bundleId, iKey);
            }
            return objRet;
        }

        /// <summary>
        /// 加载AssetBundle.
        /// </summary>
        /// <param name="iKey">Key.</param>
        /// <param name="iRetType">返回值类型.</param>
        /// <param name="iAssetType">Asset类型.</param>
        public Object LoadFromAssetBundle(
            string iKey, Type iRetType, AssetBundleType iAssetType = AssetBundleType.None)
        {

            var _bundleId = GetBundleId(iKey, iAssetType);
            if (string.IsNullOrEmpty(_bundleId))
            {
                return null;
            }
            // 预先加载依赖相关的bundle
            BundleConf _conf;
            if(null == BundleConfs || 
                false == BundleConfs.TryGetValue(_bundleId, out _conf)) {
                return null;
            }
            if (null == _conf)
            {
                return null;
            }
            var assetBundle = LoadAssetBundle(_bundleId, _conf.fileType);

            if (assetBundle == null)
            {
                Error("LoadFromAssetBundle()::Failed!!! BundleId:{0} Key:{1}", _bundleId, iKey);
                return null;
            }
            var objRet = assetBundle.LoadAsset(iKey, iRetType);
            if (objRet == null)
            {
                Error("LoadFromAssetBundle()::Failed!!! BundleId:{0} Key:{1}", _bundleId, iKey);
            }
            return objRet;
        }

        /// <summary>
        /// 加载场景.
        /// </summary>
        /// <param name="iSceneName">场景名.</param>
        /// <returns>加载成功/失败标识位</returns>
        private bool LoadSceneAssetBundle(string iSceneName)
        {

            var _bundleId = GetBundleId(iSceneName, AssetBundleType.Scene);
            if (string.IsNullOrEmpty(_bundleId))
            {
                return false;
            }
            // 预先加载依赖相关的bundle
            BundleConf _conf;
            if(null == BundleConfs || 
                false == BundleConfs.TryGetValue(_bundleId, out _conf)) {
                return false;
            }
            if (null == _conf)
            {
                return false;
            }
            var bundle = GetOrCreateBundle(_bundleId);
            if (bundle != null) return true;
            Error("LoadSceneAssetBundle()::GetOrCreateBundle Failed!!! BundleId:{0}", _bundleId);
            return false;
        }

        /// <summary>
        /// 取得或创建Bundle
        /// 备注：
        /// 1)若已经加载过了，则不重复加载.
        /// 2)若无加载过，则创建
        /// </summary>
        /// <returns>Bundle.</returns>
        /// <param name="iBundleId">BundleId.</param>
        private UnityEngine.AssetBundle GetOrCreateBundle(string iBundleId)
        {
            var _bundle = GetBundleById(iBundleId);
            if (null != _bundle) return _bundle;
            _bundle = LoadBundleById(iBundleId);
            if (null != _bundle)
            {
                _bundles[iBundleId] = _bundle;
            }
            return _bundle;
        }

        /// <summary>
        /// 取得AssetBundle.
        /// </summary>
        /// <returns>AssetBundle.</returns>
        /// <param name="iBundleId">BundleId.</param>
        private UnityEngine.AssetBundle GetBundleById(string iBundleId)
        {
            UnityEngine.AssetBundle bundle;
            return _bundles.TryGetValue(iBundleId, out bundle) ? bundle : null;
        }

        /// <summary>
        /// 检测主Manifest文件.
        /// </summary>
        /// <returns><c>true</c>, OK, <c>false</c> NG.</returns>
        private bool CheckMainManifest()
        {

            if (_mainBundle != null)
            {
                _mainBundle.Unload(false);
            }

            var _mainManifestId = BundlesResult.AssetBundleDirNameOfNormal;
            _mainBundle = LoadMainManifestById(_mainManifestId);
            if (_mainBundle == null)
            {
                Error("CheckMainManifest():ManifestBundle load failed!!!(BundleId:{0})", _mainManifestId);
                return false;
            }
            _mainManifest = _mainBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
            if (_mainManifest != null) return true;
            Error("CheckMainManifest():ManifestBundle load failed!!!(BundleId:{0})", _mainManifestId);
            return false;

        }

        /// <summary>
        /// 加载MainManifest(同步加载).
        /// </summary>
        /// <param name="iBundleId">Bundle ID</param>
        /// <returns>加载MainManifest的AssetBundle.</returns>
        private UnityEngine.AssetBundle LoadMainManifestById(string iBundleId)
        {
            var path = GetBundleFullPath(iBundleId);
            if (File.Exists(path)) return UnityEngine.AssetBundle.LoadFromFile(path);
            Error("LoadMainManifestById():The assetbundle file is not exist!!!![AssetPath:{0}]", path);
            return null;
        }

        /// <summary>
        /// 加载Bundle(同步加载).
        /// </summary>
        /// <param name="iBundleId">Bundle ID</param>
        /// <returns>Bundle.</returns>
        private UnityEngine.AssetBundle LoadBundleById(string iBundleId)
        {
            var path = GetBundleFullPath(iBundleId);
            if (File.Exists(path)) return UnityEngine.AssetBundle.LoadFromFile(path);
            Error("LoadBundleById():The assetbundle file is not exist!!!![AssetPath:{0}]", path);
            return null;
        }

#endregion

        /// <summary>
        /// 将依赖名转换成BundleId.
        /// </summary>
        /// <returns>BundleId.</returns>
        /// <param name="iDependName">依赖名.</param>
        private static string ConvertDependNameToBundleId(string iDependName)
        {
            if (string.IsNullOrEmpty(iDependName))
            {
                return null;
            }
            var fileSuffix = BundlesResult.GetInstance().FileSuffix;
            fileSuffix = fileSuffix.ToLower();
            if (string.IsNullOrEmpty(fileSuffix))
            {
                return iDependName;
            }
            fileSuffix = $".{fileSuffix}";
            return iDependName.Replace(fileSuffix, "");
        }

        /// <summary>
        /// 释放已有Bundle.
        /// </summary>
        private void ReleaseAllBundle()
        {
            if (_bundles == null) return;
            foreach (var bundle in _bundles.Values)
            {
                bundle.Unload(false);
            }
            _bundles.Clear();
        }

        /// <summary>
        /// 释放已有Bundle.(add by zhan)
        /// </summary>
        /// <param name="iIsUnload">卸载标志位.</param>
        public void ReleaseAllBundle(bool iIsUnload)
        {
            if (_bundles == null) return;
            foreach (var bundle in _bundles.Values)
            {
                bundle.Unload(iIsUnload);
            }
            _bundles.Clear();
        }

        /// <summary>
        /// 取得BundleID.
        /// </summary>
        /// <returns>BundleID.</returns>
        /// <param name="iKey">Key.</param>
        /// <param name="iType">类型.</param>
        private string GetBundleId(string iKey, AssetBundleType iType = AssetBundleType.None)
        {
            string bundleId = null;
            string[] fileSuffixs = null;

            if (string.IsNullOrEmpty(iKey))
            {
                return null;
            }

            switch (iType)
            {
                case AssetBundleType.Scene:
                    {
                        if (ScenesMap == null)
                        {
                            break;
                        }
                        if (ScenesMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixScene };
                    }
                    break;

                case AssetBundleType.Prefab:
                    {
                        if (PrefabsMap == null)
                        {
                            break;
                        }
                        if (PrefabsMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixPrefab };
                    }
                    break;

                case AssetBundleType.Audio:
                    {
                        if (AudiosMap == null)
                        {
                            break;
                        }
                        if (AudiosMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixAudioWav, AssetFileHelper.FileSunffixAudioMp3 };
                    }
                    break;

                case AssetBundleType.Mat:
                    {
                        if (MatsMap == null)
                        {
                            break;
                        }
                        if (MatsMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixMat };
                    }
                    break;

                case AssetBundleType.SpriteTexture:
                    {
                        if (TexturesMap == null)
                        {
                            break;
                        }
                        if (TexturesMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixTexturePng,
                        AssetFileHelper.FileSunffixTextureJpg, AssetFileHelper.FileSunffixTextureTga };
                    }
                    break;

                case AssetBundleType.SpriteAtlas:
                    {
                        if (SpriteAtlasMap == null)
                        {
                            break;
                        }
                        if (SpriteAtlasMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixSpriteatlas };
                    }
                    break;

                case AssetBundleType.Asset:
                    {
                        if (AssetFilesMap == null)
                        {
                            break;
                        }
                        if (AssetFilesMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixAsset };
                    }
                    break;

                case AssetBundleType.Txt:
                    {
                        if (TextsMap == null)
                        {
                            break;
                        }
                        if (TextsMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixTxt };
                    }
                    break;


                case AssetBundleType.Json:
                    {
                        if (JsonFilesMap == null)
                        {
                            break;
                        }
                        if (JsonFilesMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                        fileSuffixs = new[] { AssetFileHelper.FileSunffixJson };
                    }
                    break;

                case AssetBundleType.None:
                    break;
                default:
                    {
                        if (BundlesMap == null)
                        {
                            break;
                        }

                        if (BundlesMap.TryGetValue(iKey, out bundleId) == false)
                        {
                        }
                    }
                    break;
            }

            if (AssetBundleType.None == iType || string.IsNullOrEmpty(bundleId) != true) return bundleId;
            if (fileSuffixs == null) return bundleId;
            foreach (var fileSuffix in fileSuffixs)
            {
                var isBreak = false;
                var fileName = $"{iKey}{fileSuffix}";
                foreach (var loop in BundlesMap)
                {
                    if (!loop.Key.EndsWith(fileName)) continue;
                    bundleId = loop.Value;
                    isBreak = true;
                    break;
                }

                if (isBreak)
                {
                    break;
                }
            }

            return bundleId;
        }

		/// <summary>
		/// 取得Bundle全路径名.
		/// </summary>
		/// <returns>Bundle全路径名.</returns>
		/// <param name="iBundleId">BundleId.</param>
		public string GetBundleFullPath(string iBundleId) {
            
            BundleConf conf;
            if(null == BundleConfs || 
                false == BundleConfs.TryGetValue(iBundleId, out conf)) {
                Error ("GetBundleFullPath()::This bundles is not exist!!!(BundleId:{0})", 
                    iBundleId);
                return null;
            }
			if (null == conf) {
				return null;
			}
			var fileName = BundlesResult.GetLocalBundleFileName(iBundleId, conf.fileType);
			if (string.IsNullOrEmpty (fileName)) {
				return null;
			}
				
			string fileFullPath;
			switch (conf.bundleType) {
			case BundleType.Normal:
				{
					fileFullPath = $"{SysSettings.BundlesDirOfNormal}/{fileName}";
				}
				break;
			case BundleType.Scene:
				{
					fileFullPath = $"{SysSettings.BundlesDirOfScenes}/{fileName}";
				}
				break;
			default:
				{
					fileFullPath = $"{SysSettings.BundlesDir}/{fileName}";
				}
				break;
			}
			return fileFullPath;
		}

    }
}
