
namespace Packages.Common.Base
{
    /// <summary>
    /// 管理器接口.
    /// </summary>
    public interface IManagerBase
    {
        /// <summary>
        /// 进入State函数
        /// </summary>
        /// <param name="iCurState">当前State</param>
        /// <param name="iNextState">下一个State</param>
        void OnStateEnter(string iCurState, string iNextState);
        
        /// <summary>
        /// 退出State函数
        /// </summary>
        /// <param name="iCurState">当前State</param>
        /// <param name="iNextState">下一个State</param>
        void OnStateExit(string iCurState, string iNextState);
        
        /// <summary>
        /// 内存警告函数
        /// </summary>
        /// <param name="iCurrMem">当前内存</param>
        /// <param name="iDevMem">硬件内存</param>
        /// <param name="iPerc"></param>
        void OnMemoryWarning(float iCurrMem, float iDevMem, float iPerc);

        /// <summary>
        /// 关闭函数
        /// </summary>
        void Close();
    }

    public abstract class ManagerBehaviourBase<T> : SingletonMonoBehaviourBase<T>, 
        IManagerBase where T : ManagerBehaviourBase<T>
    {

        /// <summary>
        /// 进入State函数
        /// </summary>
        /// <param name="iCurState">当前State</param>
        /// <param name="iNextState">下一个State</param>
        public virtual void OnStateEnter(string iCurState, string iNextState)
        {
        }

        /// <summary>
        /// 退出State函数
        /// </summary>
        /// <param name="iCurState">当前State</param>
        /// <param name="iNextState">下一个State</param>
        public virtual void OnStateExit(string iCurState, string iNextState)
        {
        }

        /// <summary>
        /// 内存警告函数
        /// </summary>
        /// <param name="iCurrMem">当前内存</param>
        /// <param name="iDevMem">硬件内存</param>
        /// <param name="iPerc"></param>
        public virtual void OnMemoryWarning(float iCurrMem, float iDevMem, float iPerc)
        {
        }

        /// <summary>
        /// 关闭函数
        /// </summary>
        public virtual void Close()
        {
            Destroy(gameObject);
        }
    }
}
