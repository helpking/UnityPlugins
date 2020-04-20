//#define NFF_ASSET_BUNDLE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Packages.Common;
using Packages.Common.Base;
using Packages.Logs;

namespace Packages.AssetBundles
{

    /// <summary>
    /// 数据加载器.
    /// </summary>
    public sealed class DataLoader : ClassExtension
    {
        /// <summary>
        /// 释放所有已有的Bundles.
        /// </summary>
        public static void ReleaseAllBundles()
        {
            AssetBundlesManager.Instance.Dispose();
        }

        /// <summary>
        /// 加载场景.
        /// </summary>
        /// <param name="iSceneName">I scene name.</param>
        public static void LoadScene(string iSceneName)
        {
            if (AssetBundlesManager.Instance.LoadScene(iSceneName)) return;
            Loger.Warning($"DataLoader::LoadScene():There is no scene({iSceneName}) in asset bundles manager!!");

            // 加载场景
            SceneManager.LoadScene(iSceneName);
        }

        /// <summary>
        /// 加载函数(单个：同步).
        /// </summary>
        /// <returns>加载对象</returns>
        /// <param name="iPath">路径.</param>
        public static T Load<T>(string iPath)
            where T : UnityEngine.Object
        {
			if (string.IsNullOrEmpty(iPath))
            {
                return default(T);
            }

            Object objRet;
#if BUILD_DEBUG
            objRet = ResourcesLoad.Load<T>(iPath);
            if (null != objRet)
            {
                return (T) objRet;
			}
#endif

            var fileName = AssetBundlesManager.GetKeyOfMapByFilePath(iPath, typeof(T));
            if (string.IsNullOrEmpty(fileName))
            {
                Loger.Error($"DataLoader::Load(T):File not exist in bundles maps!!(path:{iPath})");
                return null;
			}

            objRet = AssetBundlesManager.Instance.LoadFromAssetBundle<T>(fileName);
            if (null != objRet) return (T) objRet;
            var path = ResourcesLoad.ToDefaultPath(iPath);
            objRet = ResourcesLoad.Load<T>(path);
            // if(null == objRet) {
            //     UtilsLog.Error("DataLoader", 
            //         "Load(T):Failed!!(path:{0})", iPath);
            // }
            return (T) objRet;
        }

        /// <summary>
        /// 加载函数(单个：同步).
        /// </summary>
        /// <returns>加载对象</returns>
        /// <param name="iPath">路径.</param>
		/// <param name="iRetType">返回值类型.</param>
        public static UnityEngine.Object Load(string iPath, System.Type iRetType)
        {
            if (string.IsNullOrEmpty(iPath))
            {
                return null;
            }
            Object objRet;
#if BUILD_DEBUG
            objRet = ResourcesLoad.Load(iPath, iRetType);
            if (null != objRet)
            {
                return objRet;
            }
#endif

            var fileName = AssetBundlesManager.GetKeyOfMapByFilePath(iPath, iRetType);
            if (string.IsNullOrEmpty(fileName))
            {
                Loger.Error($"DataLoader::Load():File not exist in bundles maps!!(path:{iPath})");
                return null;
            }

            objRet = AssetBundlesManager.Instance.LoadFromAssetBundle(fileName, iRetType);
            if (null != objRet) return objRet;
            var path = ResourcesLoad.ToDefaultPath(iPath);
            objRet = ResourcesLoad.Load(path, iRetType);
            // if(null == objRet) {
            //     UtilsLog.Error("DataLoader", 
            //         "Load():Failed!!(path:{0})", iPath);
            // }
            return objRet;
        }

        /// <summary>
        /// 加载函数(复数：同步).
        /// </summary>
        /// <param name="iPaths">路径列表</param>
        /// <param name="iLoadCompleted">加载完毕回调函数.</param>
        public static void Load(IEnumerable<string> iPaths,
            System.Action<bool, string, Object> iLoadCompleted)
        {
            foreach (var path in iPaths)
            {
                var obj = Load<Object>(path);
                // 加载失败
                if (null == obj)
                {
                    iLoadCompleted?.Invoke(false, path, null);
                    // 加载成功
                }
                else
                {
                    iLoadCompleted?.Invoke(true, path, obj);
                }
            }
        }

        /// <summary>
        /// 加载函数(单个：异步).
        /// </summary>
        /// <param name="iPath">路径.</param>
        /// <param name="iLoadCompleted">加载成功回调函数</param>
        public static IEnumerator LoadAsync<T>(string iPath,
            System.Action<bool, string, AssetBundleType, T> iLoadCompleted) where T : Object
        {
#if BUILD_DEBUG
            yield return ResourcesLoad.LoadAsync<T>(
                iPath, iLoadCompleted, RetryLoadAssetBundles<T>);
#else
            yield return AssetBundlesManager.Instance.LoadFromAssetBundleAsync<T>(
                iPath, iLoadCompleted, RetryLoadAssetBundles<T>);
#endif
            yield return new WaitForEndOfFrame();
            
        }

        /// <summary>
        /// 加载重试.
        /// </summary>
        /// <param name="iPath">路径.</param>
        /// <param name="iLoadCompleted">加载成功回调函数.</param>
        private static IEnumerator RetryLoadAssetBundles<T>(
            string iPath,
            System.Action<bool, string, AssetBundleType, T> iLoadCompleted) where T : Object
        {
#if BUILD_DEBUG
            // 切换路径 Default -> Develop
            string _assetPath = Const.SwitchPathToDevelop(iPath);
            yield return AssetBundlesManager.Instance.LoadFromAssetBundleAsync<T>(
                _assetPath, iLoadCompleted, null);
#else
            // 切换路径 Develop -> Default
            var path = Const.SwitchPathToDefault(iPath);
            yield return ResourcesLoad.LoadAsync<T>(path, iLoadCompleted, null);
            yield return new WaitForEndOfFrame();
#endif
        }

        /// <summary>
        /// 加载函数(复数：异步).
        /// </summary>
        /// <param name="iPaths">路径列表.</param>
        /// <param name="iLoadCompleted">加载成功回调函数</param>
        public static IEnumerator LoadAsync(IEnumerable<string> iPaths,
            System.Action<bool, string, AssetBundleType, Object> iLoadCompleted)
        {

            // 遍历所有资源
            foreach (var path in iPaths)
            {
                yield return LoadAsync<Object>(path, iLoadCompleted);
                yield return new WaitForEndOfFrame();
            }
            yield return null;

        }
    }

    /// <summary>
    /// 资源加载类.
    /// </summary>
    public static class ResourcesLoad
    {
        public const string AssetsPath = "Assets/Resources/";
        public const string ResourcesPath = "Assets/";
        public const string DefaultRootDir = "Default/";
        public const string DevelopRootDir = "Develop/";

        public static string CheckPath(string iPath)
        {
            var path = iPath;
            // Debug.Log(path);
            if (path.StartsWith(ResourcesPath))
            {
                path = path.Substring(AssetsPath.Length);
            }

            if (path.StartsWith(AssetsPath))
            {
                path = path.Substring(ResourcesPath.Length);
            }

            // 根据文件后缀名处理
            path = AssetFileHelper.RemoveFileSunffix(path);

            return path;
        }

        public static string ToDefaultPath(string iPath)
        {
            var path = CheckPath(iPath);
            if (path.StartsWith(DevelopRootDir))
            {
                path = path.Substring(DevelopRootDir.Length);
            }
            if(false == path.StartsWith(DefaultRootDir)) {
                path = DefaultRootDir + path;
            }
            return path;
        }

        /// <summary>
        /// 加载.
        /// </summary>
        /// <param name="iPath">路径.</param>
        /// <param name="iType">类型.</param>
        public static Object Load(string iPath, System.Type iType)
        {
            if (string.IsNullOrEmpty(iPath))
            {
                return null;
            }

            var path = CheckPath(iPath);
            return Resources.Load(path, iType);
        }

        /// <summary>
        /// 加载.
        /// </summary>
        /// <param name="iPath">路径.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Load<T>(string iPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(iPath))
            {
                return default(T);
            }

            var path = CheckPath(iPath);
            return Resources.Load<T>(path);
        }

        /// <summary>
        /// 加载（异步）.
        /// </summary>
        /// <param name="iPath">路径.</param>
        /// <param name="iLoadCompleted">加载完毕回调函数.</param>
        /// <param name="iRetryLoad">加载重试.</param>
        public static IEnumerator LoadAsync<T>(string iPath,
            System.Action<bool, string, AssetBundleType, T> iLoadCompleted, 
            System.Func<string, System.Action<bool, string, AssetBundleType, T>, IEnumerator> iRetryLoad) 
            where T : Object
        {
            var path = CheckPath(iPath);

            // 加载资源
            var res = Resources.LoadAsync<T>(path);
            yield return res;

            var bundleType = AssetFileHelper.GetAssetTypeByFilePath(iPath);
            var obj = res.asset as T;
            if (default(T) != obj)
            {
                iLoadCompleted?.Invoke(true, iPath, bundleType, obj);
            }
            else
            {
                if (null != iRetryLoad)
                {
                    yield return iRetryLoad(iPath, iLoadCompleted);
                } else {
                    iLoadCompleted(false, iPath, bundleType, obj);
                }
            }
        }
    }
}
