using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Packages.Common.Base;
using Packages.Common.Counter;
using Packages.Common.Editor;
using Packages.Common.Extend.Editor;
using Packages.Utils;
using UnityEditor;
using UnityEngine;

namespace Packages.Models.Editor
{
    
    /// <summary>
    /// 网格合并编辑器
    /// </summary>
    [Serializable]
    internal class ModelMeshEditor : WindowInspectorBase<ModelMeshEditor, ModelMeshEditorData>
    {
        /// <summary>
        /// Json文件导出路径
        /// </summary>
        private readonly string JsonFileDir = $"{ModelEditor.BaseDir}/Json";
        
        /// <summary>
        /// 提示文本颜色
        /// </summary>
        private static Color InfoTxtColor = Color.black;
        private static Color NoticeTxtColor = new Color(1.0f, 127.0f/255, 0.0f, 1.0f);

        /// <summary>
        /// 进度计数器
        /// </summary>
        private ProgressCounter _progressCounter = null;
        
        /// <summary>
        /// 检索Key
        /// </summary>
        private string _searchKey;
        
        /// <summary>
        /// 窗口类不要写构造函数，初始化写在Awake里
        /// </summary>
        void Awake()
        {
            _jsonPath = JsonFileDir;
            
            // 

            // TODO: 暂时打开时不加载
            // if (Init(JsonFileDir)) return;
            // this.Warning("Awake():ModelMeshEditor Failed Or not conf file!!!\n(file={0})",
            //     JsonFileDir);
        }
        
#region Implement

#region Windows - Size

        private Rect GroupRect = Rect.zero;
        private Rect ToolBarRect = Rect.zero;
        private Rect SearchBarRect = Rect.zero;
        private Rect MainViewRect = Rect.zero;
        private Rect BottomBarRect = Rect.zero;
        private const int TabsBarHeight = 30;
        private const int MainViewVBarWidth = 25;
        private const int defaultMainViewRowHeight = 135;
        private const int otherMainViewRowHeight = 140;
        private const int MainViewHBarHeight = 20;

        /// <summary>
        /// 窗体变化时，重置窗体尺寸相关信息
        /// </summary>
        protected override void ResetWindowsSizeWhenChanged()
        {
            // Group
            GroupRect.x = BasePadding;
            GroupRect.y = TabsBarHeight;
            GroupRect.width = Width - BasePadding;
            GroupRect.height = Height
                               - TabsBarHeight      // 顶部余白 + Tabs面板高度
                               - BasePadding        // 上余白
                               - BasePadding * 3;   // 下余白
            
            // ToolBar
            ToolBarRect.x = BasePadding;   // 左余白
            ToolBarRect.y = BasePadding;   // 上余白
            ToolBarRect.width = GroupRect.width;
            ToolBarRect.height = BaseCellUnitHeight;
            
            // BottomBar
            BottomBarRect.x = ToolBarRect.x; 
            BottomBarRect.width = GroupRect.width;
            BottomBarRect.height = (btnHeight + BasePadding) * 2;
            BottomBarRect.y = GroupRect.height 
                              - BottomBarRect.height 
                              - BasePadding;   // 底余白
            
            // SearchBar
            SearchBarRect.x = ToolBarRect.x;
            SearchBarRect.y = ToolBarRect.yMax + BasePadding;
            SearchBarRect.width = GroupRect.width;
            SearchBarRect.height = BaseCellUnitHeight;
            
            // 主视图
            MainViewRect.x = ToolBarRect.x;
            MainViewRect.y = SearchBarRect.yMax + BasePadding;
            MainViewRect.width = GroupRect.width - MainViewVBarWidth / 2.0f;
            MainViewRect.height = BottomBarRect.y - BasePadding -
                                  BasePadding - SearchBarRect.yMax;

        }

#endregion

        /// <summary>
        /// 绘制WindowGUI.
        /// </summary>
        protected override void OnWindowGui()
        {

            // base.OnWindowGui();
            var selectedTabIndex = ConfData.CurTabIndex;
            DrawToolBars(0, ModelEditor.Tabs.ToArray(), ref selectedTabIndex);
            
            // 检测当前平台
            if(!CheckPlatformByTabIndex(selectedTabIndex)) return;
            
            // 取得配置信息
            if (selectedTabIndex != ConfData.CurTabIndex)
            {
                ConfData.CurTabIndex = selectedTabIndex;
                if (ConfData.IsReloadNecessary(_searchKey))
                {
                    ConfData.ReloadPrefabs(_searchKey, ShowPrefabsReloadProgressBar, ShowErrorDialog);
                    
                    // 刷新
                    UtilsAsset.AssetsRefresh();
                }
            }

            var tab = ModelEditor.Tabs[ConfData.CurTabIndex];
            switch (tab)
            {
                case ModelEditor._TAB_PC:
                case ModelEditor._TAB_ANDROID:
                case ModelEditor._TAB_IOS:
                    OnWindowOtherTab(ConfData.CurTabIndex);
                    break;
                case ModelEditor._TAB_SETTINGS:
                    OnWindowSettingsTab(ConfData.CurTabIndex);
                    break;
                default:
                    OnWindowTabOfDefault();
                    break;
            }

        }

        /// <summary>
        /// 应用导入数据数据.
        /// </summary>
        /// <param name="iData">数据.</param>
        /// <param name="iForceClear">强制清空标志位.</param>
        protected override void ApplyImportData(ModelMeshEditorData iData, bool iForceClear)
        {
            if (null == iData)
            {
                return;
            }

            // 清空
            if (iForceClear)
            {
                Clear();
            }

            ConfData.CurTabIndex = iData.CurTabIndex;
            ConfData.ExportDir = iData.ExportDir;
            
            ConfData.MobileShaders.AddRange(iData.MobileShaders);
            ConfData.InitMobileShaders();
            
            ConfData.Config.AddRange(iData.Config);
            ConfData.Models.AddRange(iData.Models);
            ConfData.Settings.AddRange(iData.Settings);
            
            // 校验是否需要重新载入
            if (ConfData.IsReloadNecessary(_searchKey))
            {
                // 重载
                ConfData.ReloadPrefabs(_searchKey, ShowPrefabsReloadProgressBar, ShowErrorDialog);
            }
            
            UtilsAsset.SetAssetDirty (this);  
        }
        
#region OnWindowGUI - Tab

        /// <summary>
        /// 检测当前平台
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns></returns>
        private bool CheckPlatformByTabIndex(int iTabIndex)
        {
            // 非平台，则校验通过
            var flg = !ModelEditor.isPlatformTab(iTabIndex);
            if (flg) return true;
            
            // PC平台
            if (ModelEditor.isPCTab(iTabIndex))
            {
#if UNITY_STANDALONE
                flg = true;
#else
                flg = false;
                ConfData.CurTabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT);
#endif
            }
            
            // Android平台
            if (ModelEditor.isAndroidTab(iTabIndex))
            {
#if UNITY_ANDROID || UNITY_ANDROID_API
                flg = true;
#else
                flg = false;
                ConfData.CurTabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT);
#endif
            }
            
            // iOS平台
            if (ModelEditor.isIOSTab(iTabIndex))
            {
#if UNITY_IPHONE || UNITY_IOS
                flg = true;
#else
                flg = false;
                ConfData.CurTabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT);
#endif
            }
            
            
            if (!flg)
            {
                ShowDialog("错误", $"请切换当前平台到{ModelEditor.Tabs[iTabIndex]}！");
            }
            
            return flg;
        }

        /// <summary>
        /// Tab - Default
        /// </summary>
        private void OnWindowTabOfDefault()
        {
            DrawGroup(GroupRect.position, GroupRect.width, GroupRect.height, delegate
            {
                // 获取显示列表
                var models = ConfData.GetDisplayModels(_searchKey);
                // ToolBar
                OnToolBarGui(ToolBarRect.position, ToolBarRect.width, ToolBarRect.height, 
                    ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT), models?.Count ?? 0);
                
                // 检索Bar
                var lastSearchKey = _searchKey;
                OnSearchBarGui(SearchBarRect.position, SearchBarRect.width, SearchBarRect.height, ref lastSearchKey);
                // 校验检索对象并判断是否重载
                {
                    if ((_searchKey != null && (lastSearchKey == null)) ||
                        (_searchKey == null) && (lastSearchKey != null) ||
                        (_searchKey != null && lastSearchKey != null && !_searchKey.Equals(lastSearchKey)))
                    {
                        _searchKey = lastSearchKey;
                        if (ConfData.IsReloadNecessary(_searchKey))
                        {
                            ConfData.ReloadPrefabs(_searchKey, ShowPrefabsReloadProgressBar, ShowErrorDialog);
                        } 
                    }
                }
                
                // 绘制滚动列表
                var totalCount = models?.Count ?? 0;
                var svViewHeight = 0 == totalCount ? MainViewRect.height : totalCount * defaultMainViewRowHeight + BasePadding * 2 + MainViewHBarHeight;
                var svViewRect = new Rect(
                    BasePadding, BasePadding, 
                    MainViewRect.width - BasePadding * 2, 
                    svViewHeight >= MainViewRect.height ? svViewHeight : MainViewRect.height);
                // 取得配置信息
                var config = ConfData.GetConfigByTabIndex(ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT));
                DrawScrollView(ref config.ScrollOffset, MainViewRect, svViewRect, 
                    delegate(Rect iRect)
                    {
                        var dispalyInfo = (0 >= ConfData.Models.Count) ? "+\n添加\n将相应模型的预制体拖动到该区域!" : "";
                        var dropObj = GetObjectFromDragArea(
                            1, "RL Background", 
                            (int)iRect.width, (int)iRect.height,
                            dispalyInfo,null,
                            (iDragObj, iPath) =>
                            {
                                if (UtilsTools.CheckFilePath(iPath, ".prefab$"))
                                {
                                    if (!ConfData.AddModelByPath(iPath))
                                    {
                                        ShowDialog("警告",
                                            $"所选对象已经被追加!!!\nPath : {iPath}",
                                            "确认");
                                    }
                                }
                                else
                                {
                                    ShowDialog("警告",
                                        $"所选对象并非为预制体!!!\nPath : {iPath}",
                                        "确认");
                                }
                            });

                        if(0 >= models.Count) return;
                        // 绘制列表
                        for (var i = models.Count - 1; i >= 0; --i)
                        {
                            var model = models[i];
                            if (!File.Exists(model.Path))
                            {
                                this.Error("OnWindowTabOfDefault():File is not exist!!(path:{0})", model.Path);
                                continue;
                            }
                            
                            // 绘制行
                            var noticeLabelStyle = (i % 2 == 0) ? "flow node 2" : "flow node 5";
                            var rowRect = OnScrollViewDefaultTabRowGui(
                                new Vector2(iRect.x, (models.Count - i - 1) * (defaultMainViewRowHeight + BasePadding) + BasePadding * 2),
                                iRect.width - MainViewVBarWidth / 2.0f, defaultMainViewRowHeight, i, model, models, noticeLabelStyle);
                        }
                    }, "ProfilerScrollviewBackground");
                
                // 底部Bar
                OnBottomBarGui(BottomBarRect.position, BottomBarRect.width, BottomBarRect.height,
                    ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT));
            }, "GroupBox");
        }

        /// <summary>
        /// Tab - Default&Settings以外
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        private void OnWindowOtherTab(int iTabIndex)
        {
            DrawGroup(GroupRect.position, GroupRect.width, GroupRect.height, delegate{
                
                // 获取显示列表
                var models = ConfData.GetDisplayModelsByTabIndex(_searchKey, iTabIndex);
                // ToolBar
                OnToolBarGui(ToolBarRect.position, ToolBarRect.width, ToolBarRect.height, 
                    iTabIndex, models?.Count ?? 0);
                
                // 检索Bar
                var lastSearchKey = _searchKey;
                OnSearchBarGui(SearchBarRect.position, SearchBarRect.width, SearchBarRect.height, ref lastSearchKey);
                // 校验检索对象并判断是否重载
                {
                    if ((_searchKey != null && (lastSearchKey == null)) ||
                        (_searchKey == null) && (lastSearchKey != null) ||
                        (_searchKey != null && lastSearchKey != null && !_searchKey.Equals(lastSearchKey)))
                    {
                        _searchKey = lastSearchKey;
                        if (ConfData.IsReloadNecessary(_searchKey))
                        {
                            ConfData.ReloadPrefabs(_searchKey, ShowPrefabsReloadProgressBar, ShowErrorDialog);
                        } 
                    }
                }
                
                // 绘制滚动列表
                var totalCount = models?.Count ?? 0;
                var svViewHeight = 0 == totalCount ? MainViewRect.height : totalCount * defaultMainViewRowHeight + BasePadding * 2 + MainViewHBarHeight;
                var svViewRect = new Rect(
                    BasePadding, BasePadding, 
                    MainViewRect.width - BasePadding * 2, 
                    svViewHeight >= MainViewRect.height ? svViewHeight : MainViewRect.height);
                // 取得配置信息
                var config = ConfData.GetConfigByTabIndex(iTabIndex);
                DrawScrollView(ref config.ScrollOffset, MainViewRect, svViewRect, 
                    delegate(Rect iRect)
                    {
                        if(0 >= models.Count) return;
                        // 绘制列表
                        for (var i = models.Count - 1; i >= 0; --i)
                        {
                            var model = models[i];
                            if (!File.Exists(model.Path))
                            {
                                this.Error("OnWindowOtherTab():File is not exist!!(path:{0})", model.Path);
                                continue;
                            }
                            
                            // 绘制行
                            var noticeLabelStyle = (i % 2 == 0) ? "flow node 2" : "flow node 5";
                            var rowRect = OnScrollViewOtherTabRowGui(
                                new Vector2(iRect.x, (models.Count - i - 1) * (otherMainViewRowHeight + BasePadding) + BasePadding * 2),
                                iRect.width - MainViewVBarWidth / 2.0f, otherMainViewRowHeight, 
                                i, model, iTabIndex, models, noticeLabelStyle);
                        }
                    }, "ProfilerScrollviewBackground");
                
                // 底部Bar
                OnBottomBarGui(BottomBarRect.position, BottomBarRect.width, BottomBarRect.height,
                    iTabIndex);
                
            }, "GroupBox");
        }

        /// <summary>
        /// Tab - Settings
        /// <param name="iTabIndex">Tab索引</param>
        /// </summary>
        private void OnWindowSettingsTab(int iTabIndex)
        {
            DrawGroup(GroupRect.position, GroupRect.width, GroupRect.height, delegate
            {
                // ToolBar
                var rect = OnToolBarGui(ToolBarRect.position, ToolBarRect.width, ToolBarRect.height, iTabIndex);

                var subGroupPos = GroupRect.position;
                subGroupPos.x += BasePadding;

                var subGroupSize = GroupRect.size;
                subGroupSize.x -= BasePadding * 3;
                subGroupSize.y = (BaseCellUnitHeight + BasePadding) * 5 + BasePadding;
                
                // Default
                rect = DrawGroup(subGroupPos, 
                    subGroupSize.x, subGroupSize.y, 
                    delegate
                {
                    // 开始坐标
                    var pos = new Vector2(BasePadding, BasePadding);
                    var width = subGroupSize.x - BasePadding - BasePadding;
                    var height = subGroupSize.y - BasePadding - BasePadding;
                
                    // 取得默认设置
                    var platformTabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT);
                    // 绘制标题 - 默认
                    rect = OnSettingsDetailGui(pos, width, height, platformTabIndex);
                    
                }, "GroupBox");
                
                // Pc
#if UNITY_STANDALONE
                
                subGroupPos.y = rect.yMax + BasePadding;
                subGroupSize.y += (BaseCellUnitHeight + BasePadding); 
                
                rect = DrawGroup(subGroupPos, 
                    subGroupSize.x, subGroupSize.y + BaseCellUnitHeight + BasePadding, 
                    delegate
                {
                    // 开始坐标
                    var pos = new Vector2(BasePadding, BasePadding);
                    var width = subGroupSize.x - BasePadding - BasePadding;
                    var height = subGroupSize.y - BasePadding - BasePadding;
                
                    // 取得默认设置
                    var platformTabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_PC);
                    // 绘制标题 - PC
                    rect = OnSettingsDetailGui(pos, width, height, platformTabIndex);
                    
                }, "GroupBox");
#endif
                
                // Android
#if UNITY_ANDROID || UNITY_ANDROID_API
                
                subGroupPos.y = rect.yMax + BasePadding;
                subGroupSize.y += (BaseCellUnitHeight + BasePadding); 
                
                rect = DrawGroup(
                    subGroupPos, subGroupSize.x, subGroupSize.y + (BaseCellUnitHeight + BasePadding) * 2, 
                    delegate
                {
                    // 开始坐标
                    var pos = new Vector2(BasePadding, BasePadding);
                    var width = subGroupSize.x - BasePadding - BasePadding;
                    var height = subGroupSize.y - BasePadding - BasePadding;
                
                    // 取得默认设置
                    var platformTabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_ANDROID);
                    // 绘制标题 - Android
                    rect = OnSettingsDetailGui(pos, width, height, platformTabIndex);
                    
                }, "GroupBox");
                
#endif

                // iOS
#if UNITY_IPHONE || UNITY_IOS

                subGroupPos.y = rect.yMax + BasePadding;
                subGroupSize.y += (BaseCellUnitHeight + BasePadding); 
                
                rect = DrawGroup(subGroupPos, subGroupSize.x, subGroupSize.y, delegate
                {
                    // 开始坐标
                    var pos = new Vector2(BasePadding, BasePadding);
                    var width = subGroupSize.x - BasePadding - BasePadding;
                    var height = subGroupSize.y - BasePadding - BasePadding;
                
                    // 取得默认设置
                    var platformTabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_IOS);
                    // 绘制标题 - iOS
                    rect = OnSettingsDetailGui(pos, width, height, platformTabIndex);
                    
                }, "GroupBox");
#endif
                
            }, "GroupBox");
        }
        
        /// <summary>
        /// ToolBar
        /// </summary>
        /// <param name="iPos">开始未知</param>
        /// <param name="iWidth">宽度</param>
        /// <param name="iHeight">高度</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iModelsCount">模型计数</param>
        /// <returns>绘制范围</returns>
        private Rect OnToolBarGui(
            Vector2 iPos, float iWidth, float iHeight, 
            int iTabIndex, int iModelsCount = 0)
        {
            var displayRect = new Rect(iPos.x, iPos.y, iWidth, iHeight);

            var isSettingsTab = ModelEditor.isSettingsTab(iTabIndex);
            var pos = new Vector2(displayRect.xMax - BasePadding, displayRect.y);
            
            // ClearAll
            if (isSettingsTab)
            {
                pos.x -= (middleBtnWidth + BasePadding);
                DrawButton(
                    pos, middleBtnWidth, btnHeight,
                    "ClearAll", "PreButtonRed", delegate
                    {
                        // 清空所有对象
                        ConfData.Clear();
                        // 清空进度条
                        ProgressBarClear();
                    });

            }
            else
            {
                // Clear
                {
                    pos.x -= (middleBtnWidth + BasePadding);
                    DrawButton(
                        pos, middleBtnWidth, btnHeight,
                        "Clear", "PreButtonRed", delegate
                        {
                            // 清空对象
                            ConfData.ClearByTabIndex(iTabIndex);
                            // 清空进度条
                            ProgressBarClear();
                        });
                } 
            }

            // Export
            {
                pos.x -= (middleBtnWidth + BasePadding);
                DrawButton(
                    pos, middleBtnWidth, btnHeight,
                    "Export", "PreButtonGreen", delegate
                    {
                        // 导出设定文件
                        ExportToJsonFile();
                    });
            }
            
            // Import
            {
                pos.x -= (middleBtnWidth + BasePadding);
                DrawButton(
                    pos, middleBtnWidth, btnHeight,
                    "Import", "PreButtonGreen", delegate
                    {
                        // 导入设定文件
                        ImportFromJsonFile();
                    });
            }

            if (!isSettingsTab)
            {
                // 合计
                {
                    var labelWidth = cellSize * 2;
                    pos.x -= (labelWidth + BasePadding);
                    DrawLabel(
                        pos, labelWidth, btnHeight - 2,
                        $"合计 : {iModelsCount}", "flow node 5", 
                        12, TextAnchor.MiddleCenter,
                        0, 0, 5, 0, 14, 0, 
                        true);
                } 
            }
            
            return displayRect; 
        }
        
        /// <summary>
        /// 搜索栏
        /// </summary>
        /// <param name="iPos">开始未知</param>
        /// <param name="iWidth">宽度</param>
        /// <param name="iHeight">高度</param>
        /// <param name="iSearchKey">检索Key</param>
        /// <returns>绘制范围</returns>
        private Rect OnSearchBarGui(
            Vector2 iPos, float iWidth, float iHeight,
            ref string iSearchKey)
        {
            var displayRect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
            var searchBarWidth = iWidth - BasePadding * 2;
            {
                var rect = DrawTextField(
                    iPos, searchBarWidth, iHeight,
                    ref iSearchKey, "SearchTextField", TextAnchor.MiddleLeft);
            }
            return displayRect; 
        }
        
        /// <summary>
        /// 底部Bar
        /// </summary>
        /// <param name="iPos">开始未知</param>
        /// <param name="iWidth">宽度</param>
        /// <param name="iHeight">高度</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>绘制范围</returns>
        private Rect OnBottomBarGui(
            Vector2 iPos, float iWidth, float iHeight,
            int iTabIndex)
        {
            var displayRect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
            var pos = iPos;
            var displayRectTmp = displayRect;
            
            // 导出路径
            {
                var widthTmp = iWidth - BasePadding - longBtnWidth - BasePadding;
                var outputDir = ConfData.GetDisplayExportDirByTabIndex(iTabIndex);
                displayRectTmp = DrawLabel(pos, widthTmp, btnHeight, $"导出目录 : {outputDir}", 
                    "flow node 3", 11, TextAnchor.MiddleLeft, 0, 0,
                    5, 0, 12, 0, true);
                
                // 选择文件夹按钮
                pos.x = displayRectTmp.xMax + BasePadding;
                displayRectTmp = DrawButton(pos, longBtnWidth - BasePadding, btnHeight, "Browser...", "PreButtonBlue",
                    delegate
                    {
                        var dir = EditorUtility.OpenFolderPanel(
                            "选择导出目录", ConfData.ExportDir,"");
                        if (string.IsNullOrEmpty(dir)) return;
                        ConfData.ExportDir = UtilsTools.CheckMatchPath(dir);
                        // 导出设定文件，以便保存
                        ExportToJsonFile();
                    });
            }
            
            // 批量处理
            {
                pos.x = displayRect.xMax
                        - longBtnWidth - BasePadding
                        - longBtnWidth - BasePadding;
                pos.y += btnHeight + BasePadding;
                displayRectTmp = DrawButton(
                    pos, longBtnWidth, btnHeight, 
                    "批量处理", "PreButtonGreen",
                    delegate
                    {
                        // 批量处理
                        if (ModelEditor.isDefaultTab(iTabIndex))
                        {
                            CombineMesh(ConfData.Models.ToArray()); 
                        }
                        else
                        {
                            ModelSimplify(ConfData.Models.ToArray(), iTabIndex);
                        }

                    });
            }
            
            // 批量删除
            {
                pos.x = displayRectTmp.xMax + BasePadding;
                displayRectTmp = DrawButton(
                    pos, longBtnWidth - BasePadding, btnHeight, 
                    "批量删除", "PreButtonRed",
                    delegate
                    {
                        // 批量删除
                        DeleteModels();
                    });
            }
            
            return displayRect;
        }
        
        /// <summary>
        /// 绘制滚动行 - 默认
        /// </summary>
        /// <param name="iPos">开始未知</param>
        /// <param name="iWidth">宽度</param>
        /// <param name="iHeight">高度</param>
        /// <param name="iRowIndex">行索引</param>
        /// <param name="iCurTarget">当前模型信息</param>
        /// <param name="iModels">模型列表信息</param>
        /// <param name="iStyleName">风格名</param>
        /// <returns>绘制范围</returns>
        private Rect OnScrollViewDefaultTabRowGui(
            Vector2 iPos, float iWidth, float iHeight, int iRowIndex, 
            ModelInfo iCurTarget, List<ModelInfo> iModels,
            string iStyleName)
        {
            var displayRect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
            var displayRectTmp = displayRect;
            var pos = Vector2.zero;
            var startPosY = iPos.y;
            // 选择框
            {
                var selectBoxWidth = BaseCellUnitWidth / 2.0f;
                var selectBoxSize = selectBoxWidth >= iHeight ? iHeight : selectBoxWidth;
                pos.x += BaseCellUnitWidth / 2.0f;
                pos.y = iPos.y + (iHeight - selectBoxSize) / 2.0f - 5.0f;
                displayRectTmp = DrawToggle(
                    pos, selectBoxSize, selectBoxSize, ref iCurTarget.Checked); 
            }
            
            // ID
            {
                var labelIdWidth = BaseCellUnitWidth * 3;
                var labelIdSize = labelIdWidth >= iHeight ? iHeight : labelIdWidth;
                pos.x = displayRectTmp.xMax + BasePadding;
                pos.y = displayRect.y + (iHeight - labelIdSize) / 2.0f;
                displayRectTmp = DrawLabel(
                    pos, labelIdSize, labelIdSize,
                    $"{iCurTarget.ID}", "flow node hex 1", 14, 
                    TextAnchor.MiddleCenter, 1, -3);
            }
            
            // 预览Icon&标题
            {
                var iconWidth = BaseCellUnitWidth * 4;
                var iconSize = iconWidth >= iHeight ? iHeight : iconWidth;
                pos.x = displayRectTmp.xMax + BasePadding + (iconWidth - iconSize) / 2.0f;
                pos.y = displayRect.y + (iHeight - iconSize) / 2.0f;
                if (null != iCurTarget.PreviewIcon)
                {
                    displayRectTmp = DrawTexture(
                        pos, iconSize, iconSize, 
                        iCurTarget.PreviewIcon);
                }

                pos.y = pos.y + iconSize / 2.0f + BasePadding * 2.0f;
                var previewTxt = UtilsTools.GetFileName(iCurTarget.Path, false);
                displayRectTmp = DrawLabel(
                    pos, iconSize, iconSize, previewTxt,
                    "WhiteBoldLabel", 12);
            }
            
            var labelHeight = BaseCellUnitHeight;
            var labelPathWidth = iWidth
                                 - displayRectTmp.xMax                 // 左侧
                                 - (BasePadding + middleBtnWidth)      // 合并网格按钮
                                 - (BasePadding + middleBtnWidth);     // 删除按钮
                                 // - MainViewVBarWidth / 2.0f;          // 垂直滚动条宽度
            // 路径
            {
                pos.x = displayRectTmp.xMax + BasePadding;
                pos.y = startPosY;
                displayRectTmp = DrawLabel(pos, labelPathWidth, labelHeight, iCurTarget.Path, 
                    iStyleName, 10, TextAnchor.MiddleLeft, 0, 0,
                    5, 0, 12, 0, true);
            }
            
            // 详细信息
            var posXTmp = pos.x;
            {
                pos.y = displayRectTmp.yMax + BasePadding;
                displayRectTmp = DrawLabel(pos, 50.0f, labelHeight, "详细信息",
                    "WhiteBoldLabel", 12, TextAnchor.MiddleLeft);

                pos.x = displayRectTmp.xMax + BasePadding;
                var detailTxt = GetTextOfDetail(iCurTarget.Physical);
                displayRectTmp = DrawLabel(pos, labelPathWidth - 50.0f - BasePadding, labelHeight, detailTxt, 
                    iStyleName, 10, TextAnchor.MiddleLeft, 0, 0,
                    5, 0, 12, 0, true);
            }
            
            // GPU渲染信息统计
            {
                pos.x = posXTmp + BasePadding;
                pos.y = displayRectTmp.yMax + BasePadding;

                var widthTmp = labelPathWidth - BasePadding - middleBtnWidth - BasePadding;
                var heightTmp = 75;
                var GPUTxt = GetTextOfGpuStatistics(iCurTarget.GPU);
                displayRectTmp = DrawLabel(pos, widthTmp, heightTmp, GPUTxt,
                    iStyleName, 10, TextAnchor.MiddleLeft, 0, 0,
                    5, 0, 13, 0, true);
                
                // GUI渲染数据采样按钮
                pos.x = displayRectTmp.xMax + BasePadding;
                displayRectTmp = DrawButton(
                    pos, middleBtnWidth, heightTmp, 
                    "数据\n采样", "PreButtonGreen", 
                    delegate
                {
                    bool IsDataSampling(ModelInfo iModel)
                    {
                        var list = iModels.Where(iO => iModel.ID != iO.ID)
                            .ToList();
                        if (0 >= list.Count) return false;
                        var flg = false;
                        foreach (var tmp in list)
                        {
                            flg = tmp.DataSampling;
                            if (flg) break;
                        }
                        return flg;
                    }
                    
                    // 采样数据
                    OnSampledDataBtnClick(iCurTarget, IsDataSampling);
                });
            }
            
            // 总耗时
            {
                var widthTmp = middleBtnWidth + BasePadding + middleBtnWidth;
                pos.x = displayRectTmp.xMax + BasePadding;
                pos.y = startPosY;
                
                var totalCostTimeTxt = UtilsDateTime.GetDisplayDeltaTime(iCurTarget.TotalCostTime);
                displayRectTmp = DrawLabel(pos, widthTmp, labelHeight, $"+ {totalCostTimeTxt}",
                    iStyleName, 10, TextAnchor.MiddleCenter, 0, 0,
                    5, 0, 14, 0, true);
            }
            
            // 网格合并&除
            {
                pos.y = displayRectTmp.yMax + BasePadding;
                var heightTmp = iHeight
                                - BasePadding - labelHeight  // 总耗时  
                                - BasePadding                // 上余白
                                - BasePadding;               // 下余白
                displayRectTmp = DrawButton(
                    pos, middleBtnWidth, heightTmp,
                    "网格\n合并", "PreButtonGreen", delegate
                    {
                        // 网格合并
                        CombineMesh(iCurTarget);
                        // 导出最新配置
                        ExportToJsonFile();
                    });

                pos.x = displayRectTmp.xMax + BasePadding;
                displayRectTmp = DrawButton(
                    pos, middleBtnWidth, heightTmp,
                    "删除", "PreButtonRed", delegate
                    {
                        ConfData.RemoveModelById(iCurTarget.ID);
                        // 导出最新配置
                        ExportToJsonFile();
                    });
            }
            return displayRect;
        }

        /// <summary>
        /// 绘制滚动行 - 默认
        /// </summary>
        /// <param name="iPos">开始未知</param>
        /// <param name="iWidth">宽度</param>
        /// <param name="iHeight">高度</param>
        /// <param name="iRowIndex">行索引</param>
        /// <param name="iCurTarget">当前模型信息</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <param name="iModels">模型列表信息</param>
        /// <param name="iStyleName">风格名</param>
        /// <returns>绘制范围</returns>
        private Rect OnScrollViewOtherTabRowGui(
            Vector2 iPos, float iWidth, float iHeight, int iRowIndex,
            ModelInfo iCurTarget, int iTabIndex, List<ModelInfo> iModels,
            string iStyleName)
        {
            var displayRect = new Rect(iPos.x, iPos.y, iWidth, iHeight);
            var displayRectTmp = displayRect;
            var pos = Vector2.zero;
            var startPosY = iPos.y;
            var simplify = iCurTarget.GetSimplifyInfoByTabIndex(iTabIndex);
            // 选择框
            {
                var selectBoxWidth = BaseCellUnitWidth / 2.0f;
                var selectBoxSize = selectBoxWidth >= iHeight ? iHeight : selectBoxWidth;
                pos.x += BaseCellUnitWidth / 2.0f;
                pos.y = iPos.y + (iHeight - selectBoxSize) / 2.0f - 5.0f;
                displayRectTmp = DrawToggle(
                    pos, selectBoxSize, selectBoxSize, ref simplify.Checked); 
            }
            
            // ID
            {
                var labelIdWidth = BaseCellUnitWidth * 3;
                var labelIdSize = labelIdWidth >= iHeight ? iHeight : labelIdWidth;
                pos.x = displayRectTmp.xMax + BasePadding;
                pos.y = displayRect.y + (iHeight - labelIdSize) / 2.0f;
                displayRectTmp = DrawLabel(
                    pos, labelIdSize, labelIdSize,
                    $"{iCurTarget.ID}", "flow node hex 1", 14, 
                    TextAnchor.MiddleCenter, 1, -3);
            }
            
            // 预览Icon&标题
            {
                var iconWidth = BaseCellUnitWidth * 4;
                var iconSize = iconWidth >= iHeight ? iHeight : iconWidth;
                pos.x = displayRectTmp.xMax + BasePadding + (iconWidth - iconSize) / 2.0f;
                pos.y = displayRect.y + (iHeight - iconSize) / 2.0f;
                if (null != iCurTarget.PreviewIcon)
                {
                    displayRectTmp = DrawTexture(
                        pos, iconSize, iconSize, 
                        iCurTarget.PreviewIcon);
                }

                pos.y = pos.y + iconSize / 2.0f + BasePadding * 2.0f;
                var previewTxt = UtilsTools.GetFileName(iCurTarget.Path, false);
                displayRectTmp = DrawLabel(
                    pos, iconSize, iconSize, previewTxt,
                    "WhiteBoldLabel", 12);
            }
            
            var labelHeight = BaseCellUnitHeight;
            var labelPathWidth = iWidth
                                 - displayRectTmp.xMax                 // 左侧
                                 - (BasePadding + middleBtnWidth)      // 合并网格按钮
                                 - (BasePadding + middleBtnWidth);     // 删除按钮
                                 // - MainViewVBarWidth / 2.0f;          // 垂直滚动条宽度
            // 路径
            {
                pos.x = displayRectTmp.xMax + BasePadding;
                pos.y = startPosY;
                displayRectTmp = DrawLabel(pos, labelPathWidth, labelHeight, ConfData.GetDefaultPrefabPath(iCurTarget.Path), 
                    iStyleName, 10, TextAnchor.MiddleLeft, 0, 0,
                    5, 0, 12, 0, true);
            }
            var labelPathPosMaxY = displayRectTmp.yMax;
            
            // 详细信息
            var detailTxtWidth = labelPathWidth - 50.0f - BasePadding;
            var posXTmp = pos.x;
            {
                pos.y = displayRectTmp.yMax + BasePadding;
                displayRectTmp = DrawLabel(pos, 50.0f, labelHeight, "详细信息",
                    "WhiteBoldLabel", 12, TextAnchor.MiddleLeft);

                pos.x = displayRectTmp.xMax + BasePadding;
                var detailTxt = GetDiffOfDetail(iCurTarget.Physical, simplify.Physical);
                displayRectTmp = DrawLabel(pos, detailTxtWidth, labelHeight, detailTxt, 
                    iStyleName, 10, TextAnchor.MiddleLeft, 0, 0,
                    5, 0, 12, 0, true);
            }
            
            // GPU渲染信息统计
            {
                pos.x = posXTmp + BasePadding;
                pos.y = displayRectTmp.yMax + BasePadding;

                var widthTmp = labelPathWidth / 2.0f - BasePadding - middleBtnWidth - BasePadding;
                var heightTmp = 75;
                var GPUTxt = GetDiffOfGpuStatistics(iCurTarget.GPU, simplify.GPU);
                displayRectTmp = DrawLabel(pos, widthTmp, heightTmp, GPUTxt,
                    iStyleName, 10, TextAnchor.MiddleLeft, 0, 0,
                    5, 0, 13, 0, true);
                
                // GUI渲染数据采样按钮
                pos.x = displayRectTmp.xMax + BasePadding;
                displayRectTmp = DrawButton(
                    pos, middleBtnWidth, heightTmp, 
                    "数据\n采样", "PreButtonGreen", 
                    delegate
                    {
                        bool IsDataSampling(ModelInfo iModel)
                        {
                            var list = iModels.Where(iO => iModel.ID != iO.ID)
                                .ToList();
                            if (0 >= list.Count) return false;
                            var flg = false;
                            foreach (var tmp in list)
                            {
                                flg = tmp.DataSampling;
                                if (flg) break;
                            }
                            return flg;
                        }
                    
                        // 采样数据
                        OnSampledDataBtnClick(iCurTarget, IsDataSampling);
                    });
            }
            
            // 降面信息
            {
                pos.x = displayRectTmp.xMax + BasePadding;
                pos.y = displayRectTmp.y;
                displayRectTmp = DrawLabel(pos, 100.0f, labelHeight, "降面设定", 
                    "WhiteBoldLabel", 12,
                    TextAnchor.MiddleLeft);
            }
            
            // 降面设定
            var startOffsetX = 10.0f;
            var simplifyPosX = 0.0f;
            {
                // 标题
                var costTimeLabelWith = 100.0f;
                pos.x += startOffsetX;
                var simplifyStartPoxXTmp = pos.x;
                pos.y = displayRectTmp.yMax + BasePadding;
                
                // 标题
                displayRectTmp = DrawLabel(pos, 50.0f, labelHeight, "百分比", 
                    "WhiteBoldLabel", 12,
                    TextAnchor.MiddleLeft);
                
                // 值
                pos.x = displayRectTmp.xMax + BasePadding;
                var sliderWidth = labelPathWidth / 2.0f - startOffsetX - 50.0f - BasePadding 
                                  - costTimeLabelWith - BasePadding;
                displayRectTmp = DrawSlider(pos, sliderWidth, labelHeight,
                    ref simplify.SimplifyRatio);

                // 耗时
                pos.x = displayRectTmp.xMax + BasePadding;
                var totalCostTimeTxt = UtilsDateTime.GetDisplayDeltaTime(simplify.CostTime);
                displayRectTmp = DrawLabel(pos, costTimeLabelWith - BasePadding, labelHeight, $"+ {totalCostTimeTxt}",
                    iStyleName, 10, TextAnchor.MiddleCenter, 0, 0,
                    5, 0, 14, 0, true);
                simplifyPosX = displayRectTmp.xMax + BasePadding; 
                
                // Android 或者 iOS的时候
                if (ModelEditor.isAndroidTab(iTabIndex) || ModelEditor.isIOSTab(iTabIndex))
                {
                    // 图集尺寸
                    pos.x = simplifyStartPoxXTmp;
                    pos.y = displayRectTmp.yMax + BasePadding;
                    displayRectTmp = DrawLabel(pos, 50.0f, labelHeight, "图集大小", 
                        "WhiteBoldLabel", 12,
                        TextAnchor.MiddleLeft);

                    pos.x = displayRectTmp.xMax + BasePadding;
                    var totalWidthTmp = labelPathWidth / 2.0f - BasePadding * 2 - startOffsetX;
                    var widthTmp = totalWidthTmp / 2.0f - 50.0f - BasePadding;
                    var selectedIndex = ModelEditor.TextureMaxSizes.IndexOf(simplify.AtlasMaxSize);
                    var lastSeletedIndex = selectedIndex;
                    displayRectTmp = DrawSelectList(pos, widthTmp, labelHeight, ModelEditor.TextureMaxSizes.ToArray(),
                        ref selectedIndex, null, "ToolbarPopup");
                    if (-1 >= selectedIndex)
                    {
                        selectedIndex = ModelEditor.TextureMaxSizes.IndexOf(2048);
                    }
                    if (lastSeletedIndex != selectedIndex)
                    {
                        simplify.AtlasMaxSize = ModelEditor.TextureMaxSizes[selectedIndex];
                    }
                    
                    // 纹理缩小
                    pos.x = displayRectTmp.xMax + BasePadding;
                    displayRectTmp = DrawLabel(pos, 50.0f, labelHeight, "纹理缩小", 
                        "WhiteBoldLabel", 12,
                        TextAnchor.MiddleLeft);
                    
                    pos.x = displayRectTmp.xMax + BasePadding;
                    displayRectTmp = DrawIntField(pos, widthTmp, labelHeight, ref simplify.TextureReduce);
                    simplify.TextureReduce = simplify.TextureReduce < 1 ? 1 : simplify.TextureReduce;

                    // 着色器
                    var shadersAddOrDelBtnWidth = shortBtnWidth * 1.4f;
                    pos.x = simplifyStartPoxXTmp;
                    pos.y = displayRectTmp.yMax + BasePadding;
                    displayRectTmp = DrawLabel(pos, 50.0f, labelHeight, "着色器", 
                        "WhiteBoldLabel", 12,
                        TextAnchor.MiddleLeft);

                    var selectListWidth = totalWidthTmp 
                                          - 50.0f
                                          - (shadersAddOrDelBtnWidth + BasePadding) * 2;
                    pos.x = displayRectTmp.xMax + BasePadding;
                    var shaderIdx = ConfData.GetSimplifyShaderIndexByTabIndex(iTabIndex, iCurTarget.ID);
                    var lastIndex = shaderIdx;
                    displayRectTmp = DrawSelectList(
                        pos, selectListWidth, labelHeight,
                        ConfData.MobileShaders.ToArray(), ref shaderIdx, 
                        null, "ToolbarPopup");
                    if (string.IsNullOrEmpty(simplify.Shader) || lastIndex != shaderIdx)
                    {
                        ConfData.SetSimplifyShaderIndexByTabIndex(iTabIndex, iCurTarget.ID, shaderIdx);
                    }
                    
                    // +/- 按钮
                    pos.x = displayRectTmp.xMax + BasePadding;
                    displayRectTmp = DrawButton(
                        pos, shadersAddOrDelBtnWidth, labelHeight,
                        "+", "PreButtonGreen", delegate
                        {
                            // 模型网格合并&降面
                            var filePath = EditorUtility.OpenFilePanel(
                                "选择着色器", Application.dataPath, "shader");
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                filePath = UtilsTools.CheckMatchPath(filePath);
                                // 导出设定文件，以便保存
                                var shader = AssetDatabase.LoadAssetAtPath<Shader>(filePath);
                                if (null != shader)
                                {
                                    // 添加着色器文件
                                    ConfData.AddSimplifyShader(iTabIndex, iCurTarget.ID, shader.name);
                                }
                            }

                        });
                    
                    pos.x = displayRectTmp.xMax + BasePadding;
                    displayRectTmp = DrawButton(
                        pos, shadersAddOrDelBtnWidth, labelHeight,
                        "-", "PreButtonRed", delegate
                        {
                            // 模型网格合并&降面
                            ConfData.DelSimplifyShader(iTabIndex, iCurTarget.ID, simplify.Shader);

                        });
                }
            }
            
            // 降面&删除按钮
            {
                pos.x = simplifyPosX;
                pos.y = startPosY;
                var heightTmp = iHeight
                                - BasePadding                // 上余白
                                - BasePadding;               // 下余白
                displayRectTmp = DrawButton(
                    pos, middleBtnWidth, heightTmp,
                    "降面", "PreButtonGreen", delegate
                    {
                        // 网格合并
                        ModelSimplify(iCurTarget, iTabIndex);
                        // 导出最新配置
                        ExportToJsonFile();
                    });

                pos.x = displayRectTmp.xMax + BasePadding;
                displayRectTmp = DrawButton(
                    pos, middleBtnWidth, heightTmp,
                    "删除", "PreButtonRed", delegate
                    {
                        ConfData.RemoveModelById(iCurTarget.ID);
                        // 导出最新配置
                        ExportToJsonFile();
                    });
            }
            
            return displayRect;
        }

        /// <summary>
        /// 绘制设定信息
        /// </summary>
        /// <param name="iPos">开始未知</param>
        /// <param name="iWidth">宽度</param>
        /// <param name="iHeight">高度</param>
        /// <param name="iTabIndex">Tab索引</param>
        /// <returns>绘制范围</returns>
        private Rect OnSettingsDetailGui(
            Vector2 iPos, float iWidth, float iHeight, int iTabIndex)
        {
            var rect = new Rect(iPos.x, iPos.y, iWidth, iHeight);

            var labelWidth = BaseCellUnitWidth * 8;
            var labelValueWidth = BaseCellUnitWidth * 14;
            var labelHeight = BaseCellUnitHeight;
            var rectTmp = rect;
            var pos = rect.position;
            
            // 绘制标题
            {
                rectTmp = DrawLabel(pos, iWidth, labelHeight, $"Texture Settings - {ModelEditor.Tabs[iTabIndex]}",
                    "flow node 5", 12, TextAnchor.MiddleLeft,
                    0, 0, 5, 0, 14, 0, 
                    true);
            }
            pos.x += BasePadding;
            var detailStartPosX = pos.x;
            var offsetX = BasePadding * 3;
            
            // 纹理类型
            var settings = ConfData.GetSettingByTabIndex(iTabIndex, true);
            {
                pos.x += BasePadding;
                detailStartPosX = pos.x;
                
                pos.y = rectTmp.yMax + BasePadding;
                rectTmp = DrawLabel(pos, labelWidth, labelHeight, "Texture Type");

                pos.x = rectTmp.xMax + offsetX + BasePadding;
                var texts = ModelEditor.TextureImporterTypes.GetDisplayTexts();
                var value = ModelEditor.TextureImporterTypes.GetIndexByValue(settings.CurType);
                rectTmp = DrawSelectList(pos, labelValueWidth, labelHeight, texts.ToArray(),
                    ref value, null, "ToolbarPopup");
                settings.CurType = ModelEditor.TextureImporterTypes.GetValueByIndex(value);
                
            }
            // 设定信息
            var texSettings = settings.GetImporterSettingByType(
                settings.CurType, iTabIndex, true);
            detailStartPosX += offsetX; 
            
            // 是否覆盖默认设置
            if (ModelEditor.isPlatformTab(iTabIndex))
            {
                pos.x = detailStartPosX;
                pos.y = rectTmp.yMax + BasePadding;
                
                var overridden = texSettings.Platform.overridden;
                var lastOverridden = overridden;
                rectTmp = DrawToggle(pos, 15.0f, labelHeight, ref overridden);
                if (lastOverridden != overridden)
                {
                    texSettings.Platform.overridden = overridden;
                }
                
                pos.x = rectTmp.xMax + BasePadding;
                rectTmp = DrawLabel(pos, labelWidth, labelHeight, $"Override for {ModelEditor.Tabs[iTabIndex]}");
            }
            
            // 纹理最大尺寸
            {
                pos.x = detailStartPosX;
                pos.y = rectTmp.yMax + BasePadding;
                rectTmp = DrawLabel(pos, labelWidth, labelHeight, "Max Size");

                pos.x = rectTmp.xMax + BasePadding;
                var selectedIndex = ModelEditor.TextureMaxSizes.IndexOf(texSettings.Platform.maxTextureSize);
                rectTmp = DrawSelectList(pos, labelValueWidth, labelHeight, ModelEditor.TextureMaxSizes.ToArray(),
                    ref selectedIndex, null, "ToolbarPopup");
                texSettings.Platform.maxTextureSize = ModelEditor.TextureMaxSizes[selectedIndex];
            }
            
            // 重置尺寸算法 - 只有当纹理的尺寸大于最大尺寸的时候才有效
            {
                pos.x = detailStartPosX;
                pos.y = rectTmp.yMax + BasePadding;
                rectTmp = DrawLabel(pos, labelWidth, labelHeight, "Resize Algorithm");
                
                pos.x = rectTmp.xMax + BasePadding;
                var selected = texSettings.Platform.resizeAlgorithm;
                selected = (TextureResizeAlgorithm)DrawSelectList(
                    pos, labelValueWidth, labelHeight, selected, 
                    ref rectTmp, null, "ToolbarPopup");
                texSettings.Platform.resizeAlgorithm = selected;
            }
            
            // 纹理格式
            {
                pos.x = detailStartPosX;
                pos.y = rectTmp.yMax + BasePadding;
                rectTmp = DrawLabel(pos, labelWidth, labelHeight, "Format"); 
                
                pos.x = rectTmp.xMax + BasePadding;
                var formatsSettings = ModelEditor.TextureImporterFormatsOfDefault;
                if (ModelEditor.isPlatformTab(iTabIndex))
                {
                    formatsSettings = ModelEditor.TextureImporterFormatsOfPlatform;
                }
                var texts = formatsSettings.GetDisplayTexts();
                var selected = formatsSettings.GetIndexByValue(texSettings.Platform.format);
                rectTmp = DrawSelectList(pos, labelValueWidth, labelHeight, texts.ToArray(),
                    ref selected, null, "ToolbarPopup");
                texSettings.Platform.format = formatsSettings.GetValueByIndex(selected);
            }
            
            // 压缩质量
            // if(ModelEditor.isDefaultTab(iTabIndex))
            // {
            //     pos.x = detailStartPosX;
            //     pos.y = rectTmp.yMax + BasePadding;
            //     rectTmp = DrawLabel(pos, labelWidth, labelHeight, "Compression");
            //
            //     pos.x = rectTmp.xMax + BasePadding;
            //     var formats = ModelEditor.TextureImporterCompressions.GetDisplayTexts();
            //     var selected =
            //         ModelEditor.TextureImporterCompressions.GetIndexByValue(texSettings.Platform.textureCompression);
            //     rectTmp = DrawSelectList(pos, labelValueWidth, labelHeight, formats.ToArray(),
            //         ref selected, null, "ToolbarPopup");
            //     texSettings.Platform.textureCompression = ModelEditor.TextureImporterCompressions.GetValueByIndex(selected);
            // }
            // else
            {
                if (TextureImporterFormat.DXT1Crunched == texSettings.Platform.format ||
                    TextureImporterFormat.DXT5Crunched == texSettings.Platform.format ||
                    TextureImporterFormat.ETC_RGB4Crunched == texSettings.Platform.format ||
                    TextureImporterFormat.ETC2_RGBA8Crunched == texSettings.Platform.format)
                {
                    pos.x = detailStartPosX;
                    pos.y = rectTmp.yMax + BasePadding;
                    rectTmp = DrawLabel(pos, labelWidth, labelHeight, "Compressior Quality"); 
                    
                    pos.x = rectTmp.xMax + BasePadding;
                    var value = (float)texSettings.Platform.compressionQuality;
                    value = (0.0f >= value) ? 50.0f : value;
                    rectTmp = DrawSlider(pos, labelValueWidth, labelHeight, ref value,
                        0.0f, 100.0f);
                    texSettings.Platform.compressionQuality = (int)value;
                }
            }
            
            // // 碎片压缩
            // if(ModelEditor.isDefaultTab(iTabIndex))
            // {
            //     pos.x = detailStartPosX;
            //     pos.y = rectTmp.yMax + BasePadding;
            //     rectTmp = DrawLabel(pos, labelWidth, labelHeight, "Use Crunch Compression");
            //     
            //     pos.x = rectTmp.xMax + BasePadding;
            //     var value = texSettings.Platform.crunchedCompression;
            //     rectTmp = DrawToggle(pos, 30.0f, labelHeight, ref value);
            //     texSettings.Platform.crunchedCompression = value;
            //
            // }
            
            // 不支持ETC2的Android设备上的ETC2纹理解压缩格式覆盖
            if(ModelEditor.isAndroidTab(iTabIndex))
            {
                pos.x = detailStartPosX;
                pos.y = rectTmp.yMax + BasePadding;
                rectTmp = DrawLabel(pos, labelWidth, labelHeight, "Override ETC2 fallback"); 
                
                pos.x = rectTmp.xMax + BasePadding;
                var formats = ModelEditor.AndroidETC2Fallbacks.GetDisplayTexts();
                var selected =
                    ModelEditor.AndroidETC2Fallbacks.GetIndexByValue(texSettings.Platform.androidETC2FallbackOverride);
                rectTmp = DrawSelectList(pos, labelValueWidth, labelHeight, formats.ToArray(),
                    ref selected, null, "ToolbarPopup");
                texSettings.Platform.androidETC2FallbackOverride = ModelEditor.AndroidETC2Fallbacks.GetValueByIndex(selected);
            }
            
            return rect;
        }

#endregion

#endregion

#region ShowDialog

        /// <summary>
        /// 消失对话框（单按钮确定)
        /// </summary>
        /// <param name="iTitle">标题</param>
        /// <param name="iMsg">消失</param>
        private void ShowDialog(string iTitle, string iMsg)
        {
            ProgressBarClear();
            ShowDialog(iTitle, iMsg, "确定");
        }

#endregion

#region ProgressBar

        /// <summary>
        /// 显示预制体重载进度条
        /// </summary>
        /// <param name="iMaxCount">最大计数</param>
        /// <returns>进度计数器</returns>
        private ProgressCounter ShowPrefabsReloadProgressBar(int iMaxCount)
        {
            if (0 >= iMaxCount) return null;
            var step = new ProgressCountStep {Step = 1, Description = "状态:ID: {0} ({1}/{2}) : {3}", MaxCount = iMaxCount};
            return ShowProgressBar("预制体载入", step);  
        }
        
        /// <summary>
        /// 显示网格合并进度条
        /// </summary>
        /// <param name="iAutoClear">自动清空标识位(true:当达到100%时，进度条自动清除)</param>
        /// <returns>进度计数器</returns>
        private ProgressCounter ShowCombineMeshProgressBar(bool iAutoClear = true)
        {
            // 清空已存在的进度条
            ProgressBarClear();
            
            var steps = new List<ProgressCountStep>
            {
                new ProgressCountStep { Step = (int)CombineMeshStep.CombineMesh },
                new ProgressCountStep { Step = (int)CombineMeshStep.ExportMeshData },
                new ProgressCountStep { Step = (int)CombineMeshStep.ExportMatData },
                new ProgressCountStep { Step = (int)CombineMeshStep.ExportTexture },
                new ProgressCountStep { Step = (int)CombineMeshStep.ExportShader }
            };
            return ShowProgressBar("网格编辑", steps, iAutoClear);
        }

        /// <summary>
        /// 显示错误对话
        /// </summary>
        /// <param name="iMsgDetail">详细信息</param>
        private void ShowErrorDialog(string iMsgDetail)
        {
            // 清楚进度条
            var failedMsg = $"详细信息：\n{iMsgDetail}";
            // 显示错误弹窗
            ShowDialog("错误", failedMsg, "确定");
        }

#endregion

#region SampledData

        // 数据采集对象
        private bool _dataSampling = false;
        private ModelInfo _dataSampleTarget = null;
        private float _deltaTime = 0.0f;
        private GPUStatistics _originStatistics = null;
        private GPUStatistics _targetStatistics = null;
        private GameObject _dataSampleGo = null;

#region CollectorStage

        /// <summary>
        /// 收集器步骤
        /// </summary>
        private enum CollectorStage
        {
            // 原始数据收集
            OriginDataCollect = 0,
            // 缓冲时间
            BuffTime = 1,
            // 加载目标
            LoadTarget = 2,
            // 对象数据收集
            TargetDataCollect = 3,
            // 数据计算
            DataCalculation = 4,
            Max = DataCalculation
        }
        
        /// <summary>
        /// 统计信息收集器Stage
        /// </summary>
        private class StatisticsCollectorStage : JsonDataBase<StatisticsCollectorStage>
        {
            public CollectorStage Stage;
            /// <summary>
            /// 持续时间
            /// </summary>
            public float DurationTime;
            /// <summary>
            /// 特定操作已执行标志位
            /// </summary>
            public bool Executed;
            /// <summary>
            /// 运行中(即便特定操作已经执行，但是还在持续时间内的，也算作运行中)
            /// </summary>
            public bool Runing;
            /// <summary>
            /// 完成标志位(即执行了特定操作，又超过了持续时间)
            /// </summary>
            public bool Completed;
        }
        /// <summary>
        /// 数据收集器Stages
        /// </summary>
        private StatisticsCollectorStage[] _collectorStages = {
            // 时间缓冲
            new StatisticsCollectorStage {Stage = CollectorStage.BuffTime, DurationTime = 1.0f},
            // 原始数据收集
            new StatisticsCollectorStage {Stage = CollectorStage.OriginDataCollect, DurationTime = 0.5f},
            // 加载目标
            new StatisticsCollectorStage {Stage = CollectorStage.LoadTarget, DurationTime = 1.0f},
            // 时间缓冲
            new StatisticsCollectorStage {Stage = CollectorStage.BuffTime, DurationTime = 1.0f},
            // 目标数据收集
            new StatisticsCollectorStage {Stage = CollectorStage.TargetDataCollect, DurationTime = 0.5f},
            // 数据计算
            new StatisticsCollectorStage {Stage = CollectorStage.DataCalculation, DurationTime = 0.5f}
        };

        /// <summary>
        /// 初始化数据收集信息
        /// </summary>
        private void InitCollectorStages()
        {
            foreach (var stage in _collectorStages)
            {
                stage.Executed = false;
                stage.Runing = false;
                stage.Completed = false;
            }
        }
        
#endregion
        

        /// <summary>
        /// 取得Diff值文本
        /// </summary>
        /// <param name="iOriginValue">原值</param>
        /// <param name="iCurValue">现值</param>
        /// <returns>Diff值文本</returns>
        private string GetDiffValueText(int iOriginValue, int iCurValue)
        {
            float deltaValue = iCurValue - iOriginValue;
            var progress = 0 >= iOriginValue ? 0.0f : deltaValue / iOriginValue;
            progress = (0 == iOriginValue && 0 == iCurValue) ? 0.0f : progress;
            var progressTxt = UtilsTools.GetPercentText(progress);
            if (0 == deltaValue || 0.0f == progress) return "";
            return 0 < deltaValue ? $"(<color=\"red\">+{progressTxt}</color>)" : $"(<color=\"blue\">{progressTxt}</color>)";
        }

        /// <summary>
        /// 取得Diff信息(GPU统计信息)
        /// </summary>
        /// <param name="iOrigin">原本</param>
        /// <param name="iCurrent">现在</param>
        /// <returns>Diff信息</returns>
        private string GetDiffOfGpuStatistics(GPUStatistics iOrigin, GPUStatistics iCurrent)
        {
            return "<color=\"black\">GPU渲染统计</color>\n" + 
                   $"  Batches:{iCurrent.Batches}{GetDiffValueText(iOrigin.Batches, iCurrent.Batches)} " +
                   $"  Saved By Batching:{iCurrent.SaveBatching}{GetDiffValueText(iOrigin.SaveBatching, iCurrent.SaveBatching)} \n" +
                   $"  Tris:{iCurrent.Tris}{GetDiffValueText(iOrigin.Tris, iCurrent.Tris)} " +
                   $"  Verts:{iCurrent.Verts}{GetDiffValueText(iOrigin.Verts, iCurrent.Verts)} \n" +
                   $"  SetPassCalls:{iCurrent.SetPassCalls}{GetDiffValueText(iOrigin.SetPassCalls, iCurrent.SetPassCalls)} " +
                   $"  ShadowCasters:{iCurrent.ShadowCasters}{GetDiffValueText(iOrigin.ShadowCasters, iCurrent.ShadowCasters)} \n" +
                   $"  SkinnedMeshed:{iCurrent.VisibleSkinnedMeshed}{GetDiffValueText(iOrigin.VisibleSkinnedMeshed, iCurrent.VisibleSkinnedMeshed)} " +
                   $"  Animations:{iCurrent.VisibleAnimations}{GetDiffValueText(iOrigin.VisibleAnimations, iCurrent.VisibleAnimations)} \n" +
                   "<color=\"black\">    注意:摄像机参数不一样，统计结果也会有所不同</color>";
        }

        /// <summary>
        /// 取得文本信息信息(GPU统计信息)
        /// </summary>
        /// <param name="iStatistics">统计信息</param>
        /// <returns>文本信息信息(GPU统计信息)</returns>
        private string GetTextOfGpuStatistics(GPUStatistics iStatistics)
        {
            return "<color=\"black\">GPU渲染统计</color>\n" + 
                   $"  Batches:{iStatistics.Batches} " +
                   $"  Saved By Batching:{iStatistics.SaveBatching} \n" +
                   $"  Tris:{iStatistics.Tris} " +
                   $"  Verts:{iStatistics.Verts} \n" +
                   $"  SetPassCalls:{iStatistics.SetPassCalls} " +
                   $"  ShadowCasters:{iStatistics.ShadowCasters} \n" +
                   $"  SkinnedMeshed:{iStatistics.VisibleSkinnedMeshed} " +
                   $"  Animations:{iStatistics.VisibleAnimations} \n" +
                   "<color=\"black\">    注意:摄像机参数不一样，统计结果也会有所不同</color>";
        }
        
        /// <summary>
        /// 取得Diff信息(纹理/材质/着色器)
        /// </summary>
        /// <param name="iOrigin">原本</param>
        /// <param name="iCurrent">现在</param>
        /// <returns>Diff信息</returns>
        private string GetDiffOfDetail(PhysicalStatistics iOrigin, PhysicalStatistics iCurrent)
        {
            return $"Verts:{iCurrent.Verts}{GetDiffValueText(iOrigin.Verts, iCurrent.Verts)} " +
                   $"Tris:{iCurrent.Tris}{GetDiffValueText(iOrigin.Tris, iCurrent.Tris)} " +
                   $"Mats:{iCurrent.Mats}{GetDiffValueText(iOrigin.Mats, iCurrent.Mats)} " +
                   $"Shaders:{iCurrent.Shaders}{GetDiffValueText(iOrigin.Shaders, iCurrent.Shaders)} " +
                   $"Texs:{iCurrent.Textures}{GetDiffValueText(iOrigin.Textures, iCurrent.Textures)}";
        }

        /// <summary>
        /// 取得详细信息文本(纹理/材质/着色器)
        /// </summary>
        /// <param name="iStatistics">统计信息</param>
        /// <returns>详细信息文本(纹理/材质/着色器)</returns>
        private string GetTextOfDetail(PhysicalStatistics iStatistics)
        {
            return $"Verts:{iStatistics.Verts} " +
                   $"Tris:{iStatistics.Tris} " +
                   $"Mats:{iStatistics.Mats} " +
                   $"Shaders:{iStatistics.Shaders} " +
                   $"Texs:{iStatistics.Textures}";
        }

        /// <summary>
        /// 数据采集按钮点击事件
        /// </summary>
        /// <param name="iTarget">目标信息</param>
        /// <param name="iIsDataSampling">数据是否采集中委托</param>
        private void OnSampledDataBtnClick(
            ModelInfo iTarget, Func<ModelInfo, bool> iIsDataSampling)
        {
            _dataSampling = iIsDataSampling(iTarget);
            if (_dataSampling)
            {
                ShowDialog("警告", "别的预制体正在数据采集中，请耐心等待！", "确认");
                return; 
            }

            // 初始化数据收集Stages
            InitCollectorStages();

            _deltaTime = 0L;
            iTarget.DataSampling = !iTarget.DataSampling;
            _dataSampling = iTarget.DataSampling;
            // 数据采集对象
            _dataSampleTarget = iTarget;
        }
        
        private void Update()
        {
            // 数据尚未采集
            if(!_dataSampling) return;
            if (null == _dataSampleTarget) return;
            
            // 累加变化时间
            _deltaTime += Time.deltaTime;
            
            // 开始收集数据
            var curStage = GetStatisticsCollectorStage(_deltaTime);
            // 若已经执行完毕，则返回
            if (curStage.Completed) return;
            // 若已经执行过了，则返回
            if(curStage.Executed) return;
            switch (curStage.Stage)
            {
                case CollectorStage.BuffTime:
                {
                    //this.Info("Update():Stage:{0}", curStage.ToString());
                    // 设定焦点为当前GameView
                    SetGameViewFocus();
                }
                    break;
                case CollectorStage.OriginDataCollect:
                {
                    //this.Info("Update():Stage:{0}", curStage.ToString());
                    // 收集统计信息 - 源数据
                    _originStatistics = new GPUStatistics();
                    _originStatistics.AutoLoad();
                    
                }
                    break;
                case CollectorStage.LoadTarget:
                {
                    //this.Info("Update():Stage:{0}", curStage.ToString());
                    var prefabPath = _dataSampleTarget.Path;
                    if (!ModelEditor.isDefaultTab(ConfData.CurTabIndex))
                    {
                        var outputDir = ConfData.GetExportDirByTabIndex(ConfData.CurTabIndex, prefabPath);
                        prefabPath = $"{outputDir}/{UtilsTools.GetFileName(prefabPath)}";
                    }
                    _dataSampleGo = ModelEditor.LoadModelPrefab(prefabPath);
                }
                    break;
                case CollectorStage.TargetDataCollect:
                {
                    //this.Info("Update():Stage:{0}", curStage.ToString());
                    // 收集统计信息 - 目标数据
                    _targetStatistics = new GPUStatistics();
                    _targetStatistics.AutoLoad();
                }
                    break;
                case CollectorStage.DataCalculation:
                {
                    //this.Info("Update():Stage:{0}", curStage.ToString());
                    if (ModelEditor.isDefaultTab(ConfData.CurTabIndex))
                    {
                        _dataSampleTarget.GPU = _targetStatistics - _originStatistics; 
                    }
                    else
                    {
                        // 设定降面信息
                        var simplify = _dataSampleTarget.GetSimplifyInfoByTabIndex(ConfData.CurTabIndex);
                        simplify.GPU = _targetStatistics - _originStatistics; 
                    }
                    
                    // 设定执行完毕状态
                    _dataSampleTarget.DataSampling = false;
                    _dataSampleTarget = null;
                    _dataSampling = false;
                    _deltaTime = 0.0f;
                    if(null != _dataSampleGo) DestroyImmediate(_dataSampleGo);
                    
                    // 导出最新信息
                    ExportToJsonFile();
                }
                    break;
            }
            curStage.Executed = true;

        }
        
        /// <summary>
        /// 取得数据收集Stage
        /// </summary>
        /// <param name="iDurationTime">持续时间</param>
        /// <returns>数据收集Stage</returns>
        private StatisticsCollectorStage GetStatisticsCollectorStage(float iDurationTime)
        {
            if (0 >= _collectorStages.Length) return null;
            var durationTime = 0.0f;
            // 计算当前数据收集Stage
            var idx = 0;
            StatisticsCollectorStage curStage = null;
            while (true)
            {
                if(idx >= _collectorStages.Length) break;
                curStage = _collectorStages[idx];
                if(null == curStage) break;
                // 尚未执行特定操作
                if(!curStage.Executed) break;
                
                // 累加持续时间
                durationTime += curStage.DurationTime; 
                // 当前步骤若已经完成，则继续累加下一个操作
                if(curStage.Completed)
                {
                    ++idx;
                    continue;
                }

                // 若当前状态为运行中
                if (curStage.Runing)
                {
                    if (iDurationTime >= durationTime)
                    {
                        curStage.Completed = true;
                        curStage.Runing = false;
                        ++idx;
                        continue;
                    }
                }
                curStage.Runing = true;
                break;
            }
            return curStage;

        }

#region StatisticsCollector

        /// <summary>
        /// 设定GameView视图焦点
        /// </summary>
        private void SetGameViewFocus()
        {
            var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            // 激活GameView视图
            FocusWindowIfItsOpen(gameViewType);
        }

        /// <summary>
        /// 收集统计信息 - 源数据
        /// </summary>
        private GPUStatistics StatisticsCollector(string iPath = null)
        {
            var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            GPUStatistics statistics;
            // 采集原始数据
            if (string.IsNullOrEmpty(iPath))
            {
                // 激活GameView视图
                FocusWindowIfItsOpen(gameViewType);
                
                statistics = new GPUStatistics();
                statistics.AutoLoad();
                return statistics; 
            }
            
            // 记载预制体
            var dataSampleTarget = ModelEditor.LoadModelPrefab(iPath);
            if (null == dataSampleTarget)
            {
                //ShowDialog("错误", $"预制体加载失败！\nPath:{iPath}", "确认");
                return null;
            }
            
            // 激活GameView视图
            FocusWindowIfItsOpen(gameViewType);
            
            statistics = new GPUStatistics();
            statistics.AutoLoad();
            
            // 释放对象
            DestroyImmediate(dataSampleTarget);
            
            return statistics;
            
        }

#endregion


#endregion

#region CombineMeshAndSimplify

        /// <summary>
        /// 网格合并
        /// </summary>
        /// <param name="iTarget">目标信息</param>
        private void CombineMesh(ModelInfo iTarget)
        {
            // 显示进度条
            _progressCounter = ShowCombineMeshProgressBar(false);
            // 开始计数
            if(!_progressCounter.IsCounting) _progressCounter.StartCounter();
             
            // 记录开始时间
            var startDateTime = DateTime.Now.Ticks;
            
            // 默认导出目录
            var defaultExportDir = ConfData.GetExportDirByTab(ModelEditor._TAB_DEFAULT, iTarget.Path);
            if (!UtilsTools.CheckAndCreateDirByFullDir(defaultExportDir))
            {
                ShowDialog("错误", $"默认导出目录校验失败!\n Dir={defaultExportDir} \n 请确认磁盘空间或手动创建目录!");
                this.Error("CombineMesh():Export Dir Check Failed!!(Dir={0})", defaultExportDir);
                return;
            }
            
            // 取得导入设定信息
            var tabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT);
            var importerSettings =
                ConfData.GetSettingByTabIndex(tabIndex);
            if (null == importerSettings)
            {
                ShowDialog("错误", $"该平台尚未设定转换信息!\n(Platform:{ModelEditor.Tabs[tabIndex]})");
                this.Error("CombineMesh():Settings info invalid!!(tab={0})", ModelEditor.Tabs[tabIndex]);
                return;
            }
            
            // 合并网格到Default
            var defaultPrefab = ModelEditor.StartCombineToDefault(
                _progressCounter, iTarget, importerSettings, defaultExportDir, ShowDialog);
            if(null == defaultPrefab) return;
            var defaultGoName = UtilsTools.GetFileName(iTarget.Path, false);
            defaultPrefab.name = $"{defaultGoName}_default";
            
            // 释放内存
            UtilsAsset.ReleaseMemory(defaultPrefab, false);
            
            // 计算消耗时间
            iTarget.TotalCostTime = DateTime.Now.Ticks - startDateTime;
            iTarget.TotalCostTime = 0 >= iTarget.TotalCostTime ? 0 : iTarget.TotalCostTime;

            // 计数结束的话，清空计数条
            if (_progressCounter.isCountOver())
            {
                ProgressBarClear();
            }
            
            // 导出设定信息
            ExportToJsonFile();
            
            // 刷新
            UtilsAsset.AssetsRefresh();
            // 清空GC
            UtilsAsset.ClearGC();
        }

        /// <summary>
        /// 网格合并模型降面
        /// </summary>
        /// <param name="iTargets">目标列表</param>
        private void CombineMesh(ModelInfo[] iTargets)
        {
            // 取得导入设定信息
            var tabIndex = ModelEditor.GetTabIndexByTab(ModelEditor._TAB_DEFAULT);
            var importerSettings =
                ConfData.GetSettingByTabIndex(tabIndex);
            if (null == importerSettings)
            {
                ShowDialog("错误", $"该平台尚未设定转换信息!\n(Platform:{ModelEditor.Tabs[tabIndex]})");
                this.Error("CombineMesh():Settings info invalid!!(tab={0})", ModelEditor.Tabs[tabIndex]);
                return;
            }
            
            // 显示进度条
            _progressCounter = ShowCombineMeshProgressBar(false);
            // 开始计数
            if(!_progressCounter.IsCounting) _progressCounter.StartCounter();
            
            foreach (var target in iTargets)
            {
                if(!target.Checked) continue;
                
                // 记录开始时间
                var startDateTime = DateTime.Now.Ticks;
                
                // 默认导出目录
                var defaultExportDir = ConfData.GetExportDirByTab(ModelEditor._TAB_DEFAULT, target.Path);
                if (!UtilsTools.CheckAndCreateDirByFullDir(defaultExportDir))
                {
                    ShowDialog("错误", $"默认导出目录校验失败!\n Dir={defaultExportDir} \n 请确认磁盘空间或手动创建目录!");
                    this.Error("CombineMesh():Export Dir Check Failed!!(Dir={0})", defaultExportDir);
                    continue;
                }
                
                // 合并网格到Default
                var defaultPrefab = ModelEditor.StartCombineToDefault(
                    _progressCounter, target, importerSettings, defaultExportDir, ShowDialog);
                if(null == defaultPrefab) break;
                var defaultGoName = UtilsTools.GetFileName(target.Path, false);
                defaultPrefab.name = $"{defaultGoName}_default";
                
                // 释放
                UtilsAsset.ReleaseMemory(defaultPrefab, false);
                
                // 计算消耗时间
                target.TotalCostTime = DateTime.Now.Ticks - startDateTime;
                target.TotalCostTime = 0 >= target.TotalCostTime ? 0 : target.TotalCostTime;
                // 清空GC
                UtilsAsset.ClearGC();
            }

            // 计数结束的话，清空计数条
            if (_progressCounter.isCountOver())
            {
                ProgressBarClear();
            }
            
            // 导出设定信息
            ExportToJsonFile();
            
            // 刷新
            UtilsAsset.AssetsRefresh();
            // 清空GC
            UtilsAsset.ClearGC();
        }

        /// <summary>
        /// 网格合并模型降面
        /// </summary>
        /// <param name="iTarget">目标信息</param>
        /// <param name="iTabIndex">Tab索引</param>
        private void ModelSimplify(
            ModelInfo iTarget, int iTabIndex)
        {
            // 取得导入设定信息
            var importerSettings = ConfData.GetSettingByTabIndex(iTabIndex);
            if (null == importerSettings)
            {
                ShowDialog("错误", $"该平台尚未设定转换信息!\n(Platform:{ModelEditor.Tabs[iTabIndex]})");
                this.Error("CombineMesh():Settings info invalid!!(tab={0})", ModelEditor.Tabs[iTabIndex]);
                return;
            }
            
            // 加载Default的预制体
            var defaultGo = LoadPrefab(ModelEditor._TAB_DEFAULT, iTarget);
            if (null == defaultGo)
            {
                return;
            }
            
            // 显示进度条
            _progressCounter = ShowCombineMeshProgressBar(false);
            // 开始计数
            if(!_progressCounter.IsCounting) _progressCounter.StartCounter();
            // 降面
            if (!ModelEditor.StartSimplifyToOther(
                _progressCounter, defaultGo, importerSettings, iTarget, 
                ConfData.ExportDir, ShowDialog))
            {
                this.Error("ModelSimplify():Failed!!(Model:{0} Tab:{1} - {2}\n ImporterSettings:{3})", 
                    iTarget.ToString(), iTabIndex, ModelEditor.Tabs[iTabIndex], importerSettings); 
            }
            
            // 释放内存
            UtilsAsset.ReleaseMemory(defaultGo, false);
            
            // 计数结束的话，清空计数条
            if (_progressCounter.isCountOver())
            {
                ProgressBarClear();
            }
            
            // 导出设定信息
            ExportToJsonFile();
            
            // 刷新
            UtilsAsset.AssetsRefresh();
            // 清空GC
            UtilsAsset.ClearGC();
        }
        
        /// <summary>
        /// 网格模型批量降面
        /// </summary>
        /// <param name="iTargets">目标列表</param>
        /// <param name="iTabIndex">Tab索引</param>
        private void ModelSimplify(
            ModelInfo[] iTargets, int iTabIndex)
        {
            // 取得导入设定信息
            var importerSettings = ConfData.GetSettingByTabIndex(iTabIndex);
            if (null == importerSettings)
            {
                ShowDialog("错误", $"该平台尚未设定转换信息!\n(Platform:{ModelEditor.Tabs[iTabIndex]})");
                this.Error("CombineMesh():Settings info invalid!!(tab={0})", ModelEditor.Tabs[iTabIndex]);
                return;
            }
            
            // 显示进度条
            _progressCounter = ShowCombineMeshProgressBar(false);
            // 开始计数
            if(!_progressCounter.IsCounting) _progressCounter.StartCounter();
            
            foreach (var target in iTargets)
            {
                // 加载Default的预制体
                var defaultGo = LoadPrefab(ModelEditor._TAB_DEFAULT, target);
                if (null == defaultGo)
                {
                    return;
                }
                
                // 降面信息
                var simplify = target.GetSimplifyInfoByTabIndex(iTabIndex);
                if(null == simplify || !simplify.Checked) continue;
                
                // 合并网格
                if (!ModelEditor.StartSimplifyToOther(
                    _progressCounter, defaultGo, importerSettings,
                    target, ConfData.ExportDir, ShowDialog))
                {
                    this.Error("ModelSimplify():Simplify Failed!!(Target:{0})", target.ToString());
                }
                
                // 释放内存
                UtilsAsset.ReleaseMemory(defaultGo, false);
                // 清空GC
                UtilsAsset.ClearGC();
            }
            
            // 计数结束的话，清空计数条
            if (_progressCounter.isCountOver())
            {
                ProgressBarClear();
            }
            
            // 导出设定信息
            ExportToJsonFile();
            
            // 刷新
            UtilsAsset.AssetsRefresh();
            // 清空GC
            UtilsAsset.ClearGC();
        }

        /// <summary>
        /// 批量删除模型信息
        /// </summary>
        private void DeleteModels()
        {
            var ids = new List<int>();
            // 批量处理
            foreach (var model in ConfData.Models)
            {
                if(!model.Checked) continue;
                ids.Add(model.ID);
            }

            if (0 < ids.Count)
            {
                foreach (var id in ids)
                {
                    if(!ConfData.Models.Exists(iO => id == iO.ID)) continue;
                    var target = ConfData.Models.Where(iO => id == iO.ID).ToArray()[0];
                    ConfData.Models.Remove(target);
                }
                ids.Clear();
            }
        }
        
        /// <summary>
        /// 批量删除模型信息
        /// </summary>
        /// <param name="iTabIndex">Tab索引</param>
        private void DeleteModelSimplifies(int iTabIndex)
        {
            var ids = new List<int>();
            // 批量处理
            foreach (var model in ConfData.Models)
            {
                var simplify = model.GetSimplifyInfoByTabIndex(iTabIndex);
                if(null == simplify || !simplify.Checked) continue;
                ids.Add(model.ID);
            }

            if (0 < ids.Count)
            {
                foreach (var id in ids)
                {
                    ConfData.RemoveModelByIdAndTabIndex(id, iTabIndex);
                }
                ids.Clear();
            }
        }

#endregion

#region Prefab

        /// <summary>
        /// 再加预制体
        /// </summary>
        /// <param name="iTab">Tab</param>
        /// <param name="iTarget">模型目标信息</param>
        /// <returns>预制体GameObject</returns>
        private GameObject LoadPrefab(string iTab, ModelInfo iTarget)
        {
            // Default的预制体目录
            var defaultPrefabDir = ConfData.GetExportDirByTab(iTab, iTarget.Path);
            if (!Directory.Exists(defaultPrefabDir))
            {
                ShowDialog("错误", $"默认预制体数据目录不存在!\nDir:{defaultPrefabDir}");
                return null;
            }

            // Default的预制体路径
            var defaultPrefabPath = $"{defaultPrefabDir}/{UtilsTools.GetFileName(iTarget.Path)}";
            if (!File.Exists(defaultPrefabPath))
            {
                ShowDialog("错误", $"默认预制体不存在!\nDir:{defaultPrefabPath}");
                return null;
            }
            var defaultGo = ModelEditor.LoadModelPrefab(defaultPrefabPath);
            if (null != defaultGo) return defaultGo;
            ShowDialog("错误", $"预制体加载失败!\nDir:{defaultPrefabPath}");
            return null;
        }

#endregion
        
#region ShowWindow

        /// <summary>
        /// 显示宏定义窗口.
        /// </summary>
        [MenuItem("Tools/Model/网格编辑器", false, 800)]
        static void ShowWindow() {
            ShowWindow("网格编辑器");
        }

#endregion

    } 
}

