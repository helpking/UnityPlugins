using System;
using UnityEngine;

namespace Packages.AssetBundles
{
    /// <summary>
    /// Asset文件Helper
    /// </summary>
    public static class AssetFileHelper
    {
        //文件后缀名
        public const string FileSunffixPrefab = ".prefab";
        public const string FileSunffixAudioWav = ".wav";
        public const string FileSunffixAudioMp3 = ".mp3";
        public const string FileSunffixMat = ".mat";
        public const string FileSunffixTexturePng = ".png";
        public const string FileSunffixTextureJpg = ".jpg";
        public const string FileSunffixTextureTga = ".tga";
        public const string FileSunffixSpriteatlas = ".spriteatlas";
        public const string FileSunffixAsset = ".asset";
        public const string FileSunffixTxt = ".txt";
        public const string FileSunffixJson = ".json";
        public const string FileSunffixScene = ".unity";

        /// <summary>
        /// 取得资源类型
        /// </summary>
        /// <param name="iFilePath">文件路径.</param>
        /// <returns>资源类型</returns>
        public static AssetBundleType GetAssetTypeByFilePath(string iFilePath)
        {
            // Scene文件
            if (iFilePath.EndsWith(FileSunffixScene))
            {
                return AssetBundleType.Scene;
            }
            
            // 预制体
            if (iFilePath.EndsWith(FileSunffixPrefab))
            {
                return AssetBundleType.Prefab;
            }
            
            // 音效
            if (iFilePath.EndsWith(FileSunffixAudioWav) ||
                iFilePath.EndsWith(FileSunffixAudioMp3))
            {
                return AssetBundleType.Audio;
            }
            
            // 材质
            if (iFilePath.EndsWith(FileSunffixMat))
            {
                return AssetBundleType.Mat;
            }
            
            // 纹理
            if (iFilePath.EndsWith(FileSunffixTexturePng) ||
                 iFilePath.EndsWith(FileSunffixTextureJpg) ||
                 iFilePath.EndsWith(FileSunffixTextureTga))
            {
                return AssetBundleType.SpriteTexture;
            }
            
            // 图集
            if (iFilePath.EndsWith(FileSunffixSpriteatlas))
            {
                return AssetBundleType.SpriteAtlas;
            }
            
            // (.asset)文件
            if (iFilePath.EndsWith(FileSunffixAsset))
            {
                return AssetBundleType.Asset;
            }
            
            // (.txt)文件
            if (iFilePath.EndsWith(FileSunffixTxt))
            {
                return AssetBundleType.Txt;
            }
            
            // (.json)文件
            return iFilePath.EndsWith(FileSunffixJson) ? AssetBundleType.Json : AssetBundleType.None;
        }

        /// <summary>
        /// 移除文件后缀
        /// </summary>
        /// <param name="iFilePath">文件路径</param>
        /// <returns>移除文件后缀的路径</returns>
        public static string RemoveFileSunffix(string iFilePath)
        {
            var _path = iFilePath;
            _path = _path.Replace(FileSunffixScene, "");
            _path = _path.Replace(FileSunffixPrefab, "");
            _path = _path.Replace(FileSunffixAudioWav, "");
            _path = _path.Replace(FileSunffixAudioMp3, "");
            _path = _path.Replace(FileSunffixMat, "");
            _path = _path.Replace(FileSunffixTexturePng, "");
            _path = _path.Replace(FileSunffixTextureJpg, "");
            _path = _path.Replace(FileSunffixTextureTga, "");
            _path = _path.Replace(FileSunffixSpriteatlas, "");
            _path = _path.Replace(FileSunffixAsset, "");
            _path = _path.Replace(FileSunffixTxt, "");
            _path = _path.Replace(FileSunffixJson, "");
            return _path;
        }

        /// <summary>
        /// 修复文件后缀
        /// </summary>
        /// <param name="iFilePath">文件路径</param>
        /// <param name="iAssetType">Asset类型</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>修复后的文件路径（带文件后缀）</returns>
        public static string FixedFileSunffix(string iFilePath, AssetBundleType iAssetType)
        {
            var lastIndex1 = iFilePath.LastIndexOf("/", StringComparison.Ordinal);
            var lastIndex2 = iFilePath.LastIndexOf(".", StringComparison.Ordinal);
            if (lastIndex1 < lastIndex2)
            {
                return iFilePath;
            }

            switch (iAssetType)
            {
                case AssetBundleType.Prefab:
                    iFilePath += FileSunffixPrefab;
                    break;
                case AssetBundleType.Audio:
                    iFilePath += FileSunffixAudioWav;
                    break;
                case AssetBundleType.Mat:
                    iFilePath += FileSunffixMat;
                    break;
                case AssetBundleType.SpriteTexture:
                    iFilePath += FileSunffixTexturePng;
                    break;
                case AssetBundleType.SpriteAtlas:
                    iFilePath += FileSunffixSpriteatlas;
                    break;
                case AssetBundleType.Asset:
                    iFilePath += FileSunffixAsset;
                    break;
                case AssetBundleType.Txt:
                    iFilePath += FileSunffixTxt;
                    break;
                case AssetBundleType.Json:
                    iFilePath += FileSunffixJson;
                    break;
                case AssetBundleType.Scene:
                    iFilePath += FileSunffixScene;
                    break;
                case AssetBundleType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("iAssetType", actualValue: iAssetType, message: null);
            }
            return iFilePath;
        }

        /// <summary>
        /// 修复文件后缀
        /// </summary>
        /// <param name="iFilePath">文件路径</param>
        /// <param name="iAssetType">Asset类型</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>修复后的文件路径（带文件后缀）</returns>
        public static string FixedFileSunffix(string iFilePath, Type iAssetType)
        {
            var lastIndex1 = iFilePath.LastIndexOf("/", StringComparison.Ordinal);
            var lastIndex2 = iFilePath.LastIndexOf(".", StringComparison.Ordinal);
            if (lastIndex1 < lastIndex2)
            {
                return iFilePath;
            }

            if (iAssetType == typeof(GameObject) || iAssetType.IsSubclassOf(typeof(Component)))
            {
                iFilePath += FileSunffixPrefab;
            }
            else if (iAssetType == typeof(AudioClip))
            {
                iFilePath += FileSunffixAudioWav;
            }
            else if (iAssetType == typeof(Material))
            {
                iFilePath += FileSunffixMat;
            }
            else if (typeof(Texture).IsAssignableFrom(iAssetType))
            {
                iFilePath += FileSunffixTexturePng;
            }
            else if (iAssetType == (typeof(UnityEngine.U2D.SpriteAtlas)))
            {
                iFilePath += FileSunffixSpriteatlas;
            }
            else if (iAssetType == typeof(TextAsset))
            {
                iFilePath += FileSunffixTxt;
            }
            return iFilePath;
        }
    }
}