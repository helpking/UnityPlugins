using System.Text;
using UnityEditor;
using UnityEngine;

namespace Packages.GUIExtend
{
    
    /// <summary>
    /// 显示GPU统计信息
    /// </summary>
    [AddComponentMenu("Packages/GUIExtend/ShowGPUStatisticsInfo")]
    public class ShowGPUStatisticsInfo : MonoBehaviour
    {

        public bool m_isShow = true;

        private int m_FrameCounter;
        private float m_ClientTimeAccumulator;
        private float m_RenderTimeAccumulator;
        private float m_MaxTimeAccumulator;
        private float m_ClientFrameTime;
        private float m_RenderFrameTime;
        private float m_MaxFrameTime;
        private GUIStyle s_SectionHeaderStyle;
        private GUIStyle s_LabelStyle;
        [Range(0, 1)]
        public float position = 0.5f;
        

        void OnGUI()
        {
            if (m_isShow)
            {
                GameViewStatsGUI();
            }

        }

        private GUIStyle sectionHeaderStyle =>
            s_SectionHeaderStyle ?? (s_SectionHeaderStyle =
                EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).GetStyle("BoldLabel"));

        private GUIStyle labelStyle
        {
            get
            {
                if (s_LabelStyle != null) return s_LabelStyle;
                s_LabelStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).label) {richText = true};
                return s_LabelStyle;
            }
        }
        private string FormatNumber(int iNum)
        {
            if (iNum < 1000)
            {
                return iNum.ToString();
            }
            if (iNum < 1000000)
            {
                return ((double)iNum * 0.001).ToString("f1") + "k";
            }
            return ((double)iNum * 1E-06).ToString("f1") + "M";
        }
        public void UpdateFrameTime()
        {
            var frameTime = UnityStats.frameTime;
            var renderTime = UnityStats.renderTime;
            m_ClientTimeAccumulator += frameTime;
            m_RenderTimeAccumulator += renderTime;
            m_MaxTimeAccumulator += Mathf.Max(frameTime, renderTime);
            m_FrameCounter++;
            var flag = m_ClientFrameTime == 0f && m_RenderFrameTime == 0f;
            var flag2 = m_FrameCounter > 30 || m_ClientTimeAccumulator > 0.3f || m_RenderTimeAccumulator > 0.3f;
            if (flag || flag2)
            {
                m_ClientFrameTime = m_ClientTimeAccumulator / (float)m_FrameCounter;
                m_RenderFrameTime = m_RenderTimeAccumulator / (float)m_FrameCounter;
                m_MaxFrameTime = m_MaxTimeAccumulator / (float)m_FrameCounter;
            }
            if (flag2)
            {
                m_ClientTimeAccumulator = 0f;
                m_RenderTimeAccumulator = 0f;
                m_MaxTimeAccumulator = 0f;
                m_FrameCounter = 0;
            }
        }
        private static string FormatDb(float iVol)
        {
            return iVol == 0.0f ? "-∞ dB" : $"{20f * Mathf.Log10(iVol):F1} dB";
        }
        public void GameViewStatsGUI()
        {
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
            GUI.color = new Color(1f, 1f, 1f, 0.75f);
            var num = 300.0f;
            var num2 = 204.0f;
            // int num3 = Network.connections.Length;
            // if (num3 != 0)
            // {
            //     num2 += 220f;
            // }
            GUILayout.BeginArea(new Rect(Screen.width * position - num - 10f, 27f, num, num2), "Statistics", GUI.skin.window);
            GUILayout.Label("Audio:", sectionHeaderStyle, new GUILayoutOption[0]);
            var stringBuilder = new StringBuilder(400);
            var audioLevel = UnityStats.audioLevel;
            stringBuilder.Append($"  Level: " + FormatDb(audioLevel) + ((!EditorUtility.audioMasterMute) ? "\n" : " (MUTED)\n"));
            stringBuilder.Append($"  Clipping: {100f * UnityStats.audioClippingAmount:F1}%");
            GUILayout.Label(stringBuilder.ToString(), new GUILayoutOption[0]);
            GUI.Label(new Rect(170f, 40f, 120f, 20f), 
                $"DSP load: {100f * UnityStats.audioDSPLoad:F1}%");
            GUI.Label(new Rect(170f, 53f, 120f, 20f), 
                $"Stream load: {100f * UnityStats.audioStreamLoad:F1}%");
            GUILayout.Label("Graphics:", sectionHeaderStyle, new GUILayoutOption[0]);
            UpdateFrameTime();
            var text = $"{1f / Mathf.Max(m_MaxFrameTime, 1E-05f):F1} FPS ({m_MaxFrameTime * 1000f:F1}ms)";
            GUI.Label(new Rect(170f, 75f, 120f, 20f), text);
            var screenBytes = UnityStats.screenBytes;
            var num4 = UnityStats.dynamicBatchedDrawCalls - UnityStats.dynamicBatches;
            var num5 = UnityStats.staticBatchedDrawCalls - UnityStats.staticBatches;
            var stringBuilder2 = new StringBuilder(400);
            if (m_ClientFrameTime > m_RenderFrameTime)
            {
                stringBuilder2.Append(
                    $"  CPU: main <b>{m_ClientFrameTime * 1000f:F1}</b>ms  render thread {m_RenderFrameTime * 1000f:F1}ms\n");
            }
            else
            {
                stringBuilder2.Append(
                    $"  CPU: main {m_ClientFrameTime * 1000f:F1}ms  render thread <b>{m_RenderFrameTime * 1000f:F1}</b>ms\n");
            }
            stringBuilder2.Append($"  Batches: <b>{UnityStats.batches}</b> \tSaved by batching: {num4 + num5}\n");
            stringBuilder2.Append(
                $"  Tris: {FormatNumber(UnityStats.triangles)} \tVerts: {FormatNumber(UnityStats.vertices)} \n");
            stringBuilder2.Append($"  Screen: {UnityStats.screenRes} - {EditorUtility.FormatBytes(screenBytes)}\n");
            stringBuilder2.Append(
                $"  SetPass calls: {UnityStats.setPassCalls} \tShadow casters: {UnityStats.shadowCasters} \n");
            stringBuilder2.Append(
                $"  Visible skinned meshes: {UnityStats.visibleSkinnedMeshes}  Animations: {UnityStats.visibleAnimations}");
            GUILayout.Label(stringBuilder2.ToString(), labelStyle, new GUILayoutOption[0]);
            // if (num3 != 0)
            // {
            //     GUILayout.Label("Network:", sectionHeaderStyle, new GUILayoutOption[0]);
            //     GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            //     for (int i = 0; i < num3; i++)
            //     {
            //         GUILayout.Label(UnityStats.GetNetworkStats(i), new GUILayoutOption[0]);
            //     }
            //     GUILayout.EndHorizontal();
            // }
            // else
            // {
            //     GUILayout.Label("Network: (no players connected)", sectionHeaderStyle, new GUILayoutOption[0]);
            // }
            GUILayout.EndArea();
        }
    }
}

