using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Packages.Common.Base;
using Packages.Logs;
using Packages.Utils;
using Object = UnityEngine.Object;

namespace Packages.Common.Editor
{

    /// <summary>
    /// 下拉菜单数据
    /// </summary>
    [Serializable]
    public class GUIPopupItem : JsonDataBase<GUIPopupItem>
    {
        /// <summary>
        /// 值列表
        /// </summary>
        public int Value;
        
        /// <summary>
        /// 文本列表
        /// </summary>
        public string Text;
    }

    /// <summary>
    /// Slider 选择数据定义
    /// </summary>
    [Serializable]
    public class GUISliderToggleItem : JsonDataBase<GUISliderToggleItem>
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID;
        /// <summary>
        /// 选中
        /// </summary>
        public bool Checked;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title;
        /// <summary>
        /// 值列表
        /// </summary>
        public float Value;
    }

    /// <summary>
    /// 下拉列表数据
    /// </summary>
    [Serializable]
    public class GUIPopupList<T> : JsonDataBase<T>
        where T : GUIPopupItem, new()
    {
        /// <summary>
        /// 道具列表
        /// </summary>
        public List<T> Items = new List<T>();

        /// <summary>
        /// 取得文本列表
        /// </summary>
        /// <returns>文本列表</returns>
        public string[] GetTexts()
        {
            return Items
                .OrderBy(iO => iO.Value)
                .Select(iO => iO.Text)
                .ToArray();
        }

        /// <summary>
        /// 取得指定值的索引
        /// </summary>
        /// <param name="iValue">值</param>
        /// <returns>索引</returns>
        public int GetItemIndexByValue(int iValue)
        {
            var idx = 0;
            for (var loop = 0; loop < Items.Count; ++loop)
            {
                if(iValue != Items[loop].Value) continue;
                idx = loop;
                break;
            }
            return idx;
        }

        /// <summary>
        /// 取得指定索引的Item
        /// </summary>
        /// <param name="iIndex">索引</param>
        /// <returns>Item</returns>
        public T GetItemByIndex(int iIndex)
        {
            if (0 > iIndex && iIndex >= Items.Count) return null;
            return Items[iIndex];
        }
        
        /// <summary>
        /// 取得指定值的Item
        /// </summary>
        /// <param name="iValue">值</param>
        /// <returns>Item</returns>
        public T GetItemByValue(int iValue)
        {
            if (0 >= Items.Count) return null;
            return !Items.Exists(iO => iValue == iO.Value) 
                ? null : Items.Where(iO => iValue == iO.Value).ToArray()[0];
        }
    }
    
    /// <summary>
    /// UI编辑器助手
    /// </summary>
    public class GUIEditorHelper : ClassExtension
    {

        /// <summary>
        /// 克隆GUIStyle
        /// </summary>
        /// <param name="iCustomStyleName">欲克隆的自定义风格名</param>
        /// <param name="iFixedWidth">填充宽度</param>
        /// <param name="iFixedHeight">填充高度</param>
        /// <returns></returns>
        public static GUIStyle CloneStyle(
            string iCustomStyleName, 
            float iFixedWidth = -1, float iFixedHeight = -1)
        {
            var tempList = GUI.skin.customStyles
                .Where(iO => iCustomStyleName.Equals(iO.name))
                .ToArray();
            if (0 >= tempList.Length)
            {
                Loger.Error($"UIEditorHelper::CloneStyle():Clone Failed!!({iCustomStyleName} is not exist in GUI.skin.customStyles!)");
                return null;
            }
            var customStyle = new GUIStyle(tempList[0]);
            if (0 < iFixedWidth)
            {
                customStyle.fixedWidth = iFixedWidth;
            }
            if (0 < iFixedHeight)
            {
                customStyle.fixedHeight = iFixedHeight;
            }
            return customStyle;
        }
        
        // public static string GeneratePreviewTexture (Item item, bool isReCreate = true)
        // {
        //     if (item == null || item.prefab == null) return;
        //     {
        //         string preview_path = Configure.ResAssetsPath + "/Preview/" + item.prefab.name + ".png";
        //         if (!isReCreate && File.Exists(preview_path))
        //         {
        //             Texture texture = UIEditorHelper.LoadTextureInLocal(preview_path);
        //             item.tex = texture;
        //         }
        //         else
        //         {
        //             Texture Tex = UIEditorHelper.GetAssetPreview(item.prefab);
        //             if (Tex != null)
        //             {
        //                 DestroyTexture(item);
        //                 item.tex = Tex;
        //                 UIEditorHelper.SaveTextureToPNG(Tex, preview_path);
        //             }
        //         }
        //         item.dynamicTex = false;
        //         return;
        //     }
        // }

        /// <summary>
        /// 加载预览Icon纹理
        /// </summary>
        /// <param name="iPath">路径</param>
        /// <param name="iWidth">Icon纹理宽度</param>
        /// <param name="iHeight">Icon纹理高度</param>
        /// <returns>预览Icon纹理</returns>
        public static Texture2D LoadPreviewIconTexture(
            string iPath, int iWidth = -1, int iHeight = -1)
        {
            //创建Texture
            var width = 100;
            if (0 < iWidth)
            {
                width = iWidth;
            }
            var height = 100;
            if (0 < iHeight)
            {
                height = iHeight;
            }
            var texture = new Texture2D(width, height);

            //创建文件读取流
            using (var fs = new FileStream(iPath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(0, SeekOrigin.Begin);
                //创建文件长度缓冲区
                var bytes = new byte[fs.Length];
                //读取文件
                fs.Read(bytes, 0, (int)fs.Length);
                //释放文件读取流
                fs.Close(); 
                
                texture.LoadImage(bytes);
            }
            return texture;
        }
        
        /// <summary>
        /// 保存纹理
        /// </summary>
        /// <param name="iTexture">纹理</param>
        /// <param name="iSavePath">保存路径</param>
        /// <returns>true : 保存成功; false : 保存失败;</returns>
        public static bool SaveTexture(Texture iTexture, string iSavePath)
        {
            var temp = RenderTexture.GetTemporary(
                iTexture.width, iTexture.height, 
                0, RenderTextureFormat.ARGB32);
            Graphics.Blit(iTexture, temp);
            var ret = SaveTexture(temp, iSavePath);
            if (ret)
            {
                RenderTexture.ReleaseTemporary(temp); 
                // 刷新
                UtilsAsset.AssetsRefresh();
            }
            return ret;
        }
        
        /// <summary>
        /// 保存纹理
        /// </summary>
        /// <param name="iRenderTexture">渲染纹理</param>
        /// <param name="iSavePath">保存路径</param>
        /// <returns>true : 保存成功; false : 保存失败;</returns>
        private static bool SaveTexture(RenderTexture iRenderTexture, string iSavePath)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = iRenderTexture;
            var png = new Texture2D(iRenderTexture.width, iRenderTexture.height, TextureFormat.ARGB32, false);
            png.ReadPixels(new Rect(0, 0, iRenderTexture.width, iRenderTexture.height), 0, 0);
            var bytes = png.EncodeToPNG();
            var directory = Path.GetDirectoryName(iSavePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            using (var file = File.Open(iSavePath, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(file);
                writer.Write(bytes);
                file.Close(); 
            }
            Object.DestroyImmediate(png);
            RenderTexture.active = prev;
            return true;
        }
        
        /// <summary>
        /// 取得Asset的预览
        /// </summary>
        /// <param name="iObj">对象</param>
        /// <returns>预览纹理</returns>
        public static Texture GetAssetPreviewTexture(GameObject iObj)
        {
            GameObject canvasObj = null;
            var clone = Object.Instantiate(iObj);
            var cloneTransform = clone.transform;
            var isUiNode = false;
            if (cloneTransform is RectTransform)
            {
                // 如果是UGUI节点的话就要把它们放在Canvas下了
                canvasObj = new GameObject("render canvas", typeof(Canvas));
                var canvas = canvasObj.GetComponent<Canvas>();
                cloneTransform.SetParent(canvasObj.transform);
                cloneTransform.localPosition = Vector3.zero;

                canvasObj.transform.position = new Vector3(-1000, -1000, -1000);
                // 放在21层，摄像机也只渲染此层的，避免混入了奇怪的东西
                canvasObj.layer = 21;
                isUiNode = true;
            }
            else
                cloneTransform.position = new Vector3(-1000, -1000, -1000);

            var all = clone.GetComponentsInChildren<Transform>();
            foreach (var trans in all)
            {
                trans.gameObject.layer = 21;
            }

            var bounds = GetBounds(clone);
            var min = bounds.min;
            var max = bounds.max;
            var cameraObj = new GameObject("render camera");

            var renderCamera = cameraObj.AddComponent<Camera>();
            renderCamera.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            renderCamera.clearFlags = CameraClearFlags.Color;
            renderCamera.cameraType = CameraType.Preview;
            renderCamera.cullingMask = 1 << 21;
            if (isUiNode)
            {
                var position = cloneTransform.position;
                cameraObj.transform.position = new Vector3((max.x + min.x) / 2f, (max.y + min.y) / 2f, position.z-100);
                // +0.01f是为了去掉Unity自带的摄像机旋转角度为0的打印，太烦人了
                var center = new Vector3(position.x+0.01f, (max.y + min.y) / 2f, position.z);
                cameraObj.transform.LookAt(center);

                renderCamera.orthographic = true;
                var width = max.x - min.x;
                var height = max.y - min.y;
                var maxCameraSize = width > height ? width : height;
                // 预览图要尽量少点空白
                renderCamera.orthographicSize = maxCameraSize / 2;
            }
            else
            {
                cameraObj.transform.position = new Vector3((max.x + min.x) / 2f, (max.y + min.y) / 2f, max.z + (max.z - min.z));
                var position = cloneTransform.position;
                var center = new Vector3(position.x+0.01f, (max.y + min.y) / 2f, position.z);
                cameraObj.transform.LookAt(center);

                var angle = (int)(Mathf.Atan2((max.y - min.y) / 2, (max.z - min.z)) * 180 / 3.1415f * 2);
                renderCamera.fieldOfView = angle;
            }
            var texture = new RenderTexture(128, 128, 0, RenderTextureFormat.Default);
            renderCamera.targetTexture = texture;

            Undo.DestroyObjectImmediate(cameraObj);
            // TODO:不知道为什么要删掉再Undo回来后才Render得出来UI的节点，3D节点是没这个问题的，估计是Canvas创建后没那么快有效？
            Undo.PerformUndo();
            renderCamera.RenderDontRestore();
            var tex = new RenderTexture(128, 128, 0, RenderTextureFormat.Default);
            Graphics.Blit(texture, tex);

            Object.DestroyImmediate(canvasObj);
            Object.DestroyImmediate(cameraObj);
            Object.DestroyImmediate(clone);
            return tex;
        }

        /// <summary>
        /// 取得包围盒
        /// </summary>
        /// <param name="iObj">游戏对象</param>
        /// <returns>包围盒</returns>
        public static Bounds GetBounds(GameObject iObj)
        {
            var min = new Vector3(99999, 99999, 99999);
            var max = new Vector3(-99999, -99999, -99999);
            var renders = iObj.GetComponentsInChildren<MeshRenderer>();
            if (renders.Length > 0)
            {
                for (var i = 0; i < renders.Length; i++)
                {
                    if (renders[i].bounds.min.x < min.x)
                        min.x = renders[i].bounds.min.x;
                    if (renders[i].bounds.min.y < min.y)
                        min.y = renders[i].bounds.min.y;
                    if (renders[i].bounds.min.z < min.z)
                        min.z = renders[i].bounds.min.z;

                    if (renders[i].bounds.max.x > max.x)
                        max.x = renders[i].bounds.max.x;
                    if (renders[i].bounds.max.y > max.y)
                        max.y = renders[i].bounds.max.y;
                    if (renders[i].bounds.max.z > max.z)
                        max.z = renders[i].bounds.max.z;
                }
            }
            else
            {
                var rectTrans = iObj.GetComponentsInChildren<RectTransform>();
                var corner = new Vector3[4];
                for (var i = 0; i < rectTrans.Length; i++)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    rectTrans[i].GetWorldCorners(corner);
                    if (corner[0].x < min.x)
                        min.x = corner[0].x;
                    if (corner[0].y < min.y)
                        min.y = corner[0].y;
                    if (corner[0].z < min.z)
                        min.z = corner[0].z;

                    if (corner[2].x > max.x)
                        max.x = corner[2].x;
                    if (corner[2].y > max.y)
                        max.y = corner[2].y;
                    if (corner[2].z > max.z)
                        max.z = corner[2].z;
                }
            }

            var center = (min + max) / 2;
            var size = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
            return new Bounds(center, size);
        }
    }
}
