using UnityEngine;
using Packages.Common.Extend;

namespace Packages.Common.Base
{
    /// <summary>
    /// 单例模版类基类.
    /// </summary>
    public class SingletonBase<T> : ClassExtension where T : class, new()
    {
        protected static T _instance;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                }
                return _instance ?? default(T);
            }
        }

        /// <summary>
        /// 构造函数.
        /// </summary>
        protected SingletonBase()
        {
            _instance = this as T;
            Init();
        }

        /// <summary>
        /// 初始化.
        /// </summary>
        protected virtual void Init()
        {
            Info("Init()");
        }

        /// <summary>
        /// 初始化.
        /// </summary>
        public virtual void Reset()
        {
            Info("Reset()");
        }
    }
    
    public abstract class SingletonMonoBehaviour : MonoBehaviour
    {
        protected virtual void SingletonAwake() {}
        protected virtual void SingletonDestroy() {}

        public virtual void SingletonBeginLoadLevel(string iCurLevel, string iNextLevel, string iCurState) {}
        public virtual void SingletonEndLoadLevel(string iCurLevel, string iNextLevel, string iCurState) {}
        public virtual void SingletonOnMemoryWarning(float curMem, float devMem, float perc) {}
    }

    /// <summary>
    /// 单例脚本模版类基类.
    /// </summary>
    public class SingletonMonoBehaviourBase<T> : SingletonMonoBehaviour 
        where T : SingletonMonoBehaviourBase<T>
    {
        public static T Instance { get; private set; }

        public SingletonMonoBehaviourBase()
        {
            Initialized = false;
        }

        /// <summary>
        /// 初始化标志位.
        /// </summary>
        public bool Initialized { get; protected set; }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                if (Instance != null)
                    this.Error("Awake():duplicate singleton:{0}, current:{1}",
                        typeof(T), Instance.transform.GetInstanceID());
                Destroy(this);
                return;
            }
            Instance = this as T;
            SingletonAwake();
        }

        private void Start() {
            if (null == Instance) {
                this.Error("Start():The instance is invalid!!");
                return;
            }
            SingletonStart();
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this) return;
            SingletonDestroy();
            Instance = null;
        }
        
#region virtual

        protected virtual void SingletonStart()
        {
            this.Info("SingletonStart()");
        }

        protected virtual void SingletonAwake()
        {
            this.Info("SingletonAwake()");
        }
        protected virtual void SingletonDestroy()
        {
            this.Info("SingletonDestroy()");
        }

#endregion

    }
}
