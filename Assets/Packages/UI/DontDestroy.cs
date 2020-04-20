using Packages.Common.Base;
using UnityEngine;

namespace Packages.UI
{
    /// <summary>
    /// 不释放当前脚本
    /// </summary>
    [AddComponentMenu("Packages/UI/DontDestroy")]
    public class DontDestroy : MonoBehaviour
    {
        // Use this for initialization
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
