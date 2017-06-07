using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Common;

namespace AssetBundles
{

	/// <summary>
	/// Bundle hash.
	/// </summary>
	public class BundleHash
	{
		public class Item
		{
			public string bundleName = "";
			public string hashString = "";
			public string manifesthashString = "";
		}

		public List<BundleHash.Item> bundleList = new List<BundleHash.Item>();
		public int bundleCount = 0;

		public string GetHashByFileName(string bundleName)
		{
			for(int i = 0; i < bundleList.Count; i++)
			{
				if(bundleList[i].bundleName == bundleName)
				{
					return bundleList[i].hashString;
				}
			}

			return "";
		}
	}

	public static class AssetBundleConst
	{

		public readonly static string bundlesDir = Application.streamingAssetsPath + "/";
		public readonly static string bundleMapFilePath = "Assets/Resources/BundlesMap/BundlesMap.txt";
		public readonly static string hashFileName = "BundleHash.txt";

		public readonly static string bundleHashFile = Application.streamingAssetsPath + "/" + hashFileName;

		//local dir
		public readonly static string mainAssetBundleName = "StreamingAssets";
		public readonly static string downloadDir = Application.temporaryCachePath + "/Downloads/";
		public readonly static string bundleLocalBundlesDir = Application.temporaryCachePath + "/Bundles/";
		public readonly static string bundleUnzipTempDir = Application.temporaryCachePath + "/Temp/";

		/// <summary>
		/// 下载 Base URL.
		/// 路径格式：http://<服务器地址>/<工程名（例：NFF）>/<平台类型（如：iOS/Android等）>/<市场版本号>/<>
		/// </summary>
		public static string _downloadBaseUrl = "http://<ServerRoot>/NFF/<Platform>/<MarketVersion>/<ConfigType>/bundles/<BundleVersion>";
		public static string assetBundlesDirName = "/AssetBundles";

		public static string GetDownloadBasePath()
		{
			string path = string.Format("{0}/{1}", _downloadBaseUrl, BuildInfo.GetInstance().BuildVersionCode);
			return path;
		}

		public static string DownloadAssetBundlesUrl
		{
			get
			{
				return ProcessBaseURL(_downloadBaseUrl, CurrentUrlMode);
			}
		}

		public static string UploadLocalS3BuildBundleTagAddress
		{
			get
			{
				return ProcessBaseURL(_downloadBaseUrl, CurrentUploadMode);
			}
		}


		public enum UrlMode
		{
			Default,
			Local,
		}

		public static UrlMode CurrentUrlMode
		{
			set;get;
		}

		public static string MarketVersion
		{
			get
			{
				return "0.1.2";
			}
		}

		private static UrlMode _currentUploadMode = UrlMode.Local;
		public static UrlMode CurrentUploadMode
		{
			set
			{
				_currentUploadMode = value;
			}
			get
			{
				return _currentUploadMode;
			}
		}

		private static string _bundleVersion = "";
		public static string BundleVersion
		{
			set
			{
				_bundleVersion = value;
			}
			get
			{
				return _bundleVersion;
			}
		}

		private static string _publicServerRoot = "";
		public static string PublicServerRoot
		{
			set
			{
				_publicServerRoot = value;
			}
			get
			{
				return _publicServerRoot;
			}
		}
		
		private static string _localServerRoot = "10.5.11.52";
		public static string LocalServerRoot
		{
			set
			{
				_localServerRoot = value;
			}
			get
			{
				return _localServerRoot;
			}
		}

		public static string ConfigType
		{
			get
			{
				#if UNITY_EDITOR || DEVELOPMENT_BUILD
				return "stage";
				#else
				return "live";
				#endif
			}
		}

		public static string BuildTag
		{
			get 
			{
				return BuildInfo.GetInstance().BuildVersionCode;
			}
		}

		public static string Platform
		{
			get {
				string ret = "";
				switch(Application.platform)
				{
				case RuntimePlatform.Android:
					ret = "android";
					break;
				case RuntimePlatform.IPhonePlayer:
					ret = "ios";
					break;
				case RuntimePlatform.OSXPlayer:
					ret = "mac";
					break;
				case RuntimePlatform.WindowsPlayer:
					ret = "win";
					break;
					#if UNITY_EDITOR
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.WindowsEditor:
					#if UNITY_WEBPLAYER
					
					#elif UNITY_ANDROID
					ret = "android";
					#elif UNITY_IPHONE
					ret = "ios";
					#elif UNITY_STANDALONE_OSX
					ret = "mac";
					#elif UNITY_STANDALONE_WIN
					ret = "win";
					#endif
					break;
					#endif
					default:
						break;
				}
				return ret;
			}
		}

		public static string ProcessBaseURL(string baseURL, UrlMode currentMode)
		{
			string result = "";

			switch (currentMode)
			{
				case UrlMode.Local:
				{
					result = baseURL.Replace("<BundleVersion>", BundleVersion).Replace("<ServerRoot>", LocalServerRoot).Replace("<Platform>", Platform).Replace("<ConfigType>", ConfigType).Replace("<BuildTag>", BuildTag).Replace("<MarketVersion>", MarketVersion);
				}
				break;
				case UrlMode.Default:
				{
					result = baseURL.Replace("<BundleVersion>", BundleVersion).Replace("<ServerRoot>", PublicServerRoot).Replace("<Platform>", Platform).Replace("<ConfigType>", ConfigType).Replace("<BuildTag>", BuildTag).Replace("<MarketVersion>", MarketVersion);
				}
				break;
				default:
				{
					result = baseURL.Replace("<BundleVersion>", BundleVersion).Replace("<ServerRoot>", PublicServerRoot).Replace("<Platform>", Platform).Replace("<ConfigType>", ConfigType).Replace("<BuildTag>", BuildTag).Replace("<MarketVersion>", MarketVersion);
				}
				break;
			}

			Debug.LogWarningFormat("ProcessBaseURL result : {0}", result);

			return result;
		}




		public static void CompressFile(string sourcefilePath, string targetFilePath)
		{
//			byte[] fileByte = File.ReadAllBytes(sourcefilePath);
//			byte[] compressByte = LZ4.Compress(fileByte);
//			File.WriteAllBytes(targetFilePath, compressByte);
		}

		public static void DecompressFile(string sourcefilePath, string targetFilePath)
		{
//			AppLogger.Info("DecompressFile begin sourcefilePath " + sourcefilePath + " targetFilePath " + targetFilePath);
//			byte[] fileByte = File.ReadAllBytes(sourcefilePath);
//			AppLogger.Info("DecompressFile 1 " + fileByte.Length);
//			byte[] compressByte = LZ4.Decompress(fileByte);
//			AppLogger.Info("DecompressFile 2 " + compressByte.Length);
//			File.WriteAllBytes(targetFilePath, compressByte);
//			AppLogger.Info("DecompressFile end sourcefilePath " + sourcefilePath + " targetFilePath " + targetFilePath);
		}
			
	}
}
