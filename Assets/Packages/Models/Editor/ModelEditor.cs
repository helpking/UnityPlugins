using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Packages.Common.Base;
using Packages.Common.Counter;
using Packages.Common.Editor;
using Packages.Logs;
using Packages.Utils;
using UltimateGameTools.MeshSimplifier;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Packages.Models.Editor
{

    /// <summary>
    /// 拷贝类型
    /// </summary>
    internal enum AssetCopyType
    {
        /// <summary>
        /// 未知或未指定
        /// </summary>
        Unknow,
        /// <summary>
        /// 网格数据
        /// </summary>
        MeshData,
        /// <summary>
        /// 材质
        /// </summary>
        Material,
        /// <summary>
        /// 着色器
        /// </summary>
        Shader,
        /// <summary>
        /// 纹理
        /// </summary>
        Texture
    }
    /// <summary>
    /// Asset拷贝信息(Item)
    /// </summary>
    internal class AssetCopyItemInfo : JsonDataBase<AssetCopyItemInfo>
    {
        /// <summary>
        /// 目标路径
        /// </summary>
        public string Path;
        /// <summary>
        /// 目标文件Md5
        /// </summary>
        public string Md5;
    }
    
    /// <summary>
    /// 模型操作泪
    /// </summary>
    internal static class ModelEditor
    {

        internal const string _EXPORT_DATA_MESHES = "Meshes";
        internal const string _EXPORT_DATA_MATS = "Mats";
        internal const string _EXPORT_DATA_SHADERS = "Shaders";
        internal const string _EXPORT_DATA_TEXTURES = "Textures";
        
#region ProgressBar

        private static ProgressCounter _progressCounter = null;

        /// <summary>
        /// 更新进度条状态
        /// </summary>
        /// <param name="iStatus">状态</param>
        private static void UpdateProgressBarStatus(string iStatus)
        {
            if(null == _progressCounter) return;
            if(string.IsNullOrEmpty(iStatus)) return;
            _progressCounter.UpdateByStep(
                _progressCounter.CurStep, iStatus, 0);
        }
        
#endregion
        
#region Export

        /// <summary>
        /// 取得网格导出目录
        /// </summary>
        /// <param name="iExportRootDir">导出根目录</param>
        /// <returns>导出目录</returns>
        private static string GetExportMeshDir(string iExportRootDir)
        {
            // 检测导出目录
            var exportDir = $"{iExportRootDir}/{_EXPORT_DATA_MESHES}";
            // 校验数据输出目录
            if (UtilsTools.CheckAndCreateDirByFullDir(exportDir)) return exportDir;
            Loger.Error($"ModelEditor::GetExportMeshDir():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
            return null;
        }

        /// <summary>
        /// 取得材质导出目录
        /// </summary>
        /// <param name="iExportRootDir">导出根目录</param>
        /// <returns>导出目录</returns>
        private static string GetExportMatDir(string iExportRootDir)
        {
            // 检测导出目录
            var exportDir = $"{iExportRootDir}/{_EXPORT_DATA_MATS}";
            // 校验数据输出目录
            if (UtilsTools.CheckAndCreateDirByFullDir(exportDir)) return exportDir;
            Loger.Error($"ModelEditor::ExportMatDir():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
            return null;
        }
        
        /// <summary>
        /// 取得着色器导出目录
        /// </summary>
        /// <param name="iExportRootDir">导出根目录</param>
        /// <returns>导出目录</returns>
        private static string GetExportShaderDir(string iExportRootDir)
        {
            // 检测导出目录
            var exportDir = $"{iExportRootDir}/{_EXPORT_DATA_SHADERS}";
            // 校验数据输出目录
            if (UtilsTools.CheckAndCreateDirByFullDir(exportDir)) return exportDir;
            Loger.Error($"ModelEditor::GetExportShaderDir():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
            return null;
        }
        
        
        
        /// <summary>
        /// 取得纹理导出目录
        /// </summary>
        /// <param name="iExportRootDir">导出根目录</param>
        /// <returns>导出目录</returns>
        private static string GetExportTextureDir(string iExportRootDir)
        {
            // 检测导出目录
            var exportDir = $"{iExportRootDir}/{_EXPORT_DATA_TEXTURES}";
            // 校验数据输出目录
            if (UtilsTools.CheckAndCreateDirByFullDir(exportDir)) return exportDir;
            Loger.Error($"ModelEditor::GetExportTextureDir():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
            return null;
        }

#region Prefab

        /// <summary>
        /// 导出预制体
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iTarget">目标对象</param>
        /// <param name="iExportDir">导出路径</param>
        /// <param name="iPhysical">物理统计信息</param>
        /// <param name="iExportPrefabName">导出路径</param>
        /// <param name="iImporterSetting">导入信息设定</param>
        /// <param name="iAutoRelease">自动释放标志位</param>
        /// <returns>true:成功;false:失败;</returns>
        private static bool ExportPrefab(
            ProgressCounter iProgressCounter,
            GameObject iTarget, string iExportDir, ref PhysicalStatistics iPhysical,
            string iExportPrefabName = null, ModelMeshSettingsData iImporterSetting = null, 
            bool iAutoRelease = true)
        {

            if (null == iTarget) return false;
            // 校验数据输出目录
            var flg = UtilsTools.CheckAndCreateDirByFullDir(iExportDir);
            if (!flg)
            {
                Loger.Error($"ModelEditor::ExportPrefab():CheckAndCreateDirByFullDir Failed!(Dir={iExportDir})");
            }
            else
            {
                // 清空原有数据
                iPhysical.Clear();
                // 导出网格数据
                flg = ExportMeshesData(iProgressCounter, iExportDir, iTarget,
                    ref iPhysical.Tris, ref iPhysical.Verts);
                
                // 导出材质
                if (flg)
                {
                    flg = ExportMatsData(iProgressCounter, iExportDir, iTarget, 
                        ref iPhysical.Mats, ref iPhysical.Shaders, ref iPhysical.Textures,
                        iImporterSetting);
                }
            
                // 导出预制体路径
                if (!flg) return false;

                var prefabName = string.IsNullOrEmpty(iExportPrefabName) ? $"{iTarget.name}.prefab" : iExportPrefabName;
                var prefabPath = $"{iExportDir}/{prefabName}";
                if(File.Exists(prefabPath)) File.Delete(prefabPath);
                PrefabUtility.SaveAsPrefabAsset(iTarget, prefabPath, out flg);

                // 自动释放对象
                if (!iAutoRelease) return flg;
                Object.DestroyImmediate(iTarget);
                UtilsAsset.AssetsRefresh();

            }
            return flg;
        }

        /// <summary>
        /// 导出网格数据
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iExportDir">导出目录</param>
        /// <param name="iExportGo">导出用GameObject</param>
        /// <param name="iTris">网格三角形总数</param>
        /// <param name="iVerts">网格顶点总数</param>
        /// <returns>true:成功;false:失败;</returns>
        private static bool ExportMeshesData(
            ProgressCounter iProgressCounter,
            string iExportDir, GameObject iExportGo,
            ref int iTris, ref int iVerts)
        {
            if (null == iExportGo) return false;
            // 开始合并网格
            var mfs = iExportGo.GetComponentsInChildren<MeshFilter>();
            if (null == mfs || 0 >= mfs.Length) return false;
            
            // 检测导出目录
            var exportDir = GetExportMeshDir(iExportDir);
            // 校验数据输出目录
            if (string.IsNullOrEmpty(exportDir))
            {
                Loger.Error($"ModelEditor::ExportMeshData():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
                return false; 
            }
            
            // 更新计数器Step计数信息
            iProgressCounter.UpdateStepInfo((int) CombineMeshStep.ExportMeshData, mfs.Length);
            iTris = 0;
            iVerts = 0;
            foreach (var mf in mfs)
            {
                if (false == mf.gameObject.activeInHierarchy)
                {
                    Loger.Warning($"ModelEditor::ExportMeshData():This gameobject(name:{mf.gameObject.name}) is invalid!(activeInHierarchy={mf.gameObject.activeInHierarchy})");
                    // 更新进度信息(导出网格数据)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportMeshData, $"网格导出(Skip):{mf.gameObject.name}");
                    continue;
                }
                
                var sharedMesh = mf.sharedMesh;
                iTris += sharedMesh.triangles.Length / 3;
                iVerts += sharedMesh.vertexCount;
                
                // 网格数据拷贝
                var meshDataFilePath = $"{exportDir}/{mf.gameObject.name}.asset";
                AssetDatabase.CreateAsset(sharedMesh, meshDataFilePath);
                
                // 重新设定新的网格数据对象
                var newMesh = AssetDatabase.LoadAssetAtPath(meshDataFilePath, typeof(Mesh)) as Mesh;
                if (null == newMesh)
                {
                    Loger.Error($"ModelEditor::ExportMeshData():LoadAssetAtPath new mesh data failed!(name:{mf.gameObject.name}) is invalid!(path={meshDataFilePath})");
                    // 更新进度信息(导出网格数据)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportMeshData, $"网格导出(Skip):{mf.gameObject.name}");
                    continue;
                }
                mf.sharedMesh = newMesh;
                
                // 更新进度信息(导出网格数据)
                iProgressCounter.UpdateByStep(
                    (int) CombineMeshStep.ExportMeshData, $"网格导出:{mf.gameObject.name}");
            }
            
            return true;
        }

        /// <summary>
        /// 导出网格数据
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iExportDir">导出目录</param>
        /// <param name="iExportGo">导出用GameObject</param>
        /// <param name="iMats">导出材质数</param>
        /// <param name="iShaders">导出着色器数</param>
        /// <param name="iTextures">导出纹理数</param>
        /// <param name="iImporterSetting">平台导入信息设定</param>
        /// <returns>true:成功;false:失败;</returns>
        private static bool ExportMatsData(
            ProgressCounter iProgressCounter, 
            string iExportDir, GameObject iExportGo, 
            ref int iMats, ref int iShaders, ref int iTextures, 
            ModelMeshSettingsData iImporterSetting = null)
        {
            if (null == iExportGo) return false;
            // 开始合并网格
            var mrs = iExportGo.GetComponentsInChildren<MeshRenderer>();
            if (null == mrs || 0 >= mrs.Length) return false;
            
            // 检测导出目录
            var exportDir = GetExportMatDir(iExportDir);
            // 校验数据输出目录
            if (string.IsNullOrEmpty(exportDir))
            {
                Loger.Error($"ModelEditor::ExportMatData():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
                return false; 
            }
            
            // 更新计数器Step计数信息
            iProgressCounter.UpdateStepInfo((int) CombineMeshStep.ExportMatData, mrs.Length);
            
            foreach (var mr in mrs)
            {
                // 材质拷贝
                var oldMatFilePath = AssetDatabase.GetAssetPath(mr.sharedMaterial);
                var matFileName = UtilsTools.GetFileName(oldMatFilePath);
                var newMatFilePath = $"{exportDir}/{matFileName}";
                if (!IsCopyNecessary(
                    AssetCopyType.Material, oldMatFilePath, ref newMatFilePath))
                {
                    // 更新进度信息(导出网格数据)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportMatData, $"材质导出(Skip):{mr.gameObject.name}");
                    continue;
                }
                
                // 材质数累加
                iMats += 1;
                if (!AssetDatabase.CopyAsset(oldMatFilePath, newMatFilePath))
                {
                    Loger.Error($"ModelEditor::ExportMatData():The new mat copy failed!!!(old={oldMatFilePath} new={newMatFilePath})");
                    // 更新进度信息(导出网格数据)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportMatData, $"材质导出(Skip):{mr.gameObject.name}");
                    continue;
                }

                // 重新设定新的网格数据对象
                var newMat = AssetDatabase.LoadAssetAtPath(newMatFilePath, typeof(Material)) as Material;
                if (null == newMat)
                {
                    Loger.Error($"ModelEditor::ExportMeshData():LoadAssetAtPath new mat data failed!(name:{mr.gameObject.name}) is invalid!(path={newMatFilePath})");
                    // 更新进度信息(导出网格数据)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportMatData, $"材质导出(Skip):{mr.gameObject.name}");
                    continue;
                }
                mr.sharedMaterial = newMat;
                
                // 导出材质相关纹理
                if (!ExportTexturesDataByMat(iProgressCounter, iExportDir, newMat, 
                    ref iTextures, iImporterSetting))
                {
                    Loger.Error($"ModelEditor::ExportMeshData():ExportTexturesDataByMat failed!(name:{mr.gameObject.name})");
                }
                
                // 导出材质着色器
                if (!ExportShadersDataByMat(iProgressCounter, iExportDir, newMat, ref iShaders))
                {
                    Loger.Error($"ModelEditor::ExportMeshData():ExportTexturesDataByMat failed!(name:{mr.gameObject.name})");
                }
                
                // 更新进度信息(导出网格数据)
                iProgressCounter.UpdateByStep(
                    (int) CombineMeshStep.ExportMatData, $"材质导出:{mr.gameObject.name}");
            }
            
            return true;
        }

        /// <summary>
        /// 根据材质导出纹理文件
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iExportDir">导出目录</param>
        /// <param name="iMat">材质</param>
        /// <param name="iTextures">导出纹理数</param>
        /// <param name="iImporterSetting">平台导入信息设定</param>
        /// <returns>true:成功;false:失败;</returns>
        private static bool ExportTexturesDataByMat(
            ProgressCounter iProgressCounter, string iExportDir, 
            Material iMat, ref int iTextures, 
            ModelMeshSettingsData iImporterSetting = null)
        {
            
            if (null == iMat) return false;
            // 拷贝用纹理信息收集
            var names= iMat.GetTexturePropertyNames();
            var ids = iMat.GetTexturePropertyNameIDs();
            var texMaxCount = 0;
            for (var idx = 0; idx < ids.Length; ++idx)
            {
                var texId = ids[idx];
                var tex = iMat.GetTexture(texId);
                if (null == tex) continue;
                ++texMaxCount;
            }

            if (0 >= texMaxCount) return true;
            // 检测导出目录
            var exportDir = GetExportTextureDir(iExportDir); 
            // 校验数据输出目录
            if (string.IsNullOrEmpty(exportDir))
            {
                Loger.Error($"ModelEditor::ExportTexturesDataByMat():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
                return false; 
            }

            // 更新计数统计设定(导出纹理数据)
            iProgressCounter.UpdateStepInfo((int) CombineMeshStep.ExportTexture, texMaxCount);
            
            for (var idx = 0; idx < ids.Length; ++idx)
            {
                var texId = ids[idx];
                var texName = names[idx];
                var tex = iMat.GetTexture(texId);
                if(null == tex) continue;
                var oldTexFilePath = AssetDatabase.GetAssetPath(tex);
                var texFileName = UtilsTools.GetFileName(oldTexFilePath);
                var newTexFilePath = $"{exportDir}/{texFileName}";

                // 校验是否有必要拷贝
                if (!IsCopyNecessary(
                    AssetCopyType.Texture, oldTexFilePath, ref newTexFilePath))
                {
                    // 更新进度信息(导出纹理数据 - 失败 - 跳过)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportTexture, $"纹理导出(Skip):{texFileName}");
                    continue;
                }

                // 纹理数累加
                iTextures += 1;
                if(!AssetDatabase.CopyAsset(oldTexFilePath, newTexFilePath))
                {
                    Loger.Error($"ModelEditor::ExportTexturesDataByMat():The new texture copy failed!!!(old={oldTexFilePath} new={newTexFilePath})");
                    // 更新进度信息(导出纹理数据 - 失败 - 跳过)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportTexture, $"纹理导出(Skip):{texFileName}");
                    continue;
                }
                    
                var newTex = AssetDatabase.LoadAssetAtPath(newTexFilePath, typeof(Texture)) as Texture;
                
                // 纹理导入设定
                if (null == newTex)
                {
                    // 更新进度信息(导出纹理数据 - 失败 - 跳过)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportTexture, $"纹理导出(Skip):{texFileName}");
                    continue;
                }

                // 导入纹理平台设定
                if (null != iImporterSetting)
                {
                    // 更新进度计数 - 网格数据备份
                    iProgressCounter.AddStepInfo((int) CombineMeshStep.ApplyPlatformSettings, 
                        1, true);
                    
                    if(!ApplyTexturePlatformSettings(iImporterSetting, newTexFilePath))
                    {
                        Loger.Error($"ModelEditor::ExportTexturesDataByMat():ApplyTexturePlatformSettings failed!!!" +
                                    $"(path={newTexFilePath})");
                    }
                    
                    // 更新进度信息(导出纹理数据)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ApplyPlatformSettings, $"平台设定:{newTex.name}");
                }

                iMat.SetTexture(texName, newTex);
                // 更新进度信息(导出纹理数据)
                iProgressCounter.UpdateByStep(
                    (int) CombineMeshStep.ExportTexture, $"纹理导出:{texFileName}");
            }
            
            return true;
        }

        /// <summary>
        /// 根据材质导出着色器文件
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iExportDir">导出目录</param>
        /// <param name="iMat">材质</param>
        /// <param name="iShaders">导出着色器数</param>
        /// <returns>true:成功;false:失败;</returns>
        private static bool ExportShadersDataByMat(
            ProgressCounter iProgressCounter, string iExportDir, Material iMat, ref int iShaders)
        {
            if (null == iMat) return false;
            
            var oldShaderFilePath = AssetDatabase.GetAssetPath(iMat.shader);
            if (!File.Exists(oldShaderFilePath))
            {
                if (oldShaderFilePath.StartsWith("Assets/"))
                {
                    Loger.Error($"ModelEditor::ExportShadersDataByMat():shader file missing!!!(path={oldShaderFilePath})");
                }
            }
            else
            {
                // 检测导出目录
                var exportDir = GetExportShaderDir(iExportDir); 
                // 校验数据输出目录
                if (string.IsNullOrEmpty(exportDir))
                {
                    Loger.Error($"ModelEditor::ExportShadersDataByMat():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
                    return false; 
                }
                var shaderShortName = UtilsTools.GetFileName(oldShaderFilePath);
                var newShaderFilePath = $"{exportDir}/{shaderShortName}";

                if (IsCopyNecessary(
                    AssetCopyType.Shader, oldShaderFilePath, ref newShaderFilePath))
                {
                    // 更新进度信息(导出纹理数据)
                    iProgressCounter.UpdateStepInfo(
                        (int) CombineMeshStep.ExportShader, 1);
                    
                    // 材质数累加
                    iShaders += 1;
                    if (!AssetDatabase.CopyAsset(oldShaderFilePath, newShaderFilePath))
                    {
                        Loger.Error($"ModelEditor::ExportShadersDataByMat():The new shader copy failed!!!(old={oldShaderFilePath} new={newShaderFilePath})");
                    }
                    else
                    {
                        var newShader = AssetDatabase.LoadAssetAtPath(newShaderFilePath, typeof(Shader)) as Shader;
                        iMat.shader = newShader;   
                    }
                    
                    // 更新进度信息(导出纹理数据)
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.ExportShader, $"着色器导出:{shaderShortName}");
                }
            }
            
            return true;
        }

#endregion
        
#endregion


#region AssetsCopyCheck

        /// <summary>
        /// 拷贝历史记录
        /// </summary>
        private static Dictionary<AssetCopyType, List<AssetCopyItemInfo>> CopyHistory = 
            new Dictionary<AssetCopyType, List<AssetCopyItemInfo>>();

        /// <summary>
        /// 清空拷贝历史记录
        /// </summary>
        /// <param name="iType">拷贝类型</param>
        private static void ClearAssetCopyHistory(AssetCopyType iType = AssetCopyType.Unknow)
        {
            if (AssetCopyType.Unknow == iType)
            {
                CopyHistory.Clear();
            }
            else
            {
                if (CopyHistory.TryGetValue(iType, out var list))
                {
                    list.Clear();
                }
            }
        }
        
        /// <summary>
        /// 判断拷贝是否需要
        /// </summary>
        /// <param name="iType">类型</param>
        /// <param name="iSourcePath">源路径</param>
        /// <param name="iDestPath">目标路径</param>
        /// <param name="iDeepCheck">深度检测标志位(true:校验文件的Md5是否一致。false:简单校验路径)</param>
        /// <returns>true:需要拷贝; false:不需要拷贝;</returns>
        private static bool IsCopyNecessary(
            AssetCopyType iType, string iSourcePath, 
            ref string iDestPath, bool iDeepCheck = false)
        {
            var flg = false;
            var destPath = iDestPath;
            if (string.IsNullOrEmpty(iSourcePath) || string.IsNullOrEmpty(iDestPath)) return flg;
            
            if (CopyHistory.TryGetValue(iType, out var copies))
            {
                // 判断存在与否，若不存在则追加，并返回true，需要拷贝Asset。反之，则不追加，不拷贝
                flg = !copies.Exists(iO =>
                    destPath.Equals(iO.Path));
                // 若不存在，在追加
                if (flg)
                {
                    var sourceMd5 = UtilsTools.GetMd5ByFilePath(iSourcePath);
                    copies.Add(new AssetCopyItemInfo
                    {
                        Path = iDestPath,
                        Md5 = sourceMd5
                    });
                }
                else
                {
                    // 若已经拷贝过，则且未深度检测时，则比较当前源文件和已拷贝文件的Md5码是否一致
                    if (iDeepCheck)
                    {
                        var copyed = copies
                            .Where(iO => destPath.Equals(iO.Path))
                            .ToArray()[0];
                        var sourceMd5 = UtilsTools.GetMd5ByFilePath(iSourcePath);
                        flg = !sourceMd5.Equals(copyed.Md5);
                        // 若需要拷贝，为了避免文件重名，则需要重命名目标文件。
                        if (flg)
                        {
                            var idx = 0;
                            // 取得文件名
                            var fileDir = UtilsTools.GetFileDirByFilePath(destPath);
                            var fileName = UtilsTools.GetFileName(destPath, false);
                            var fileExtension = UtilsTools.GetFileExtension(destPath);
                            var filePath = $"{fileDir}/{fileName}_{idx}{fileExtension}";
                            while (File.Exists(filePath))
                            { 
                                ++idx;
                                filePath = $"{fileDir}/{fileName}_{idx}{fileExtension}"; 
                            }
                            iDestPath = filePath;

                        }
                    }
                }
            }
            else
            {
                // 追加
                var sourceMd5 = UtilsTools.GetMd5ByFilePath(iSourcePath);
                CopyHistory.Add(iType, new List<AssetCopyItemInfo>
                {
                   new AssetCopyItemInfo
                   {
                       Path = iDestPath,
                       Md5 = sourceMd5
                   } 
                });
                flg = true;
            }
            return flg;
        }


#endregion
        /// <summary>
        /// 开始网格合并以及降面(批量)
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iTargets">目标信息列表</param>
        /// <param name="iOutputDir">输出目录</param>
        /// <param name="iOnFailed">失败处理</param>
        /// <returns>true:成功; false:失败;</returns>
        internal static bool StartCombineAndSimplify(
            ProgressCounter iProgressCounter, ModelInfo[] iTargets,
            string iOutputDir, Action<string, string> iOnFailed = null)
        {
            var flg = true;
            // // 遍历循环执行
            // foreach (var target in iTargets)
            // {
            //     if(!target.Checked) continue;
            //     var otherInfo = StartCombineAndSimplify(
            //         iProgressCounter, false, target, iOutputDir, iOnFailed);
            //     flg = null != otherInfo;
            //     if(!flg) break;
            // }
            //
            // // 执行成功，则清除进度条
            // if (flg)
            // {
            //     iProgressCounter.EndCounter();
            // }
            return flg;
        }
        
        /// <summary>
        /// 合并网格到默认
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iTarget">目标信息</param>
        /// <param name="iImporterSetting">导入设定信息</param>
        /// <param name="iExportDir">导出路径</param>
        /// <param name="iOnFailed">失败处理</param>
        /// <returns>转化后的Default GameObject</returns>
        internal static GameObject StartCombineToDefault(
            ProgressCounter iProgressCounter, ModelInfo iTarget, 
            ModelMeshSettingsData iImporterSetting, string iExportDir, 
            Action<string, string> iOnFailed = null)
        {
            // 记录开始时间
            var startDateTime = DateTime.Now.Ticks;
            
            // 清空拷贝历史记录
            ClearAssetCopyHistory();
            
            // 开始合并原始资源到默认网格
            var defaultObject = StartCombineMeshToDefault(
                iProgressCounter, iTarget, iImporterSetting, iExportDir);
            if(null == defaultObject)
            {
                iOnFailed?.Invoke("错误", 
                    $"模型网格合并失败(default)!\nPath:{iTarget.Path} -> {iExportDir}");
                return null;
            }
            
            // 清空拷贝历史记录
            ClearAssetCopyHistory();

            // 计算消耗时间
            iTarget.CombineMeshCostTime = DateTime.Now.Ticks - startDateTime;
            iTarget.CombineMeshCostTime = 0 >= iTarget.CombineMeshCostTime ? 0 : iTarget.CombineMeshCostTime;
            
            return defaultObject;
        }
        
        /// <summary>
        /// 开始网格合并以及降面(单个)
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iDefaultGo">Default GameObject</param>
        /// <param name="iImporterSettings">导入设定信息</param>
        /// <param name="iTarget">目标信息</param>
        /// <param name="iExportDir">导出目录</param>
        /// <param name="iOnFailed">失败处理</param>
        /// <returns>true:成功; false:失败;</returns>
        internal static bool StartSimplifyToOther(
            ProgressCounter iProgressCounter, GameObject iDefaultGo,
            ModelMeshSettingsData iImporterSettings, ModelInfo iTarget, 
            string iExportDir, Action<string, string> iOnFailed = null)
        {
            // 设定降面信息
            var tabIndex = iImporterSettings.TabIndex;
            var simplify = iTarget.GetSimplifyInfoByTabIndex(tabIndex);
            if (null == simplify)
            {
                iOnFailed?.Invoke("错误", 
                    $"降面信息不存在!\n模型信息:{iTarget}");
                return false;
            }
            
            // 取得预制体路径
            var flg = true;
            if(!iProgressCounter.IsCounting) iProgressCounter.StartCounter($"模型降面开始:{iDefaultGo.name}");
            
            // 取得导出目录
            var exportDir = iTarget.GetExportPath(iExportDir, tabIndex);
            // 记录开始时间
            var startDateTime = DateTime.Now.Ticks;
                
            // PC以外的场合，合并网格为一个网格且合并材质与纹理
            var otherPrefabGo = iDefaultGo;
            if (!isPCTab(tabIndex))
            {
                // 平台纹理设定
                if (!ApplyTexturePlatformSettingsByGameObject(
                    iProgressCounter, otherPrefabGo, iImporterSettings))
                {
                    iOnFailed?.Invoke("错误", 
                        $"平台纹理设定失败!\nPrefab:{iDefaultGo.name}\n Tab:{Tabs[tabIndex]}");
                    return false;
                }
                
                otherPrefabGo = CombineMatsAndTextures(
                    iProgressCounter, iDefaultGo, simplify, exportDir,
                    iOnFailed);
                if (null == otherPrefabGo)
                {
                    return false;
                }
            }
                
            // 清空拷贝历史记录
            ClearAssetCopyHistory();
            // 开始降面
            if (!StartModelSimplify(
                iProgressCounter, otherPrefabGo, exportDir, 
                simplify, iImporterSettings, iOnFailed))
            {
                iOnFailed?.Invoke("错误", 
                    $"模型网格降面失败!\nPath:{iTarget.Path} -> {exportDir}/{UtilsTools.GetFileName(iTarget.Path)}");
                flg = false;
            }
            // 清空拷贝历史记录
            ClearAssetCopyHistory();

            // 计算消耗时间
            simplify.CostTime = DateTime.Now.Ticks - startDateTime;
            simplify.CostTime = 0 >= simplify.CostTime ? 0 : simplify.CostTime;
            return flg;
        }

#region Combine

        /// <summary>
        /// 加载预制体并实例化
        /// </summary>
        /// <param name="iPath">路径</param>
        /// <returns>实例化对象</returns>
        internal static GameObject LoadModelPrefab(string iPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(iPath);
            if (string.IsNullOrEmpty(guid))
            {
                Loger.Error($"ModelEditor::LoadModelPrefab():Invalid Path:{iPath}");
                return null;
            }
            // 加载预制造体
            var prefab = AssetDatabase.LoadAssetAtPath(iPath, typeof(GameObject)) as GameObject;
            if (null == prefab)
            {
                Loger.Error($"ModelEditor::LoadModelPrefab():AssetDatabase.LoadAssetAtPath Failed!(Path={iPath})");
                return null;
            }
            // 实例化
            var instantiatePrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (null != instantiatePrefab) return instantiatePrefab;
            Loger.Error($"ModelEditor::LoadModelPrefab():PrefabUtility.InstantiatePrefab Failed!(Path={iPath})");
            return null;
        }
        
        /// <summary>
        /// 开始合并原始模型网格到Default
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iTarget">目标信息</param>
        /// <param name="iImporterSetting">导入设定信息</param>
        /// <param name="iOutputDir">输出目录</param>
        /// <returns>true:OK; false:NG;</returns>
        private static GameObject StartCombineMeshToDefault(
            ProgressCounter iProgressCounter, 
            ModelInfo iTarget, ModelMeshSettingsData iImporterSetting, 
            string iOutputDir)
        {
            
            Loger.Info($"ModelEditor::CombineMesh():{iTarget.ToString()}");
            // 加载预制体并实例化
            var instantiate = LoadModelPrefab(iTarget.Path);
            if (null == instantiate)
            {
                Loger.Error($"ModelEditor::CombineMesh():LoadModelPrefab Failed!(Dir={iTarget.Path})");
                return null;
            }
            // 检测输出路径
            var modelOutputDir = iOutputDir;
            if (!UtilsTools.CheckAndCreateDirByFullDir(modelOutputDir))
            {
                Loger.Error($"ModelEditor::CombineMesh():CheckAndCreateDirByFullDir Failed!(Dir={modelOutputDir})");
                return null;
            }
            
            var guid = AssetDatabase.AssetPathToGUID(iTarget.Path);
            if (string.IsNullOrEmpty(guid))
            {
                Loger.Error($"ModelEditor::CombineMesh():AssetDatabase.AssetPathToGUID Failed!:{iTarget.Path}");
                return null;
            }
            
            // 开始网格合并
            var defaultPrefabGo = CombineMeshBySameMat(
                iProgressCounter, instantiate);
            // 释放
            Object.DestroyImmediate(instantiate);
            if (null == defaultPrefabGo) return null;
            // 保存成预制体
            var prefabName = UtilsTools.GetFileName(iTarget.Path);
            if (!ExportPrefab(
                iProgressCounter, defaultPrefabGo, modelOutputDir, 
                ref iTarget.Physical, prefabName, iImporterSetting, false))
            {
                Loger.Error($"ModelEditor::CombineMesh():ExportPrefab Failed!:{iTarget.Path}");
            }
            // 刷新目录
            UtilsAsset.AssetsRefresh();
            return defaultPrefabGo;
        }

        /// <summary>
        /// 对目标GameObject中的纹理，应用平台设定
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iTarget">目标GameObject</param>
        /// <param name="iImporterSetting">导入设定</param>
        /// <returns>true:OK; false:NG;</returns>
        private static bool ApplyTexturePlatformSettingsByGameObject(
            ProgressCounter iProgressCounter,  
            GameObject iTarget, ModelMeshSettingsData iImporterSetting)
        {

            if (null == iTarget)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettingsByGameObject():The target gameobject is invalid or null!!");
                return false;
            }
            if (null == iImporterSetting)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettingsByGameObject():The importer setting is invalid or null!!");
                return false;
            }

            var texs = new List<Texture>();
            var paths = new List<string>();
            var mrs = iTarget.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in mrs)
            {
                var mats = mr.sharedMaterials;
                foreach (var mat in mats)
                {
                    if(null == mat) continue;
                    var shader = mat.shader;
                    if(null == shader) continue;
                    var properties = GetTexturePropertiesFromShader(shader);
                    foreach (var property in properties)
                    {
                        var tex = mat.GetTexture(property);
                        if(null == tex) continue;

                        var path = AssetDatabase.GetAssetPath(tex);
                        if(string.IsNullOrEmpty(path)) continue;
                        if(paths.Exists(iO => path.Equals(iO))) continue;
                        paths.Add(path);
                        texs.Add(tex);
                    }
                }
            }
            paths.Clear();

            if (0 >= texs.Count) return true;
            
            // 更新进度计数 - 网格数据备份
            iProgressCounter.AddStepInfo((int) CombineMeshStep.ApplyPlatformSettings, 
                texs.Count, true);
            foreach (var tex in texs)
            {
                if (!ApplyTexturePlatformSettings(iImporterSetting, tex))
                {
                    Loger.Error($"ModelEditor::ApplyTexturePlatformSettingsByGameObject():ApplyTexturePlatformSettings Failed!!" +
                                $"\n(Tex:{tex.name} \nSettings:{iImporterSetting.ToString()})");
                }
                
                // 更新进度信息(网格合并)
                iProgressCounter.UpdateByStep(
                    (int) CombineMeshStep.ApplyPlatformSettings, $"导入设定:{tex.name}({Tabs[iImporterSetting.TabIndex]})");
            }
            texs.Clear();
            
            return true;
        }

        /// <summary>
        /// 生成网格合并用的配置信息
        /// </summary>
        /// <param name="iTarget"></param>
        /// <returns></returns>
        private static CombineConfig GenerateCombineConfig(GameObject iTarget)
        {
            var config = new CombineConfig();
            // 取得原来对象所有网格
            var meshFilters = iTarget.GetComponentsInChildren<MeshFilter> ();
            // var centerPos = GetParentCenterFromChildComponents(meshFilters);
            var matrix = iTarget.transform.worldToLocalMatrix;
            for (var i = 0; i < meshFilters.Length; i++)
            {
                var mf = meshFilters[i];
                if (null == mf)
                {
                    Loger.Warning($"ModelEditor::GenerateCombineConfig():The MeshFilter is invalid or missing in this node {mf.gameObject.name}!!");
                    continue;
                }
                // 未被激活的排除
                if(!mf.gameObject.activeInHierarchy) continue;
                var mr = mf.GetComponent<MeshRenderer>();
                if (mr == null) continue;

                var sharedMesh = mf.sharedMesh;
                if (null == sharedMesh)
                {
                    Loger.Warning($"ModelEditor::GenerateCombineConfig():The mesh in MeshFilter is invalid or missing in this node {mf.gameObject.name}!!");
                    continue;
                }

                // var matrixTmp = mf.transform.localToWorldMatrix;
                // matrixTmp.m03 -= centerPos.x;
                // matrixTmp.m13 -= centerPos.y;
                // matrixTmp.m23 -= centerPos.z;
                // matrixTmp = matrix * matrixTmp;
                var combine = new CombineInstance()
                {
                    mesh = Object.Instantiate(mf.sharedMesh), // 克隆新网格
                    transform = mf.transform.localToWorldMatrix,
                    lightmapScaleOffset = mr.lightmapScaleOffset
                    // transform = mf.transform.localToWorldMatrix
                };

                mr.enabled = false;
                if (null == mr.sharedMaterial)
                {
                    Loger.Warning($"ModelEditor::GenerateCombineConfig():The Material is invalid or missing in this node {mr.gameObject.name}!!");
                    continue;
                }
                if (1 < mr.sharedMaterials.Length)
                {
                    Loger.Error($"ModelEditor::GenerateCombineConfig():The Material is multiple (node name :{mr.gameObject.name})!!");
                }
                var mat = mr.sharedMaterial;
                if (!config.Items.ContainsKey(mat.name))
                {
                    config.Items.Add(mat.name, new CombineItem
                    {
                        InstanceID = mr.gameObject.GetInstanceID(), 
                        Mat = mat,
                        CombineInstances = new List<CombineInstance>() { combine }
                    });
                }
                else
                {
                    config.Items[mat.name].CombineInstances.Add(combine);
                }
                
            }
            return config;
        }
        
        /// <summary>
        /// 取得父节点中心点坐标
        /// </summary>
        /// <param name="iChildComponents">子节点组建数组</param>
        /// <returns></returns>
        private static Vector3 GetParentCenterFromChildComponents(Component[] iChildComponents)
        {
            if (iChildComponents == null || iChildComponents.Length <= 0) return Vector3.zero;
            var min = iChildComponents[0].transform.position;
            var max = min;
            foreach (var comp in iChildComponents)
            {
                var position = comp.transform.position;
                min = Vector3.Min(min, position);
                max = Vector3.Max(max, position);
            }
            return min + ((max - min) / 2);
        }

        /// <summary>
        /// 合并网格(同材质合并)
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iTarget">目标对象</param>
        /// <returns>合并后的对象</returns>
        private static GameObject CombineMeshBySameMat(
            ProgressCounter iProgressCounter, GameObject iTarget)
        {
            
            // 取得原来对象所有网格
            var config = GenerateCombineConfig(iTarget);
            
            // 生成一个新物体
            var newGo = new GameObject($"{iTarget.name}_Tmp");
            if (null == newGo)
            {
                Loger.Error($"ModelEditor::CombineMeshWithSameMat():GameObject Create Failed!(Name:{iTarget.name})");
                return null; 
            }
            
            // 更新计数统计设定(网格合并/导出网格数据/导出材质数据)
            iProgressCounter.UpdateStepInfo((int) CombineMeshStep.CombineMesh, config.Items.Count, false);
            
            // 循环便利材质并合并相同材质
            var flg = false;
            foreach (var item in config.Items)
            {
                var child = new GameObject(item.Key);
                child.transform.parent = newGo.transform;
                var cbmf = child.AddComponent<MeshFilter>();
                if(null == cbmf) continue;
                var ms = new Mesh();
                ms.CombineMeshes(
                    item.Value.CombineInstances.ToArray(), 
                    true,true, true); 
                ms.UploadMeshData(true);
                //合拼会自动生成UV3，但是我们并不需要，可以这样删除
                ms.uv3 = null;
                cbmf.sharedMesh = ms;
                // 更新进度信息(网格合并)
                iProgressCounter.UpdateByStep(
                    (int) CombineMeshStep.CombineMesh, $"网格合并:{item.Key}");
                
                var cmr = child.AddComponent<MeshRenderer>();
                if(null == cmr) continue;
                child.SetActive(true);
                cmr.sharedMaterial = item.Value.Mat;
                cmr.enabled = true;
                cmr.shadowCastingMode = ShadowCastingMode.Off;
                cmr.receiveShadows = true;
                if (!flg) flg = true;

            }
            newGo.SetActive(true);
            if (!flg)
            {
                Object.DestroyImmediate(newGo);
                newGo = null;
            }
            
            // 刷新本地
            UtilsAsset.AssetsRefresh();
            
            config.Items.Clear();
            return newGo;
        }

        /// <summary>
        /// 合并材质和纹理
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iTarget">目标对象</param>
        /// <param name="iSimplify">降面信息</param>
        /// <param name="iExportDir">导出目录</param>
        /// <param name="iOnFailed">失败处理</param>
        /// <returns>合并后的对象</returns>
        private static GameObject CombineMatsAndTextures(
            ProgressCounter iProgressCounter, GameObject iTarget,
            SimplifyInfo iSimplify, string iExportDir,
            Action<string, string> iOnFailed = null)
        {
            
            // 取得原来对象所有网格
            var config = GenerateCombineConfig(iTarget);
            // 取得相同Shader的材质列表
            if (config == null || 0 >= config.Items.Count) return null;
            // 取得简化降面用着色器纹理属性列表
            var properties = GetTexturePropertiesFromShader(iSimplify.Shader);
            if (null == properties)
            {
                Loger.Error($"ModelEditor::CombineMatsAndTextures():There is no texture property in this shader {iSimplify.Shader}");
                return null; 
            }
            var count = properties.Count;
            if (0 >= count)
            {
                Loger.Error($"ModelEditor::CombineMatsAndTextures():There is no texture property in this shader {iSimplify.Shader}");
                return null;
            }
            // 检测校验材质纹理信息
            var checkMsg = "";
            if (!config.CheckTextureOfMat(properties.ToArray(), ref checkMsg))
            {
                iOnFailed?.Invoke("错误", $"材质纹理校验失败!\n{checkMsg}");
                return null;
            }

            // 生成新的预制体
            var newGo = new GameObject(iTarget.name);
            if (null == newGo)
            {
                return null;
            }
            var mr = newGo.AddComponent<MeshRenderer>();
            // 生成新材质
            var newMat = new Material(Shader.Find(iSimplify.Shader));
            var newMatDir = GetExportMatDir(iExportDir);
            var newMatFilePath = $"{newMatDir}/{iTarget.name}.mat";
            // 生成Asset
            AssetDatabase.CreateAsset(newMat, newMatFilePath);
            
            // 重新设定新的网格数据对象
            newMat = AssetDatabase.LoadAssetAtPath(newMatFilePath, typeof(Material)) as Material;
            if (null == newMat)
            {
                Loger.Error($"ModelEditor::CombineMatsAndTextures():LoadAssetAtPath new mat data failed!" +
                            $"(name:{mr.gameObject.name}) is invalid!(path={newMatFilePath})");
                return null;
            }
            mr.sharedMaterial = newMat;
            
            // 追加降面进度Step
            iProgressCounter.AddStepInfo((int) CombineMeshStep.CombineMatAndTexture, 
                count, true);
            iProgressCounter.AddStepInfo((int) CombineMeshStep.CombineMesh, 
                1, true);
            
            // 根据降化见面用着色器材质属性一览遍历
            var isUvReset = false;
            foreach (var property in properties)
            {
                // 更新进度计数 - 重置网格Uv
                iProgressCounter.AddStepInfo((int) CombineMeshStep.TexturePack, 
                    1, true);
                var propertyDesc = GetPropertyDescByShader(iSimplify.Shader, property);
                var isNormalMap = !string.IsNullOrEmpty(propertyDesc) && "NormalMap".ToLower().Equals(propertyDesc.ToLower());

                // 打包纹理(自动保存)
                Dictionary<int, int> indexs;
                var rects = config.PackTextures(
                    property, isNormalMap, 0, 
                    iSimplify.AtlasMaxSize, iSimplify.TextureReduce,
                    newMat, out indexs, iExportDir);
                if (null == rects || 0 >= rects.Length)
                {
                    iProgressCounter.UpdateByStep(
                        (int) CombineMeshStep.TexturePack, $"图集合并(Skip):{property} no textures");
                    continue;
                }
                iProgressCounter.UpdateByStep(
                    (int) CombineMeshStep.TexturePack, $"图集合并:{property}"); 
                
                if(null == indexs || 0 >= indexs.Count) continue;
                
                // 刷新网格纹理UV坐标
                if (!isUvReset)
                {
                    Action<int, int, bool, string> resetUvsUpdate = 
                        delegate(int iCurIdx, int iMaxCount, bool iAccumulation, string iInfo)
                    {
                        iProgressCounter.UpdateByStep(
                            (int) CombineMeshStep.ResetMeshUvs, $"Uv重置:{iInfo}", iAccumulation ? 1 : 0); 
                    };
                    
                    foreach (var it in config.Items)
                    {
                        // 更新进度计数 - 重置网格Uv
                        iProgressCounter.AddStepInfo((int) CombineMeshStep.ResetMeshUvs, 
                            it.Value.CombineInstances.Count, true);
                        
                        var idx = -1;
                        if (indexs.TryGetValue(it.Value.InstanceID, out idx))
                        {
                            if(-1 >= idx) continue;
                            var rect = rects[idx];
                            if (null == it.Value)
                            {
                                Loger.Error($"ModelEditor::CombineMatsAndTextures():CombineItem missing!!!(key:{it.Key})");
                                continue;
                            }
                            // 重置UV坐标
                            it.Value.ResetMeshUvs(rect, resetUvsUpdate);
                        }
                    }
                    isUvReset = true;
                }
                
                // 更新进度信息(材质合并)
                iProgressCounter.UpdateByStep(
                    (int) CombineMeshStep.CombineMatAndTexture, $"材质合并:{property}");
            }
            
            // 取得合并用实例列表
            var combines = config.CombineInstances();
            var backupFiles = new List<string>();
            foreach (var combine in combines)
            {
                if(null == combine.mesh) continue;
                var backupFilePath = AssetDatabase.GetAssetPath(combine.mesh);
                if(string.IsNullOrEmpty(backupFilePath) || !File.Exists(backupFilePath)) continue;
                if(backupFiles.Exists(iO => backupFilePath.Equals(iO))) continue;
                backupFiles.Add(backupFilePath);
            }
            // 更新进度计数 - 网格数据备份
            iProgressCounter.AddStepInfo((int) CombineMeshStep.DelBackUpData, 
                backupFiles.Count, true);
            
            // 网格合并
            var mf = newGo.AddComponent<MeshFilter>();
            var newMesh = new Mesh { name = iTarget.name };
            
            newMesh.CombineMeshes(combines.ToArray(), true,true);//合并网格
            mf.mesh = newMesh;
            
            // 更新进度信息(网格合并)
            iProgressCounter.UpdateByStep(
                (int) CombineMeshStep.CombineMesh, $"网格合并:{iTarget.name}");
            
            // 清除备份文件
            foreach (var file in backupFiles)
            {
                var fileName = UtilsTools.GetFileName(file);
                File.Delete(file);
                // 更新进度信息(网格合并)
                iProgressCounter.UpdateByStep(
                    (int) CombineMeshStep.DelBackUpData, $"清除备份:{fileName}");
            }
            
            // 释放
            Object.DestroyImmediate(iTarget);
            return newGo;
        }
        
#endregion

#region Simplify

        /// <summary>
        /// 开始网格降面
        /// </summary>
        /// <param name="iProgressCounter">进度计数器</param>
        /// <param name="iSimplifyGo">简化降面用GameObject</param>
        /// <param name="iExportDir">导出目录</param>
        /// <param name="iSimplifyInfo">降面信息</param>
        /// <param name="iImporterSettings">平台导入设定</param>
        /// <param name="iOnFailed">失败回调函数</param>
        private static bool StartModelSimplify(
            ProgressCounter iProgressCounter,
            GameObject iSimplifyGo, string iExportDir, 
            SimplifyInfo iSimplifyInfo, ModelMeshSettingsData iImporterSettings,
            Action<string, string> iOnFailed = null)
        {
            // 追加降面进度信息
            iProgressCounter.AddStepInfo((int)CombineMeshStep.ModelSimplify, 1, true);
            iProgressCounter.CurStep = (int) CombineMeshStep.ModelSimplify;
            // 降面
            _progressCounter = iProgressCounter;
            var ret = StartModelSimplify(iSimplifyGo, iSimplifyInfo.SimplifyRatio);
            var prefabName = iSimplifyGo.name;
            if (!ret)
            {
                iOnFailed?.Invoke("错误",
                    $"预制体降面失败！！\nPrefab:{prefabName}");
                // 结束进度条
                iProgressCounter.EndCounter();
            }
            else
            {
                // 更新降面进度信息
                iProgressCounter.UpdateByStep((int) CombineMeshStep.ModelSimplify,
                    $"预制体降面:{prefabName}");
            }
            
            // 追加计数器Step
            iProgressCounter.AddStepInfo(
                (int)CombineMeshStep.ExportPrefab, 1, true);
            // 导出预制体
            ret = ExportPrefab(iProgressCounter, iSimplifyGo, 
                iExportDir, ref iSimplifyInfo.Physical,
                null, iImporterSettings);
            if (!ret)
            {
                iOnFailed?.Invoke("错误",
                    $"预制体导出失败!\nPath:{iExportDir}");
                return ret;
            }
            // 更新计数器Step追加
            iProgressCounter.UpdateByStep((int) CombineMeshStep.ExportPrefab, 
                $"预制体导出:{prefabName}");
            
            return ret;
        }
        
        /// <summary>
        /// 降面进度
        /// </summary>
        /// <param name="iTitle">标题</param>
        /// <param name="iMessage">消息</param>
        /// <param name="iProgress">进度</param>
        private static void SimplifyProgress(string iTitle, string iMessage, float iProgress)
        {
            var percent = Mathf.RoundToInt(iProgress * 100.0f);

            Loger.Info($"ModelEditor::SimplifyProgress():{percent}% {iTitle} - {iMessage}");

            var status = $"降面中:{percent}% {iTitle}";
            UpdateProgressBarStatus(status);

            // if(nPercent != s_nLastProgress || s_strLastTitle != strTitle || s_strLastMessage != strMessage)
            // {
            //     s_strLastTitle   = strTitle;
            //     s_strLastMessage = strMessage;
            //     s_nLastProgress  = nPercent;
            //
            //     if(EditorUtility.DisplayCancelableProgressBar(strTitle, strMessage, fT))
            //     {
            //         Simplifier.Cancelled = true;
            //     }
            // }
        }

        /// <summary>
        /// 开始降面操作
        /// </summary>
        /// <param name="iSimplifyGo">降面GameObject</param>
        /// <param name="iSimplifyRatio">降面率(顶点百分比)</param>
        /// <returns>true:降面成功; false:降面失败;</returns>
        private static bool StartModelSimplify(
            GameObject iSimplifyGo, float iSimplifyRatio)
        {
            var flg = true;
            try
            {
             
                if (null == iSimplifyGo) return false;
                // 记载降面用的脚本
                var script = iSimplifyGo.AddComponent<MeshSimplify>();
                if (null == script) return false;
                script.m_fVertexAmount = iSimplifyRatio;
                
                if (script.HasDataDirty() || script.HasData() == false || script.HasNonMeshSimplifyGameObjectsInTree())
                {
                    script.RestoreOriginalMesh(true, script.m_meshSimplifyRoot == null);
                    script.ComputeData(script.m_meshSimplifyRoot == null, SimplifyProgress);

                    if (Simplifier.Cancelled)
                    {
                        script.RestoreOriginalMesh(true, script.m_meshSimplifyRoot == null);
                        return false;
                    }
                }

                script.ComputeMesh(script.m_meshSimplifyRoot == null, SimplifyProgress);

                if (Simplifier.Cancelled)
                {
                    return false;
                }

                script.AssignSimplifiedMesh(script.m_meshSimplifyRoot == null);

                if (script.m_strAssetPath != null && script.m_bEnablePrefabUsage)
                {
                    //SaveMeshAssets();
                }
                
                // 移除降面插件脚本
                Object.DestroyImmediate(script);
                var simplifier = iSimplifyGo.GetComponent<Simplifier>();
                if (null != simplifier)
                {
                    Object.DestroyImmediate(simplifier); 
                }

            }
            catch (Exception e)
            {
                Loger.Fatal($"Error generating mesh: {e.Message} Stack: {e.StackTrace}");
                flg = false;
            }
            finally
            {
                Simplifier.Cancelled = false;
            }
            return flg;
        } 

#endregion

#region Model - PreviewIcon

        /// <summary>
        /// 基准路径
        /// </summary>
        public const string BaseDir = "Assets/Packages/Models/Editor";

        /// <summary>
        /// 产生预览Icon纹理
        /// </summary>
        /// <param name="iPrefabPath">预制体路径</param>
        /// <param name="iIsReCreate">重新生成标识位</param>
        internal static Texture GeneratePreviewIcon (
            string iPrefabPath, bool iIsReCreate = true)
        {
            var outputDir = $"{BaseDir}/Preview";
            // 校验并生成相应目录
            if(!UtilsTools.CheckAndCreateDirByFullDir(outputDir)) return null;
            // guid
            var guid = AssetDatabase.AssetPathToGUID(iPrefabPath);
            if (string.IsNullOrEmpty(guid))
            {
                Loger.Error($"ModelEditor::GeneratePreviewIcon():GUID Get Failed!!");
                return null;
            }
            
            var prefabName = UtilsTools.GetFileName(iPrefabPath, false);
            var previewPath = $"{outputDir}/{prefabName}_{guid}.png"; 
            // 不重新生成，且文件存在
            if (!iIsReCreate && File.Exists(previewPath))
            {
                return GUIEditorHelper.LoadPreviewIconTexture(previewPath);
            }

            // 若文件存在，则删除
            if (File.Exists(previewPath))
            {
                File.Delete(previewPath);
            }
            
            // 加载预制体
            var prefab = AssetDatabase.LoadAssetAtPath(iPrefabPath, typeof(GameObject)) as GameObject;
            // 取得Icon
            var icon = GUIEditorHelper.GetAssetPreviewTexture(prefab);
            if (null != icon)
            {
                // 保存Icon
                GUIEditorHelper.SaveTexture(icon, previewPath);
            }
            else
            {
                Loger.Error($"ModelEditor::GeneratePreviewIcon():Generate Failed!!");

            }
            return icon;
        }
        
#endregion

#region Texture2D

        /// <summary>
        /// 最大纹理尺寸
        /// </summary>
        public static List<int> TextureMaxSizes = new List<int>
        {
            128, 256, 512, 1024, 2048, 4096
        };

        /// <summary>
        /// 纹理导入 - 类型
        /// </summary>
        public static TextureImporterTypes TextureImporterTypes = new TextureImporterTypes
        {
            List = new List<EnumDisplay<TextureImporterType>>()
            {
                new EnumDisplay<TextureImporterType> { Value = TextureImporterType.Default,  Text = "Default" },
                new EnumDisplay<TextureImporterType> { Value = TextureImporterType.NormalMap,  Text = "Normal Map" },
                new EnumDisplay<TextureImporterType> { Value = TextureImporterType.GUI,  Text = "Editor GUI and Legacy GUI" },
                new EnumDisplay<TextureImporterType> { Value = TextureImporterType.Sprite,  Text = "Sprite(2D and UI)" },
                new EnumDisplay<TextureImporterType> { Value = TextureImporterType.Cursor,  Text = "Cursor" },
                new EnumDisplay<TextureImporterType> { Value = TextureImporterType.Cookie,  Text = "Cookie" },
                new EnumDisplay<TextureImporterType> { Value = TextureImporterType.Lightmap,  Text = "Light Map" },
                new EnumDisplay<TextureImporterType> { Value = TextureImporterType.SingleChannel,  Text = "Single Channel" }
            } 
        };
        
        /// <summary>
        /// 纹理导入 - 压缩质量
        /// </summary>
        public static TextureImporterCompressions TextureImporterCompressions = new TextureImporterCompressions
        {
            List = new List<EnumDisplay<TextureImporterCompression>>()
            {
                new EnumDisplay<TextureImporterCompression> { Value = TextureImporterCompression.Uncompressed,  Text = "None" },
                new EnumDisplay<TextureImporterCompression> { Value = TextureImporterCompression.Compressed,  Text = "Normal Quality" },
                new EnumDisplay<TextureImporterCompression> { Value = TextureImporterCompression.CompressedLQ,  Text = "Low Quality" },
                new EnumDisplay<TextureImporterCompression> { Value = TextureImporterCompression.CompressedHQ,  Text = "High Quality" }
            }
        };

        /// <summary>
        /// 纹理导入格式 - 默认
        /// </summary>
        public static TextureImporterFormats TextureImporterFormatsOfDefault = new TextureImporterFormats
        {
            List = new List<EnumDisplay<TextureImporterFormat>>()
            {
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.Automatic,  Text = "Automatic" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBA32,  Text = "RGBA 32 bit" }
            }
        };

        /// <summary>
        /// 纹理导入格式 - 平台
        /// </summary>
        public static TextureImporterFormats TextureImporterFormatsOfPlatform = new TextureImporterFormats
        {
            List = new List<EnumDisplay<TextureImporterFormat>>()
            {
#if UNITY_STANDALONE
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.Alpha8,  Text = "Alpha 8" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ARGB16,  Text = "ARGB 16 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGB24,  Text = "RGB 24 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBA32,  Text = "RGBA 32 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGB16,  Text = "RGB 16 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.R16,  Text = "R 16 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.DXT1,  Text = "RGB Compressed DXT1" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.DXT5,  Text = "RGBA Compressed DXT5" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBAHalf,  Text = "RGBA Half" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.BC6H,  Text = "RGB HDR Compressed BC6H" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.BC7,  Text = "RGB(A) Compressed BC7" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.BC4,  Text = "R Compressed BC4" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.BC5,  Text = "RG Compressed BC5" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.DXT1Crunched,  Text = "RGB Crunched DXT1" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.DXT5Crunched,  Text = "RGBA Crunched DXT5" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.R8,  Text = "R8" }
#elif UNITY_ANDROID || UNITY_ANDROID_API
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGB24, Text = "RGB 24 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBA32, Text = "RGBA 32 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGB16, Text = "RGB 16 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.DXT1, Text = "RGB Compressed DXT1" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.DXT5, Text = "RGBA Compressed DXT5" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBA16, Text = "RGBA 16 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBAHalf, Text = "RGBA Half" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.DXT1Crunched, Text = "RGB Crunched DXT1" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.DXT5Crunched, Text = "RGBA Crunched DXT5" },
                // TODO:Compressior Quality 暂时保留
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.PVRTC_RGB2, Text = "RGB Compressed PVRTC 2 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.PVRTC_RGBA2, Text = "RGBA Compressed PVRTC 2 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.PVRTC_RGB4, Text = "RGB Compressed PVRTC 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.PVRTC_RGBA4, Text = "RGBA Compressed PVRTC 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC_RGB4, Text = "RGB Compressed ETC 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC2_RGB4, Text = "RGB Compressed ETC2 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA, Text = "RGB +1-bit Alpha Compressed ETC2 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC2_RGBA8, Text = "RGBA Compressed ETC2 8 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_4x4, Text = "RGB Compressed ASTC 4x4 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_5x5, Text = "RGB Compressed ASTC 5x5 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_5x5, Text = "RGB Compressed ASTC 5x5 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_6x6, Text = "RGB Compressed ASTC 6x6 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_8x8, Text = "RGB Compressed ASTC 8x8 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_10x10, Text = "RGB Compressed ASTC 10x10 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_12x12, Text = "RGB Compressed ASTC 12x12 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_4x4, Text = "RGBA Compressed ASTC 4x4 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_5x5, Text = "RGBA Compressed ASTC 5x5 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_5x5, Text = "RGBA Compressed ASTC 5x5 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_6x6, Text = "RGBA Compressed ASTC 6x6 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_8x8, Text = "RGBA Compressed ASTC 8x8 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_10x10, Text = "RGBA Compressed ASTC 10x10 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_12x12, Text = "RGBA Compressed ASTC 12x12 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC_RGB4Crunched, Text = "RGB Crunched ETC" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC2_RGBA8Crunched, Text = "RGBA Crunched ETC2" }
#elif UNITY_IPHONE || UNITY_IOS 
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.Alpha8,  Text = "Alpha 8" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGB24,  Text = "RGB 24 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBA32,  Text = "RGBA 32 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGB16,  Text = "RGB 16 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.R16,  Text = "R 16 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBA16,  Text = "RGBA 16 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.RGBAHalf, Text = "RGBA Half" },
                // TODO:Compressior Quality 暂时保留
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.PVRTC_RGB2, Text = "RGB Compressed PVRTC 2 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.PVRTC_RGBA2, Text = "RGBA Compressed PVRTC 2 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.PVRTC_RGB4, Text = "RGB Compressed PVRTC 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.PVRTC_RGBA4, Text = "RGBA Compressed PVRTC 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC_RGB4, Text = "RGB Compressed ETC 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.EAC_R, Text = "R Compressed EAC 4 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.EAC_RG, Text = "RG Compressed EAC 8 bit" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC2_RGB4, Text = "RGB Compressed ETC2 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA, Text = "RGB +1-bit Alpha Compressed ETC2 4 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC2_RGBA8, Text = "RGBA Compressed ETC2 8 bits" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_4x4, Text = "RGB Compressed ASTC 4x4 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_5x5, Text = "RGB Compressed ASTC 5x5 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_5x5, Text = "RGB Compressed ASTC 5x5 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_6x6, Text = "RGB Compressed ASTC 6x6 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_8x8, Text = "RGB Compressed ASTC 8x8 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_10x10, Text = "RGB Compressed ASTC 10x10 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGB_12x12, Text = "RGB Compressed ASTC 12x12 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_4x4, Text = "RGBA Compressed ASTC 4x4 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_5x5, Text = "RGBA Compressed ASTC 5x5 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_5x5, Text = "RGBA Compressed ASTC 5x5 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_6x6, Text = "RGBA Compressed ASTC 6x6 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_8x8, Text = "RGBA Compressed ASTC 8x8 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_10x10, Text = "RGBA Compressed ASTC 10x10 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ASTC_RGBA_12x12, Text = "RGBA Compressed ASTC 12x12 block" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.R8, Text = "R8" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC_RGB4Crunched, Text = "RGBA Crunched ETC" },
                new EnumDisplay<TextureImporterFormat> { Value = TextureImporterFormat.ETC2_RGBA8Crunched, Text = "RGBA Crunched ETC2" }

#endif
            }
        };
        
        /// <summary>
        /// 不支持ETC2的Android设备上的ETC2纹理解压缩格式覆盖
        /// </summary>
        public static AndroidETC2FallbackOverrides AndroidETC2Fallbacks = new AndroidETC2FallbackOverrides
        {
            List = new List<EnumDisplay<AndroidETC2FallbackOverride>>()
            {
                new EnumDisplay<AndroidETC2FallbackOverride> { Value = AndroidETC2FallbackOverride.UseBuildSettings,  Text = "Use Build Settings" },
                new EnumDisplay<AndroidETC2FallbackOverride> { Value = AndroidETC2FallbackOverride.Quality32Bit,  Text = "32-bit" },
                new EnumDisplay<AndroidETC2FallbackOverride> { Value = AndroidETC2FallbackOverride.Quality16Bit,  Text = "16-bit" },
                new EnumDisplay<AndroidETC2FallbackOverride> { Value = AndroidETC2FallbackOverride.Quality32BitDownscaled,  Text = "32-bit(half resolution)" }
            }
        };

        /// <summary>
        /// 保存纹理
        /// </summary>
        /// <param name="iTex">纹理</param>
        /// <param name="iSaveDir">保存目录</param>
        /// <param name="iForce">强制标志位(true:如果文件存在，则强制删除;false:若文件存在，则不执行，返回;)</param>
        /// <param name="iFileExtension">文件后缀(默认格式:*.tga)</param>
        /// <returns>保存路径</returns>
        public static string SaveTexture( 
            Texture2D iTex, string iSaveDir,
            bool iForce = true, string iFileExtension = ".tga")
        {
            // 检测导出目录
            var exportDir = $"{iSaveDir}/{_EXPORT_DATA_TEXTURES}"; 
            // 校验数据输出目录
            if (!UtilsTools.CheckAndCreateDirByFullDir(exportDir))
            {
                Loger.Error($"ModelEditor::SaveTexture():CheckAndCreateDirByFullDir Failed!(Dir={exportDir})");
                return null; 
            }

            var filePath = $"{exportDir}/{iTex.name}{iFileExtension}";
            var existFlg = File.Exists(filePath);
            if (!iForce && existFlg)
            {
                return filePath;
            }
            if (existFlg)
            {
                File.Delete(filePath);
            }
                    
            using (var fs = File.Open(filePath, FileMode.Create))
            {
                var writer = new BinaryWriter(fs);
                var bytes= iTex.EncodeToTGA();
                writer.Write(bytes);
                fs.Close();
            }
            
            AssetDatabase.ImportAsset(filePath);
            // Object.DestroyImmediate(iTex);
            UtilsAsset.AssetsRefresh();

            return filePath;
        }

        /// <summary>
        /// 转换纹理到目标尺寸的纹理像素数组
        /// </summary>
        /// <param name="iSource">源纹理</param>
        /// <param name="iWidth">目标纹理宽度</param>
        /// <param name="iHeight">目标纹理高度</param>
        /// <returns>目标尺寸的纹理像素数组</returns>
        public static Color[] ConvertTexture(
            Texture2D iSource, int iWidth, int iHeight)
        {
            
            // 设定文件Read/Write Enabled
            if (!SetTextureReadWriteEnable(iSource))
            {
                return null;
            }

            var pixels = new Color[iWidth * iHeight];
            for (var h = 0; h < iHeight; ++h) {
                for (var w = 0; w < iWidth; ++w) {
                    var newColor = iSource.GetPixelBilinear(
                        w / (float)iWidth, h / (float)iHeight);
                    pixels[h * iWidth + w] = newColor;
                }
            }

            return !SetTextureReadWriteEnable(iSource, false) ? null : pixels;
        }

#region ImportSettings

        /// <summary>
        /// 设定纹理类型
        /// </summary>
        /// <param name="iTex">纹理</param>
        /// <param name="iFormat">纹理类型</param>
        /// <param name="isNormalMap">是否为法线贴图</param>
        /// <returns>true:OK; false:NG;</returns>
        public static bool SetTextureImportSettins(
            Texture2D iTex, TextureImporterFormat iFormat, bool isNormalMap)
        {
            if (null == iTex)
            {
                Loger.Error($"ModelEditor::SetTextureImportSettins():The texture2d is invalid or null!!");
                return false;
            }
            
            var textPath = AssetDatabase.GetAssetPath(iTex);
            if (string.IsNullOrEmpty(textPath))
            {
                Loger.Error($"ModelEditor::SetTextureImportSettins():AssetDatabase.GetAssetPath Failed!!" +
                            $"(Texture2D Name:{iTex.name})");
                return false;
            }
            var ti = (TextureImporter)AssetImporter.GetAtPath(textPath);
            if (null == ti)
            {
                Loger.Error($"ModelEditor::SetTextureImportSettins():AssetImporter.GetAtPath Failed!!" +
                            $"(Texture2D Path:{textPath})");
                return false;
            }

            // 设定纹理格式
            var pSettings = ti.GetDefaultPlatformTextureSettings();
            pSettings.format = iFormat;
            if (isNormalMap) ti.textureType = TextureImporterType.NormalMap;
            
            AssetDatabase.ImportAsset(textPath);
            // 刷新
            UtilsAsset.AssetsRefresh();
            return true;
        }

        /// <summary>
        /// 设定纹理属性：Read/Write Enabled
        /// </summary>
        /// <param name="iTex">纹理</param>
        /// <param name="iEnable">Enable</param>
        /// <returns>true:OK; false:NG;</returns>
        public static bool SetTextureReadWriteEnable(Texture2D iTex, bool iEnable = true)
        {
            if (null == iTex)
            {
                Loger.Error($"ModelEditor::SetTextureReadWriteEnable():The texture2d is invalid or null!!");
                return false;
            }
            
            var textPath = AssetDatabase.GetAssetPath(iTex);
            if (string.IsNullOrEmpty(textPath))
            {
                Loger.Error($"ModelEditor::SetTextureReadWriteEnable():AssetDatabase.GetAssetPath Failed!!(Texture2D Name:{iTex.name})");
                return false;
            }
            var ti = (TextureImporter)AssetImporter.GetAtPath(textPath);
            if (null == ti)
            {
                Loger.Error($"ModelEditor::SetTextureReadWriteEnable():AssetImporter.GetAtPath Failed!!(Texture2D Path:{textPath})");
                return false;
            }

            if (iEnable == ti.isReadable) return true;
            ti.isReadable = iEnable;
            AssetDatabase.ImportAsset(textPath);
            // 刷新
            UtilsAsset.AssetsRefresh();
            return true;
        }
        
        /// <summary>
        /// 应用纹理平台设定
        /// </summary>
        /// <param name="iImporterSetting">导入设定信息</param>
        /// <param name="iTex">纹理</param>
        /// <returns>true:OK; false:NG;</returns>
        private static bool ApplyTexturePlatformSettings(
            ModelMeshSettingsData iImporterSetting, Texture iTex)
        {
            if (null == iImporterSetting)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettings():The importer setting is invalid or null!!");
                return false;
            }
            if (null == iTex)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettings():The texture2d is invalid or null!!");
                return false;
            }
            var textPath = AssetDatabase.GetAssetPath(iTex);
            if (string.IsNullOrEmpty(textPath))
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettings():AssetDatabase.GetAssetPath Failed!!" +
                            $"(Texture2D Name:{iTex.name})");
                return false;
            }
            var ti = (TextureImporter)AssetImporter.GetAtPath(textPath);
            if (null == ti)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettings():AssetImporter.GetAtPath Failed!!" +
                            $"(Texture2D Path:{textPath})");
                return false;
            }

            // 取得设定信息
            var settings = iImporterSetting.GetImporterSettingByType(ti.textureType);
            if (null == settings?.Platform)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettings():Platform.settings is not exist or invalid!!" +
                            $"(Type:{ti.textureType}\n Importer Settings:{iImporterSetting})");
                return false; 
            }
            ti.SetPlatformTextureSettings(settings.Platform);
            
            // 应用导入设定
            AssetDatabase.ImportAsset(textPath);
            // 刷新
            UtilsAsset.AssetsRefresh();
            return true;
        }
        
        /// <summary>
        /// 应用纹理平台设定
        /// </summary>
        /// <param name="iImporterSetting">导入设定信息</param>
        /// <param name="iTexPath">纹理路径</param>
        /// <returns>true:OK; false:NG;</returns>
        private static bool ApplyTexturePlatformSettings(
            ModelMeshSettingsData iImporterSetting, string iTexPath)
        {
            if (null == iImporterSetting)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettings():The importer setting is invalid or null!!");
                return false;
            }
            var ti = (TextureImporter)AssetImporter.GetAtPath(iTexPath);
            if (null == ti)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettings():AssetImporter.GetAtPath Failed!!(Texture2D Path:{iTexPath})");
                return false;
            }

            // 取得设定信息
            var settings = iImporterSetting.GetImporterSettingByType(ti.textureType);
            if (null == settings?.Platform)
            {
                Loger.Error($"ModelEditor::ApplyTexturePlatformSettings():Platform.settings is not exist or invalid!!" +
                            $"(Type:{ti.textureType}\n Importer Settings:{iImporterSetting})");
                return false; 
            }
            ti.SetPlatformTextureSettings(settings.Platform);

            // 应用导入设定
            AssetDatabase.ImportAsset(iTexPath);
            // 刷新
            UtilsAsset.AssetsRefresh();
            return true;
        }
        
#endregion


#endregion

#region Tabs

        public const string _TAB_DEFAULT = "Default";
        public const string _TAB_PC = "PC";
        public const string _TAB_ANDROID = "Android";
        public const string _TAB_IOS = "iOS";
        public const string _TAB_SETTINGS = "Settings";
        /// <summary>
        /// Tab列表定义
        /// </summary>
        public static List<string> Tabs = new List<string>
        {
            _TAB_DEFAULT, _TAB_PC, _TAB_ANDROID, _TAB_IOS, _TAB_SETTINGS
        };
        
        /// <summary>
        /// 检测Tab和TabIndex是否匹配
        /// </summary>
        /// <param name="iTab">Tab</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns></returns>
        private static bool CheckTabIndex(string iTab, int iTabIndex)
        {
            if (iTabIndex >= Tabs.Count) return false;
            if (string.IsNullOrEmpty(iTab)) return false;
            var tab = Tabs[iTabIndex];
            return !string.IsNullOrEmpty(tab) && tab.Equals(iTab);
        }

        /// <summary>
        /// 取得Tab索引
        /// </summary>
        /// <param name="iTab">Tab</param>
        /// <returns>Tab索引</returns>
        public static int GetTabIndexByTab(string iTab)
        {
            return Tabs.IndexOf(iTab);
        }

        /// <summary>
        /// 判断是否为默认Tab
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>是否为默认Tab</returns>
        public static bool isDefaultTab(int iTabIndex)
        {
            return CheckTabIndex(_TAB_DEFAULT, iTabIndex);
        }
        
        /// <summary>
        /// 判断是否为PC Tab
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>是否为PC Tab</returns>
        public static bool isPCTab(int iTabIndex)
        {
            return CheckTabIndex(_TAB_PC, iTabIndex);
        }
        
        /// <summary>
        /// 判断是否为Android Tab
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>是否为Android Tab</returns>
        public static bool isAndroidTab(int iTabIndex)
        {
            return CheckTabIndex(_TAB_ANDROID, iTabIndex); 
        }

        /// <summary>
        /// 判断是否为iOS Tab
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>是否为iOS Tab</returns>
        public static bool isIOSTab(int iTabIndex)
        {
            return CheckTabIndex(_TAB_IOS, iTabIndex); 
        }

        /// <summary>
        /// 判断是否为开发平台Tab
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>是否为开发平台Tab</returns>
        public static bool isPlatformTab(int iTabIndex)
        {
            return isPCTab(iTabIndex) || isAndroidTab(iTabIndex) || isIOSTab(iTabIndex) ;
        }
        
        /// <summary>
        /// 判断是否为Settings Tab
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>是否为Settings Tab</returns>
        public static bool isSettingsTab(int iTabIndex)
        {
            return CheckTabIndex(_TAB_SETTINGS, iTabIndex); 
        }

        /// <summary>
        /// 取得打包目标名
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>打包目标名</returns>
        public static string GetBuildTargetNameByTabIndex(int iTabIndex)
        {
            var tab = Tabs[iTabIndex];
            var buildTargetName = "DefaultTexturePlatform";
            switch (tab)
            {
                case _TAB_PC:
                    buildTargetName = "Standalone";
                    break;
                case _TAB_ANDROID:
                    buildTargetName = $"{BuildTarget.Android}";
                    break;
                case _TAB_IOS:
                    // buildTargetName = $"{BuildTarget.iPhone}";
                    buildTargetName = "iPhone";
                    break;
            }
            return buildTargetName;
        }

#endregion

#region Shader

        /// <summary>
        /// 属性匹配信息
        ///  key : 当前属性名
        ///  value : 匹配属性名列表(从左到右为，匹配优先顺序)
        /// </summary>
        private static Dictionary<string, List<string>> PropertiesMatches = new Dictionary<string, List<string>>
        {
            // 主纹理
            { "_MainTex", new List<string> { "_MainTex" } },
            // 法线纹理
            { "_BumpMap", new List<string> { "_BumpMap", "_NormalMap" } },
            { "_NormalMap", new List<string> { "_NormalMap", "_BumpMap" } },
            // 高光纹理
            { "_MetallicRoughness", new List<string> { "_MetallicRoughness" } }
        };

        /// <summary>
        /// 取得着色器属性匹配优先列表
        /// </summary>
        /// <param name="iProperty">属性</param>
        /// <returns>着色器属性匹配优先列表</returns>
        public static List<string> GetShaderPropertyMatchList(string iProperty)
        {
            return !PropertiesMatches.TryGetValue(iProperty, out var listTmp) ? null : listTmp;
        }
        
        /// <summary>
        /// 着色器属性匹配校验
        ///  备注：不同着色器中，名字命名有所不同。比如法线纹理有_BumpMap，_NormalMap之分，但实际物理意义是相同的。
        /// </summary>
        /// <param name="iSourceProperty">源属性名</param>
        /// <param name="iDestSource">目标属性名</param>
        /// <returns>true:OK; false:NG;</returns>
        public static bool ShaderPropertyMatch(string iSourceProperty, string iDestSource)
        {
            if(string.IsNullOrEmpty(iSourceProperty) || string.IsNullOrEmpty(iDestSource)) return false;
            if (!PropertiesMatches.TryGetValue(iSourceProperty, out var listTmp)) return false;
            if (null == listTmp || 0 >= listTmp.Count) return false;
            var isExist = false;
            foreach (var property in listTmp
                .Where(iO => !string.IsNullOrEmpty(iO))
                .Where(iO => iO.Equals(iDestSource)))
            {
                isExist = true;
            }
            return isExist;
        }

        /// <summary>
        /// 取得纹理名
        /// </summary>
        /// <param name="iShader">着色器</param>
        /// <param name="iShaderTexturePropertyName">着色器纹理属性名</param>
        /// <returns>纹理名</returns>
        private static string GetPropertyNameByShader(
            Shader iShader, string iShaderTexturePropertyName = null)
        {
            if (null == iShader) return null;
            var count = ShaderUtil.GetPropertyCount(iShader);
            string propertyName = null;
            string firstName = null;
            var isExist = false;
            for (var idx = 0; idx < count; ++idx)
            {
                var type = ShaderUtil.GetPropertyType(iShader, idx);
                if ( ShaderUtil.ShaderPropertyType.TexEnv != type ) continue;
                
                propertyName = ShaderUtil.GetPropertyName(iShader, idx);
                if(string.IsNullOrEmpty(propertyName)) continue;
                // 保存第一个纹理属性名
                if (string.IsNullOrEmpty(firstName)) firstName = propertyName;
                // 为指定属性名，则返回第一个找到的属性名
                if (string.IsNullOrEmpty(iShaderTexturePropertyName)) break;
                if (!iShaderTexturePropertyName.Equals(propertyName)) continue;
                isExist = true;
                break;

            }
            return isExist ? propertyName : firstName;
        }

        /// <summary>
        /// 取得着色器属性描述
        /// </summary>
        /// <param name="iShaderName">着色器名</param>
        /// <param name="iPropertyName">属性名</param>
        /// <returns>属性描述</returns>
        public static string GetPropertyDescByShader(
            string iShaderName, string iPropertyName)
        {
            if (string.IsNullOrEmpty(iShaderName)) return null;
            var shader = Shader.Find(iShaderName);
            return GetPropertyDescByShader(shader, iPropertyName);
        }
        /// <summary>
        /// 取得着色器属性描述
        /// </summary>
        /// <param name="iShader">着色器</param>
        /// <param name="iPropertyName">属性名</param>
        /// <returns>属性描述</returns>
        public static string GetPropertyDescByShader(
            Shader iShader, string iPropertyName)
        {
            if (null == iShader) return null;
            if (string.IsNullOrEmpty(iPropertyName)) return null;
            var count = ShaderUtil.GetPropertyCount(iShader);
            string descTxt = null;
            for (var idx = 0; idx < count; ++idx)
            {
                // 取得属性名
                var propertyName = ShaderUtil.GetPropertyName(iShader, idx);
                if(string.IsNullOrEmpty(propertyName)) continue;
                if(!iPropertyName.Equals(propertyName)) continue;
                descTxt = ShaderUtil.GetPropertyDescription(iShader, idx);
                break;

            }
            return descTxt;
        }

        /// <summary>
        /// 取得着色器中纹理名列表
        /// </summary>
        /// <param name="iShaderName">着色器名</param>
        /// <returns>纹理名列表</returns>
        public static List<string> GetTexturePropertiesFromShader(string iShaderName)
        {
            if (string.IsNullOrEmpty(iShaderName)) return null;
            var shader = Shader.Find(iShaderName);
            return GetTexturePropertiesFromShader(shader);
        }
        
        /// <summary>
        /// 取得着色器中纹理名列表
        /// </summary>
        /// <param name="iShader">着色器</param>
        /// <returns>纹理名列表</returns>
        public static List<string> GetTexturePropertiesFromShader(Shader iShader)
        {
            if (null == iShader)
            {
                Loger.Error($"ModelEditor::GetTexturePropertyNamesFromShader():The Shader is invalid or not exist!!");
                return null;
            }
            var list = new List<string>();
            var count = ShaderUtil.GetPropertyCount(iShader);
            for (var idx = 0; idx < count; ++idx)
            {
                var type = ShaderUtil.GetPropertyType(iShader, idx);
                if ( ShaderUtil.ShaderPropertyType.TexEnv != type ) continue;
                
                var propertyName = ShaderUtil.GetPropertyName(iShader, idx);
                list.Add(propertyName);
            }

            return list;
        }

#endregion

    }
}

