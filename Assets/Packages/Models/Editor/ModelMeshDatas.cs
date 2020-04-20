using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Packages.Common.Base;
using Packages.Common.Counter;
using Packages.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Packages.Models.Editor
{
    /// <summary>
    /// 网格合并Step
    /// </summary>
    internal enum CombineMeshStep
    {
        /// <summary>
        /// 合并网格
        /// </summary>
        CombineMesh = 1,
        /// <summary>
        /// 导出网格数据
        /// </summary>
        ExportMeshData = 2,
        /// <summary>
        /// 导出材质数据
        /// </summary>
        ExportMatData = 3,
        /// <summary>
        /// 导出纹理数据
        /// </summary>
        ExportTexture = 4,
        /// <summary>
        /// 导出着色器数据
        /// </summary>
        ExportShader = 5,
        /// <summary>
        /// 导出预制体
        /// </summary>
        ExportPrefab = 6,
        /// <summary>
        /// 合并材质&纹理
        /// </summary>
        CombineMatAndTexture = 7,
        /// <summary>
        /// 重置网格Uv
        /// </summary>
        ResetMeshUvs = 8,
        /// <summary>
        /// 纹理打包
        /// </summary>
        TexturePack = 9,
        /// <summary>
        /// 模型降面
        /// </summary>
        ModelSimplify = 10,
        /// <summary>
        /// 应用平台设定
        /// </summary>
        ApplyPlatformSettings = 11,
        /// <summary>
        /// 清除备份数据
        /// </summary>
        DelBackUpData = 12
    }
    
    /// <summary>
    /// 网格合并模式
    /// </summary>
    internal enum CombineMode
    {
        /// <summary>
        /// 同材质导出
        ///   备注：仅合并相同材质的网格为一个网格
        /// </summary>
        SameMatUnitOnly,
        /// <summary>
        /// 全局单一
        ///   备注：全部合成一个完整的单一网格
        /// </summary>
        WholeSingle
    }

    /// <summary>
    /// 网格合并用Item信息
    /// </summary>
    internal class CombineItem : ClassExtension
    {
        /// <summary>
        /// 实例ID
        /// </summary>
        public int InstanceID;
        
        /// <summary>
        /// 材质
        /// </summary>
        public Material Mat = null;

        /// <summary>
        /// 纹理列表
        /// key : 着色器属性名。如：_MainTex
        /// value : 纹理
        /// </summary>
        private Dictionary<string, Texture2D> _textures = null;
        
        /// <summary>
        /// 合并实例列表
        /// </summary>
        public List<CombineInstance> CombineInstances = new List<CombineInstance>();

        /// <summary>
        /// 取得纹理
        /// </summary>
        /// <param name="iPropertyName">着色器属性名</param>
        /// <returns>纹理</returns>
        public Texture2D GetTextureByShaderProperty(string iPropertyName)
        {
            if (null == _textures)
            {
                if (null == Mat) return null;
                var properties = ModelEditor.GetTexturePropertiesFromShader(Mat.shader);
                if (null == properties || !properties.Any()) return null;
                _textures = new Dictionary<string, Texture2D>();
                foreach (var property in properties)
                {
                    var tex = Mat.GetTexture(property) as Texture2D;
                    if(null == tex) continue;
                    _textures.Add(property, tex);
                }
            }
            if (0 >= _textures.Count) return null;
            var matchProperties = ModelEditor.GetShaderPropertyMatchList(iPropertyName);
            if (null == matchProperties || 0 >= matchProperties.Count) return null;
            Texture2D matchTex = null;
            foreach (var property in matchProperties
                .Where(iO => _textures.TryGetValue(iO, out matchTex)))
            {
                break;
            }
            return matchTex;

        }
        
        /// <summary>
        /// 备份网格数据
        /// </summary>
        /// <param name="iBackUpDir">备份目录</param>
        /// <param name="iBackupUpdate">备份更新回调函数</param>
        public void BackUpMeshDatas(string iBackUpDir,
            Action<int, int, string> iBackupUpdate = null)
        {
            var idx = 0;
            var combines = new List<CombineInstance>();
            combines.AddRange(CombineInstances);
            CombineInstances.Clear();
            
            foreach (var it in combines)
            {
                // 源网格数据
                var oldMeshDataPath = AssetDatabase.GetAssetPath(it.mesh);
                if (string.IsNullOrEmpty(oldMeshDataPath) || !File.Exists(oldMeshDataPath))
                {
                    Error("BackUpMeshDatas():This mesh data is not exist in disk!(mash name:{0})", it.mesh.name);
                    continue;
                }
                // 备份网格数据路径
                var backupMeshDataPath = $"{iBackUpDir}/{it.mesh.name}_backup.asset";
                // 备份网格数据
                if (!AssetDatabase.CopyAsset(oldMeshDataPath, backupMeshDataPath))
                {
                    Error("BackUpMeshDatas():AssetDatabase.CopyAsset Failed!\n(path:{0} backup path:{1})", 
                        oldMeshDataPath, backupMeshDataPath);
                    continue;
                }
                
                // 重新设定新的网格数据对象
                var backupMesh = AssetDatabase.LoadAssetAtPath(backupMeshDataPath, typeof(Mesh)) as Mesh;
                if (null == backupMesh)
                {
                    Error($"BackUpMeshDatas():LoadAssetAtPath backup mesh data failed!" +
                          $"(name:{0}) is invalid!(path={1})", it.mesh.name, backupMeshDataPath);
                    continue;
                }

                // 替换备份网格
                CombineInstances.Add(new CombineInstance()
                {
                    mesh = backupMesh,
                    transform = it.transform
                });

                iBackupUpdate?.Invoke(idx, CombineInstances.Count, $"{it.mesh.name} -> {backupMesh.name}");
                ++idx;
            }
        }
        
        /// <summary>
        /// 重置网格UV坐标
        /// </summary>
        /// <param name="iRect">纹理范围</param>
        /// <param name="iResetUpdate">重置更新回调函数</param>
        public void ResetMeshUvs(Rect iRect,
            Action<int, int, bool, string> iResetUpdate = null)
        {
            var idx = 0;
            foreach (var it in CombineInstances)
            {
                var mesh = it.mesh;
                var uvs = new Vector2[mesh.uv.Length];
                
                // 把网格的uv根据贴图的rect刷一遍
                for (var i = 0; i < uvs.Length; ++i)
                {
                    uvs[i].x = iRect.x + mesh.uv[i].x * iRect.width;
                    uvs[i].y = iRect.y + mesh.uv[i].y * iRect.height;
                    iResetUpdate?.Invoke(idx, CombineInstances.Count, false, $"{it.mesh.name}-{i}/{uvs.Length}");
                }
                mesh.uv = uvs;

                iResetUpdate?.Invoke(idx, CombineInstances.Count, true, $"{it.mesh.name}");
                ++idx;
            }
        }
        
    }
    
    internal class CombineConfig : ClassExtension
    {
        
        /// <summary>
        /// 信息列表
        /// key : 材质名
        /// value : 网格合并用Item信息
        /// </summary>
        public Dictionary<string, CombineItem> Items = new Dictionary<string, CombineItem>();
        
        /// <summary>
        /// 取得网格合并用实例列表
        /// </summary>
        /// <returns>网格合并用实例列表</returns>
        public List<CombineInstance> CombineInstances()
        {
            var instances = new List<CombineInstance>();

            foreach (var item in Items
                .Where(item => null != item.Value.CombineInstances)
                .Where(item => 0 < item.Value.CombineInstances.Count))
            {
                instances.AddRange(item.Value.CombineInstances);
            }
            
            return instances;
        }

        /// <summary>
        /// 检测校验材质的纹理
        /// </summary>
        /// <param name="iShaderTexProperities">着色器纹理属性列表</param>
        /// <param name="iDetailMsg">详细信息(仅仅验证失败时有内容)</param>
        /// <returns>true:OK; false:NG;</returns>
        public bool CheckTextureOfMat(
            string[] iShaderTexProperities, ref string iDetailMsg)
        {
            // 尺寸检测
            var flg = true;
            // 遍历所有材质
            foreach (var item in Items)
            {
                var lastSize = Vector2.zero;
                var lastProperty = "";
                var lastTexure = "";
                foreach (var property in iShaderTexProperities)
                {
                    // 添加纹理信息到列表
                    var sourceTex = item.Value.GetTextureByShaderProperty(property);
                    if(null == sourceTex) continue;
                    var texSize = new Vector2(sourceTex.width, sourceTex.height);
                    if (Vector2.zero == lastSize)
                    {
                        lastSize = texSize;
                        lastProperty = property;
                        lastTexure = sourceTex.name;
                    }

                    if (lastSize != texSize)
                    {
                        flg = false;
                    }
                    if (flg) continue;
                    iDetailMsg = $"材质纹理尺寸不一致!\n Mat:{item.Key} " +
                                 $"\n Properties:{lastProperty}/{property}" +
                                 $"\n Texture:{lastTexure}(w:{lastSize.x} H:{lastSize.y}) / {sourceTex.name}(w:{texSize.x} H:{texSize.y})";
                    break;
                }
                if (!flg) break;
            }
            return flg;
        }
        
        /// <summary>
        /// 打包纹理
        /// </summary>
        /// <param name="iShaderPropertyName">不存在指定着色器属性的纹理</param>
        /// <param name="iIsNormalMap">法线贴图标志位</param>
        /// <param name="iPadding">图集中纹理间距</param>
        /// <param name="iMaximumAtlasSize">图集最大尺寸</param>
        /// <param name="iTextureReduce">单个纹理缩小倍数</param>
        /// <param name="iMat">目标材质</param>
        /// <param name="ioIndexs">索引信息(key:源GameObject的InstanceID, value:对应纹理在图集中的索引)</param>
        /// <param name="iSaveDir">保存目录</param>
        /// <returns>打包后各个纹理范围信息</returns>
        public Rect[] PackTextures(
            string iShaderPropertyName, bool iIsNormalMap, int iPadding, 
            int iMaximumAtlasSize, int iTextureReduce, Material iMat,
            out Dictionary<int, int> ioIndexs, string iSaveDir = null)
        {
            // 初始化纹理信息
            ioIndexs = new Dictionary<int, int>();
            // 纹理列表
            var texs = new List<Texture2D>();
            // 遍历
            foreach (var item in Items)
            {
                // 添加纹理信息到列表
                var sourceTex = item.Value.GetTextureByShaderProperty(iShaderPropertyName);
                if(null == sourceTex) continue;
                
                // 缩小单个纹理
                var width = sourceTex.width / iTextureReduce;
                var height = sourceTex.height / iTextureReduce;
                var pixels = ModelEditor.ConvertTexture(sourceTex, width, height);
                if (null == pixels || 0 >= pixels.Length)
                {
                    continue;
                }
                // 目标纹理
                var destTex = new Texture2D(
                    sourceTex.width / iTextureReduce, sourceTex.height / iTextureReduce, 
                    iIsNormalMap ? TextureFormat.RGB24 : TextureFormat.RGBA32, false)
                {
                    name = sourceTex.name
                };
                destTex.SetPixels(pixels);
                destTex.Apply();
                
                texs.Add(destTex);
                // 保存索引信息
                ioIndexs.Add(item.Value.InstanceID, ioIndexs.Count);
            }
            
            // 若无纹理
            if (0 >= texs.Count) return null;
            
            var texAtlas = new Texture2D(
                iMaximumAtlasSize, iMaximumAtlasSize, 
                iIsNormalMap ? TextureFormat.RGB24 : TextureFormat.RGBA32, true)
            {
                name = $"{iShaderPropertyName}_atlas"
            };
            if (null == texAtlas)
            {
                Error("PackTextures():The target texture is invalid or null!!");
                return null;
            }
            var rects = texAtlas.PackTextures(texs.ToArray(), iPadding, iMaximumAtlasSize);

            // 保存
            if (string.IsNullOrEmpty(iSaveDir)) return rects;
            var savePath = ModelEditor.SaveTexture(texAtlas, iSaveDir);
            if (string.IsNullOrEmpty(savePath))
            {
                Error("PackTextures():Texture Save Failed!!(SaveDir:{0} Property:{1})", 
                    iSaveDir, iShaderPropertyName);
            }
            else
            {
                // 释放临时存储的
                Object.DestroyImmediate(texAtlas);
                // 重新加载之前保存的
                texAtlas = AssetDatabase.LoadAssetAtPath(savePath, typeof(Texture2D)) as Texture2D;
                // 若是法线贴图，则设置纹理类型
                ModelEditor.SetTextureImportSettins(texAtlas, TextureImporterFormat.RGBA32, iIsNormalMap);
                // 重新设置纹理
                iMat.SetTexture(iShaderPropertyName, texAtlas);
            }
            return rects;
        }
    }
    
    /// <summary>
    /// 模型物理统计信息
    /// </summary>
    [Serializable]
    internal class PhysicalStatistics : JsonDataBase<GPUStatistics>
    {
        public int Tris;
        public int Verts;
        public int Mats;
        public int Shaders;
        public int Textures;

        /// <summary>
        /// 清空
        /// </summary>
        public override void Clear()
        {
            Tris = 0;
            Verts = 0;
            Mats = 0;
            Shaders = 0;
            Textures = 0;
        }
    }
    
    /// <summary>
    /// 模型加载GPU消耗统计信息
    ///  注意：GPU渲染后的统计信息
    /// </summary>
    [Serializable]
    internal class GPUStatistics : JsonDataBase<GPUStatistics>
    {
        public int Batches;
        public int SaveBatching;
        public int Tris;
        public int Verts;
        public int SetPassCalls;
        public int ShadowCasters;
        public int VisibleSkinnedMeshed;
        public int VisibleAnimations;

        /// <summary>
        /// 自动加载
        /// </summary>
        public void AutoLoad()
        {
            Batches = UnityStats.batches;
            SaveBatching = (UnityStats.dynamicBatchedDrawCalls - UnityStats.dynamicBatches) +
                                                            (UnityStats.staticBatchedDrawCalls - UnityStats.staticBatches);
            Tris = UnityStats.triangles;
            Verts = UnityStats.vertices;
            SetPassCalls = UnityStats.setPassCalls;
            ShadowCasters = UnityStats.shadowCasters;
            VisibleSkinnedMeshed = UnityStats.visibleSkinnedMeshes;
            VisibleAnimations = UnityStats.visibleAnimations;
        }
        
        /// <summary>
        /// 运算符重载
        /// </summary>
        /// <param name="iA">被减数</param>
        /// <param name="iB">减数</param>
        /// <returns>减法结果</returns>
        public static GPUStatistics operator -(GPUStatistics iA, GPUStatistics iB)
        {
            return new GPUStatistics()
            {
                Batches = iA.Batches - iB.Batches,
                SaveBatching = iA.SaveBatching - iB.SaveBatching,
                Tris = iA.Tris - iB.Tris,
                Verts = iA.Verts - iB.Verts,
                SetPassCalls = iA.SetPassCalls - iB.SetPassCalls,
                ShadowCasters = iA.ShadowCasters - iB.ShadowCasters,
                VisibleSkinnedMeshed = iA.VisibleSkinnedMeshed - iB.VisibleSkinnedMeshed,
                VisibleAnimations = iA.VisibleAnimations - iB.VisibleAnimations,
                //MatCount = iA.MatCount - iB.MatCount,
                //TexCount = iA.TexCount - iB.TexCount,
                //ShaderCount = iA.ShaderCount - iB.ShaderCount
            }; 
        }
    }

    /// <summary>
    /// 纹理导入设定
    /// </summary>
    [Serializable]
    internal class TextureImporterSetting : JsonDataBase<TextureImporterSetting>
    {
        /// <summary>
        /// 导入类型
        /// </summary>
        public TextureImporterType Type;

        /// <summary>
        /// 平台设定信息
        /// </summary>
        public TextureImporterPlatformSettings Platform;
    }

    /// <summary>
    /// 模型设定信息
    /// </summary>
    [Serializable]
    internal class ModelMeshSettingsData : JsonDataBase<ModelMeshSettingsData>
    {
        /// <summary>
        /// Tab索引
        /// </summary>
        public int TabIndex;

        /// <summary>
        /// 当前类型
        /// </summary>
        public TextureImporterType CurType;
        
        /// <summary>
        /// 纹理设定信息
        /// </summary>
        public List<TextureImporterSetting> Settings = new List<TextureImporterSetting>();
        
        /// <summary>
        /// 取得导入设定信息
        /// </summary>
        /// <param name="iType">导入类型</param>
        /// <returns>导入设定信息</returns>
        public TextureImporterSetting GetImporterSettingByType(TextureImporterType iType)
        {
            return !Settings.Exists(iO => iType == iO.Type) ? 
                null : Settings.Where(iO => iType == iO.Type).ToArray()[0];
        }
        
        /// <summary>
        /// 取得导入设定信息
        /// </summary>
        /// <param name="iType">导入类型</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iAutoCreated">自动创建标识位</param>
        /// <returns>导入设定信息</returns>
        public TextureImporterSetting GetImporterSettingByType(
            TextureImporterType iType, int iTabIndex, bool iAutoCreated = false)
        {
            if (!Settings.Exists(iO => iType == iO.Type))
            {
                if (!iAutoCreated) return null;
                Settings.Add(new TextureImporterSetting
                {
                    Type = iType,
                    Platform = new TextureImporterPlatformSettings
                    {
                        name = ModelEditor.GetBuildTargetNameByTabIndex(iTabIndex)
                    }
                });
                // 重新排序
                Settings.Sort((iX, iY) => iX.Type >= iY.Type ? 1 : 0);
            }
            return Settings.Where(iO => iType == iO.Type).ToArray()[0];
        }
    }
    
    /// <summary>
    /// 模型网格编辑器数据
    /// </summary>
    [Serializable]
    internal class ModelMeshEditorData : JsonDataBase<ModelMeshEditorData>
    {
        
        /// <summary>
        /// 当前平台Tab索引
        /// </summary>
        public int CurTabIndex;
        
        /// <summary>
        /// 导出目录
        /// </summary>
        public string ExportDir;
        
        /// <summary>
        /// 着色器列表(移动设备)
        /// </summary>
        public List<string> MobileShaders = new List<string>();
        
        /// <summary>
        /// 配置数据
        /// </summary>
        public List<EditorData> Config = new List<EditorData>();
        
        /// <summary>
        /// 模型信息一览
        /// </summary>
        public List<ModelInfo> Models = new List<ModelInfo>();

        /// <summary>
        /// 设定信息
        /// </summary>
        public List<ModelMeshSettingsData> Settings = new List<ModelMeshSettingsData>();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public ModelMeshEditorData()
        {
            // 移动设备着色器列表
            MobileShaders?.Clear();
        }

        /// <summary>
        /// 初始化移动平台着色器列表
        /// </summary>
        public void InitMobileShaders()
        {
            if(0 < MobileShaders.Count) return;
            MobileShaders.Add("Mobile/Diffuse");
            MobileShaders.Add("Mobile/Bumped Diffuse");
            MobileShaders.Add("Mobile/Bumped Specular");
            
            // 排序
            MobileShaders.Sort((iX, iY) => string.Compare(iX, iY, StringComparison.Ordinal));
        }
        
        /// <summary>
        /// 取得模型信息
        /// </summary>
        /// <param name="iId">ID</param>
        /// <param name="iAutoCreated">自动创建</param>
        /// <returns>模型信息</returns>
        public ModelInfo GetModelInfoById(int iId, bool iAutoCreated = true)
        {
            if (Models.Exists(iO => iId == iO.ID))
            {
                return Models.Where(iO => iId == iO.ID).ToArray()[0];  
            }
                
            if (!iAutoCreated) return null;
            var model = new ModelInfo { ID = iId };
            Models.Add(model);
                
            // 重新排序
            Models.Sort((iX, iY) => iX.ID >= iY.ID ? 1 : 0);
            return model;
        }
        
        /// <summary>
        /// 追加模型信息
        /// </summary>
        /// <param name="iPrefabPath">预制体路径</param>
        /// <returns>true : 追加成功; false : 已存在;</returns>
        public bool AddModelByPath(string iPrefabPath)
        {
            if(Models.Exists(iO => iO.Path.Equals(iPrefabPath)))
            {
                Warning("AddModelByPath():This info is already exist!!!(Path:{0})", iPrefabPath);
                return false;
            }

            var model = new ModelInfo
            {
                ID = GenerateModelId(),
                Path = iPrefabPath
            };
            Models.Add(model);
            
            // 生成Icon
            var icon = ModelEditor.GeneratePreviewIcon(iPrefabPath);
            if (null == icon) return false;
            model.PreviewIcon = icon;
            return true;
        }

        /// <summary>
        /// 移除模型信息
        /// </summary>
        /// <param name="iId">ID</param>
        /// <returns>true:移除成功; false:移除失败或者未移除;</returns>
        public bool RemoveModelById(int iId)
        {
            if(!Models.Exists(iO => iId == iO.ID)) return false;
            var model = Models.Where(iO => iId == iO.ID).ToArray()[0];
            if(null == model) return false;
            Models.Remove(model);
            return true;
        }
        
        /// <summary>
        /// 移除模型信息
        /// </summary>
        /// <param name="iId">ID</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>true:移除成功; false:移除失败或者未移除;</returns>
        public bool RemoveModelByIdAndTabIndex(int iId, int iTabIndex)
        {
            if (ModelEditor.isDefaultTab(iTabIndex)) return false;
            if(!Models.Exists(iO => iId == iO.ID)) return false;
            var model = Models.Where(iO => iId == iO.ID).ToArray()[0];
            var simplify = model?.GetSimplifyInfoByTabIndex(iTabIndex);
            if(simplify == null) return false;
            model.Simplifies.Remove(simplify);
            return true;
        }
        
        /// <summary>
        /// 生成模型ID
        /// </summary>
        /// <returns>模型ID</returns>
        private int GenerateModelId()
        {
            if (0 >= Models.Count) return 1;
            return Models.Max(iO => iO.ID) + 1;
        }
        
        /// <summary>
        /// 取得表示用的目标列表
        /// </summary>
        /// <param name="iSearchKey">检索Key</param>
        /// <returns>表示用的目标列表</returns>
        public List<ModelInfo> GetDisplayModels(string iSearchKey)
        {
            if (string.IsNullOrEmpty(iSearchKey)) return Models;
            return Models
                .Where(iO => iO.Path.Contains(iSearchKey))
                .ToList();
        }

        /// <summary>
        /// 取得先试用的目标列表
        /// </summary>
        /// <param name="iSearchKey">检索Key</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>表示用的目标列表</returns>
        public List<ModelInfo> GetDisplayModelsByTabIndex(string iSearchKey, int iTabIndex)
        {
            if (!ModelEditor.isPlatformTab(iTabIndex)) return null;
            if (string.IsNullOrEmpty(iSearchKey))
            {
                return Models
                    .Where(iO => 
                        iO.Simplifies.Exists(iS => 
                            iTabIndex == iS.TabIndex))
                    .ToList();
            }
            return Models
                .Where(iO => 
                    iO.Path.Contains(iSearchKey) && 
                    iO.Simplifies.Exists(iS => 
                        iTabIndex == iS.TabIndex))
                .ToList();
        } 
        
        /// <summary>
        /// 判断是否有必要重载
        /// </summary>
        /// <param name="iSearchKey">模糊查询Key</param>
        /// <returns>true : 需要重载;false : 不需要重载;</returns>
        public bool IsReloadNecessary(string iSearchKey)
        {
            var ret = false;
            var models = GetDisplayModels(iSearchKey);
            if (null == models || 0 >= models.Count) return false;
            foreach (var target in models)
            {
                if (null != target.PreviewIcon) continue;
                ret = true;
                break;
            }
            return ret;
        }
        
        /// <summary>
        /// 重载预制体，并刷新预览Icon纹理
        /// </summary>
        /// <param name="iSearchKey">检索Key</param>
        /// <param name="iShowProgressBar">显示进度条回调函数</param>
        /// <param name="iReloadFailed">重载更新失败</param>
        public void ReloadPrefabs(
            string iSearchKey,
            Func<int, ProgressCounter> iShowProgressBar = null, 
            Action<string> iReloadFailed = null)
        {
            var models = GetDisplayModels(iSearchKey);
            var progressCounter = iShowProgressBar?.Invoke(models.Count);
            if(null == progressCounter) return;
            if(progressCounter.isCountOver()) return;
            // 开始计数
            progressCounter.StartCounter();
            
            var reloadCnt = 0;
            foreach (var model in models)
            {
                ++reloadCnt; 
                if (null != model.PreviewIcon)
                {
                    var msgDetail = $" ID:{model.ID}:{model.Path})";
                    progressCounter.UpdateByStep(1, msgDetail);
                    continue; 
                }
                
                // 释放原有的Icon
                DestroyPreviewIcon(model);
                
                // 生成Icon
                var icon = ModelEditor.GeneratePreviewIcon(model.Path);
                if (null != icon)
                {
                    model.PreviewIcon = icon;
                    // 更新进度信息
                    var msgDetail = $" ID:{model.ID}:{model.Path})";
                    progressCounter.UpdateByStep(1, msgDetail);
                    continue;
                }
                
                // 重载失败
                var msg = $"ReloadPrefabs():Invalid GUID (null or Empty)\n{reloadCnt}/{models.Count} : {model}!!!";
                iReloadFailed?.Invoke(msg);
                Error(msg);
            }
        }
        
        /// <summary>
        /// 释放预览Icon
        /// </summary>
        /// <param name="iInfo">模型信息</param>
        private void DestroyPreviewIcon (ModelInfo iInfo)
        {
            if (iInfo == null || iInfo.PreviewIcon == null) return;
            Object.DestroyImmediate(iInfo.PreviewIcon);
            iInfo.PreviewIcon = null;
        }

        /// <summary>
        /// 取得配置信息
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iAutoCreated">自动创建</param>
        /// <returns></returns>
        public EditorData GetConfigByTabIndex(
            int iTabIndex, bool iAutoCreated = true)
        {
            if (!Config.Exists(iO => iTabIndex == iO.TabIndex))
            {
                if (!iAutoCreated) return null;
                var config = new EditorData
                {
                    TabIndex =  iTabIndex,
                    ScrollOffset = Vector2.zero
                };
                Config.Add(config);
                
                // 排序
                Config.Sort((iX, iY) => iX.TabIndex >= iY.TabIndex ? 1 : 0);
                return config;
            }
            return Config.Where(iO => iTabIndex == iO.TabIndex).ToArray()[0];
        }

        /// <summary>
        /// 取得简化降面用着色器名在着色器列表中的索引值
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iModelId">Model ID</param>
        /// <returns>简化降面用着色器名在着色器列表中的索引值</returns>
        public int GetSimplifyShaderIndexByTabIndex(
            int iTabIndex, int iModelId)
        {
            // 初始化移动平台着色器列表
            InitMobileShaders();

            // 取得降面简化信息
            var model = GetModelInfoById(iModelId, false);
            var simplify = model?.GetSimplifyInfoByTabIndex(iTabIndex, false);
            if (simplify == null) return 0;
            return string.IsNullOrEmpty(simplify.Shader) ? 0 : MobileShaders.IndexOf(simplify.Shader);
        }
        
        /// <summary>
        /// 设定简化降面用着色器名
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iModelId">Model ID</param>
        /// <param name="iShaderIndex">Shader 索引</param>
        public void SetSimplifyShaderIndexByTabIndex(
            int iTabIndex, int iModelId, int iShaderIndex)
        {
            // 取得shader名
            if(0> iShaderIndex || iShaderIndex >= MobileShaders.Count) return;
            var shader = MobileShaders[iShaderIndex];
            if(string.IsNullOrEmpty(shader)) return;
            
            // 取得降面简化信息
            var model = GetModelInfoById(iModelId);
            var simplify = model?.GetSimplifyInfoByTabIndex(iTabIndex, true);
            if (simplify == null) return;
            simplify.Shader = shader;
        }

        /// <summary>
        /// 增加降面用的着色器
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iModelId">模型ID</param>
        /// <param name="iShaderName">着色器名</param>
        public void AddSimplifyShader(
            int iTabIndex, int iModelId, string iShaderName)
        {
            if (!MobileShaders.Exists(iShaderName.Equals))
            {
                MobileShaders.Add(iShaderName);
                // 排序
                MobileShaders.Sort((iX, iY) => string.Compare(iX, iY, StringComparison.Ordinal));
            }
            
            // 取得索引
            var idx = MobileShaders.IndexOf(iShaderName);
            
            // 设定着色器
            SetSimplifyShaderIndexByTabIndex(iTabIndex, iModelId, idx);
        }
        
        /// <summary>
        /// 删除降面用的着色器
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iModelId">模型ID</param>
        /// <param name="iShaderName">着色器名</param>
        public void DelSimplifyShader(
            int iTabIndex, int iModelId, string iShaderName)
        {
            // 取得删除后的默认Shader
            var shaderName = 0 >= MobileShaders.Count ? "" : MobileShaders[0];
            
            // 设定着色器
            var model = GetModelInfoById(iModelId, false);
            if(null == model) return;
            var simplify = model.GetSimplifyInfoByTabIndex(iTabIndex);
            simplify.Shader = shaderName;

            var isExist = false;
            foreach (var itM in Models
                .TakeWhile(iO => !isExist)
                .Where(iX => iX.Simplifies
                    .Any(iY => !ModelEditor.isPCTab(iY.TabIndex) && iShaderName.Equals(iY.Shader))))
            {
                isExist = true;
            }
            if (!isExist && MobileShaders.Exists(iShaderName.Equals))
            {
                MobileShaders.Remove(iShaderName);
            }
        }
        
        /// <summary>
        /// 清空
        /// </summary>
        public override void Clear()
        {
            CurTabIndex = 0;
            
            // 配置信息
            Config?.Clear();
            // 移动设备着色器列表
            MobileShaders?.Clear();
            // 模型信息
            Models?.Clear();
            // 设定信息
            Settings?.Clear();
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        public void ClearByTabIndex(int iTabIndex)
        {
            if (ModelEditor.isDefaultTab(iTabIndex))
            {
                Clear();
                return;
            }

            // 移除相关Tab中的信息
            foreach (var model in Models)
            {
                if(!model.Simplifies.Exists(iO => iTabIndex == iO.TabIndex)) continue;
                var removes = model.Simplifies.Where(iO => iTabIndex == iO.TabIndex).ToArray();
                if(0 >= removes.Length) continue;
                foreach (var remove in removes)
                {
                    if(null == remove) continue;
                    remove.Clear();
                    model.Simplifies.Remove(remove);
                }
                
            }
        }

#region ExportDir - Path

        /// <summary>
        /// 取得导出目录(显示用)
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>导出目录(显示用)</returns>
        public string GetDisplayExportDirByTabIndex(int iTabIndex)
        {
            if (string.IsNullOrEmpty(ExportDir))
            {
                ExportDir = UtilsTools.CheckMatchPath($"{Application.dataPath}/Resources");
            }
            return $"{ExportDir}/{ModelEditor.Tabs[iTabIndex]}/Prefabs";
        }
        
        /// <summary>
        /// 取得导出目录(显示用)
        /// </summary>
        /// <param name="iTab">Tab</param>
        /// <returns>导出目录(显示用)</returns>
        public string GetDisplayExportDir(string iTab)
        {
            if (string.IsNullOrEmpty(ExportDir))
            {
                ExportDir = UtilsTools.CheckMatchPath($"{Application.dataPath}/Resources/{iTab}");
            }
            return $"{ExportDir}/{iTab}/Prefabs";
        }
        
        /// <summary>
        /// 取得导出目录
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iPrefabPath">预制体路径</param>
        /// <returns>输出目录</returns>
        public string GetExportDirByTabIndex(int iTabIndex, string iPrefabPath)
        {
            if (string.IsNullOrEmpty(iPrefabPath)) return null;
            var exportDirTmp = GetDisplayExportDirByTabIndex(iTabIndex);
            return string.IsNullOrEmpty(exportDirTmp) ? 
                null : $"{exportDirTmp}/{UtilsTools.GetFileName(iPrefabPath, false)}";
        }
        
        /// <summary>
        /// 取得导出目录
        /// </summary>
        /// <param name="iTab">Tab</param>
        /// <param name="iPrefabPath">预制体路径</param>
        /// <returns>输出目录</returns>
        public string GetExportDirByTab(string iTab, string iPrefabPath)
        {
            if (string.IsNullOrEmpty(iPrefabPath)) return null;
            var exportDirTmp = GetDisplayExportDir(iTab);
            return string.IsNullOrEmpty(exportDirTmp) ? 
                null : $"{exportDirTmp}/{UtilsTools.GetFileName(iPrefabPath, false)}";
        }

        /// <summary>
        /// 取得默认预制体的路径
        /// </summary>
        /// <param name="iPrefabPath">预制体路径</param>
        /// <returns>默认预制体的路径</returns>
        public string GetDefaultPrefabPath(string iPrefabPath)
        {
            var dir = GetExportDirByTab(ModelEditor._TAB_DEFAULT, iPrefabPath);
            return !UtilsTools.CheckAndCreateDirByFullDir(dir) ? null : $"{dir}/{UtilsTools.GetFileName(iPrefabPath)}";
        }
        
#endregion

        /// <summary>
        /// 取得设定信息
        /// </summary>
        /// <param name="iTabIndex"></param>
        /// <param name="iAutoCreated"></param>
        /// <returns></returns>
        public ModelMeshSettingsData GetSettingByTabIndex(int iTabIndex, bool iAutoCreated = false)
        {
            if (Settings.Exists(iO => iTabIndex == iO.TabIndex))
                return Settings.Where(iO => iTabIndex == iO.TabIndex).ToArray()[0];
            if (!iAutoCreated) return null;
            var setting = new ModelMeshSettingsData
            {
                TabIndex =  iTabIndex,
                CurType = TextureImporterType.Default
            };
            Settings.Add(setting);
            // 重新排序
            Settings.Sort((iX, iY) => (iX.TabIndex >= iY.TabIndex) ? 1 : 0);
            // 返回
            return Settings.Where(iO => iTabIndex == iO.TabIndex).ToArray()[0];
        }

        /// <summary>
        /// 取得纹理设定信息
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iType">类型</param>
        /// <param name="iAutoCreated">自动创建标识位</param>
        /// <returns>设定信息</returns>
        public TextureImporterSetting GetTextureImporterSetting(
            int iTabIndex, TextureImporterType iType, bool iAutoCreated = false)
        {
            var settings = GetSettingByTabIndex(iTabIndex, iAutoCreated);
            // 返回
            return settings?.GetImporterSettingByType(iType, iTabIndex, iAutoCreated);
        }
    }

    /// <summary>
    /// 降面信息
    /// </summary>
    [Serializable]
    internal class SimplifyInfo : JsonDataBase<ModelInfo>
    {
        /// <summary>
        /// 选中标志位置
        /// </summary>
        public bool Checked;

        /// <summary>
        /// 降面率
        /// </summary>
        public float SimplifyRatio;
        
        /// <summary>
        /// Tab索引
        /// </summary>
        public int TabIndex;

        /// <summary>
        /// 着色器
        /// </summary>
        public string Shader;

        /// <summary>
        /// 图集最大尺寸
        /// </summary>
        public int AtlasMaxSize;

        /// <summary>
        /// 纹理缩小尺寸
        /// </summary>
        public int TextureReduce;
        
        /// <summary>
        /// 花费时间(单位:100毫微秒)
        /// </summary>
        public long CostTime;
        
        /// <summary>
        /// 模型加载GPU消耗统计信息
        /// </summary>
        public GPUStatistics GPU = new GPUStatistics();
        
        /// <summary>
        /// 模型物理统计信息
        /// </summary>
        public PhysicalStatistics Physical = new PhysicalStatistics();
        
    }

    /// <summary>
    /// 编辑器数据
    /// </summary>
    [Serializable]
    internal class EditorData : JsonDataBase<EditorData>
    {
        /// <summary>
        /// Tab索引
        /// </summary>
        public int TabIndex;
        /// <summary>
        /// 滚动列表滚动偏移.
        /// </summary>
        public Vector2 ScrollOffset;
    }

    /// <summary>
    /// 模型信息
    /// </summary>
    [Serializable]
    internal class ModelInfo : JsonDataBase<ModelInfo>
    {
        /// <summary>
        /// 选中标志位置
        /// </summary>
        public bool Checked;
        /// <summary>
        /// ID
        /// </summary>
        public int ID;

        /// <summary>
        /// 网格合并模式
        /// </summary>
        public CombineMode CombineMode;
        
        /// <summary>
        /// 路径
        /// </summary>
        public string Path;

        /// <summary>
        /// 模型加载GPU消耗统计信息
        /// </summary>
        public GPUStatistics GPU = new GPUStatistics();
        
        /// <summary>
        /// 模型物理统计信息
        /// </summary>
        public PhysicalStatistics Physical = new PhysicalStatistics();
        
        /// <summary>
        /// 降面信息列表
        /// </summary>
        public List<SimplifyInfo> Simplifies = new List<SimplifyInfo>();
        
        /// <summary>
        /// 合并网格花费时间(单位:100毫微秒)
        /// </summary>
        public long CombineMeshCostTime;
        
        /// <summary>
        /// 总花费时间(单位:100毫微秒)
        /// </summary>
        public long TotalCostTime;
        
        /// <summary>
        /// 预览Icon
        /// </summary>
        [NonSerialized]
        public Texture PreviewIcon;
        
        /// <summary>
        /// 数据采集中标志位
        /// </summary>
        [NonSerialized]
        public bool DataSampling;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public ModelInfo()
        {
            var start = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_PC);
            var end = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_IOS);
            for (var value = start; value <= end; ++value)
            {
                var config = GetSimplifyInfoByTabIndex(value, true);
                if(null == config) 
                    Error("ModelSimplifies():Simplify Config Created Failed!!(Value:{0})", value);
            }
        }
        
        /// <summary>
        /// 取得降面信息
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iAutoCreated">是否自动创建</param>
        /// <returns>降面信息</returns>
        public SimplifyInfo GetSimplifyInfoByTabIndex(
            int iTabIndex, bool iAutoCreated = false)
        {
            if (IsSimplifyInfoExistByTabIndex(iTabIndex))
                return Simplifies.Where(iO => iTabIndex == iO.TabIndex).ToArray()[0];
            if (!iAutoCreated) return null;
            var simplify = new SimplifyInfo()
            {
                TabIndex = iTabIndex,
                Checked = false,
                SimplifyRatio = 1.0f,
                CostTime = 0L
            };  
            Simplifies.Add(simplify);
            
            // 排序
            Simplifies.Sort((iX, iY) => iX.TabIndex >= iY.TabIndex ? 1 : 0);
            
            return simplify;
        }

        /// <summary>
        /// 判断降面信息是否存在
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>true:存在; false:不存在;</returns>
        public bool IsSimplifyInfoExistByTabIndex(int iTabIndex)
        {
            return Simplifies.Exists(iO => iTabIndex == iO.TabIndex);
        }

        /// <summary>
        /// 取得导出路径
        /// </summary>
        /// <param name="iExportRootDir">导出根目录</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>导出路径</returns>
        public string GetExportPath(string iExportRootDir, int iTabIndex)
        {
            if (string.IsNullOrEmpty(Path)) return null;
            if (string.IsNullOrEmpty(iExportRootDir)) return null;
            return $"{iExportRootDir}/{ModelEditor.Tabs[iTabIndex]}/Prefabs/" +
                   $"{UtilsTools.GetFileName(Path, false)}";
        }
    }
        
    /// <summary>
    /// 纹理导出格式列表
    /// </summary>
    internal class TextureImporterFormats 
        : EnumDisplayList<TextureImporterFormat>, IEnumDisplayList<TextureImporterFormat>
    {
        /// <summary>
        /// 取得显示用文本列表
        /// </summary>
        /// <returns>文本列表</returns>
        public List<string> GetDisplayTexts()
        {
            var formats = new List<string>();
                
            // // 排序
            // List.Sort((iX, iY) => 
            //     string.Compare(iX.Value.ToString(), iY.Value.ToString(), StringComparison.Ordinal));

            for (var idx = 0; idx < List.Count; ++idx)
            {
                formats.Add(List[idx].Text);
            }
                
            return formats;
        }

        /// <summary>
        /// 取得纹理导入格式索引
        /// </summary>
        /// <param name="iValue">纹理格式</param>
        /// <returns>纹理格式索引</returns>
        public int GetIndexByValue(TextureImporterFormat iValue)
        {
            if (!List.Exists(iO => iValue == iO.Value)) return 0;
            var target = List.Where(iO => iValue == iO.Value).ToArray()[0];
            return null == target ? 0 : List.IndexOf(target, 0);
        }

        /// <summary>
        /// 取得纹理导入格式
        /// </summary>
        /// <param name="iIndex">索引</param>
        /// <returns>纹理导入格式</returns>
        public TextureImporterFormat GetValueByIndex(int iIndex)
        {
            return iIndex >= List.Count ? TextureImporterFormat.Automatic : List[iIndex].Value;
        } 
    }

    /// <summary>
    /// 纹理压缩质量表示信息
    /// </summary>
    internal class TextureImporterCompressions
        : EnumDisplayList<TextureImporterCompression>, IEnumDisplayList<TextureImporterCompression>
    {
        /// <summary>
        /// 取得显示用文本列表
        /// </summary>
        /// <returns>文本列表</returns>
        public List<string> GetDisplayTexts()
        {
            var formats = new List<string>();
                
            // // 排序
            // List.Sort((iX, iY) => 
            //     string.Compare(iX.Value.ToString(), iY.Value.ToString(), StringComparison.Ordinal));

            for (var idx = 0; idx < List.Count; ++idx)
            {
                formats.Add(List[idx].Text);
            }
                
            return formats;
        }

        /// <summary>
        /// 取得指定纹理压缩质量的列表索引
        /// </summary>
        /// <param name="iValue">纹理格式</param>
        /// <returns>纹理压缩质量的列表索引</returns>
        public int GetIndexByValue(TextureImporterCompression iValue)
        {
            if (!List.Exists(iO => iValue == iO.Value)) return 0;
            var target = List.Where(iO => iValue == iO.Value).ToArray()[0];
            return null == target ? 0 : List.IndexOf(target, 0);
        }
        
        /// <summary>
        /// 取得指定索引的纹理压缩质量值
        /// </summary>
        /// <param name="iIndex">索引</param>
        /// <returns>纹理压缩质量值</returns>
        public TextureImporterCompression GetValueByIndex(int iIndex)
        {
            return iIndex >= List.Count ? TextureImporterCompression.Uncompressed : List[iIndex].Value;
        }
    }

    /// <summary>
    /// 纹理导出类型列表
    /// </summary>
    internal class TextureImporterTypes 
        : EnumDisplayList<TextureImporterType>, IEnumDisplayList<TextureImporterType>
    {
        /// <summary>
        /// 取得显示用文本列表
        /// </summary>
        /// <returns>文本列表</returns>
        public List<string> GetDisplayTexts()
        {
            var formats = new List<string>();
                
            // // 排序
            // List.Sort((iX, iY) => 
            //     string.Compare(iX.Value.ToString(), iY.Value.ToString(), StringComparison.Ordinal));

            for (var idx = 0; idx < List.Count; ++idx)
            {
                formats.Add(List[idx].Text);
            }
                
            return formats;
        }

        /// <summary>
        /// 取得指定纹理导入类型的列表索引
        /// </summary>
        /// <param name="iValue">纹理导入类型</param>
        /// <returns>纹理导入类型的列表索引</returns>
        public int GetIndexByValue(TextureImporterType iValue)
        {
            if (!List.Exists(iO => iValue == iO.Value)) return 0;
            var target = List.Where(iO => iValue == iO.Value).ToArray()[0];
            return null == target ? 0 : List.IndexOf(target, 0);
        }
        
        /// <summary>
        /// 取得指定索引的纹理导入类型
        /// </summary>
        /// <param name="iIndex">索引</param>
        /// <returns>纹理导入类型</returns>
        public TextureImporterType GetValueByIndex(int iIndex)
        {
            return iIndex >= List.Count ? TextureImporterType.Default : List[iIndex].Value;
        }
    }

    /// <summary>
    /// 不支持ETC2的Android设备上的ETC2纹理解压缩格式覆盖
    /// </summary>
    internal class AndroidETC2FallbackOverrides
        : EnumDisplayList<AndroidETC2FallbackOverride>, IEnumDisplayList<AndroidETC2FallbackOverride>
    {
        /// <summary>
        /// 取得显示用文本列表
        /// </summary>
        /// <returns>文本列表</returns>
        public List<string> GetDisplayTexts()
        {
            var formats = new List<string>();
                
            // // 排序
            // List.Sort((iX, iY) => 
            //     string.Compare(iX.Value.ToString(), iY.Value.ToString(), StringComparison.Ordinal));

            for (var idx = 0; idx < List.Count; ++idx)
            {
                formats.Add(List[idx].Text);
            }
                
            return formats;
        }

        /// <summary>
        /// 取得指定值的列表索引
        /// </summary>
        /// <param name="iValue">纹理格式</param>
        /// <returns>列表索引</returns>
        public int GetIndexByValue(AndroidETC2FallbackOverride iValue)
        {
            if (!List.Exists(iO => iValue == iO.Value)) return 0;
            var target = List.Where(iO => iValue == iO.Value).ToArray()[0];
            return null == target ? 0 : List.IndexOf(target, 0);
        }
        
        /// <summary>
        /// 取得指定索引的值
        /// </summary>
        /// <param name="iIndex">索引</param>
        /// <returns>值</returns>
        public AndroidETC2FallbackOverride GetValueByIndex(int iIndex)
        {
            return iIndex >= List.Count ? AndroidETC2FallbackOverride.UseBuildSettings : List[iIndex].Value;
        }  
    }
}
