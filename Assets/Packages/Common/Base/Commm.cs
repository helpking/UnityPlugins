using System;
using System.Collections.Generic;
using System.Linq;

namespace Packages.Common.Base
{
    /// <summary>
    /// 枚举体表示用泛型类
    /// </summary>
    /// <typeparam name="T">枚举体类型</typeparam>
    public class EnumDisplay<T> : JsonDataBase<EnumDisplay<T>>
        where T : Enum
    {
        public EnumDisplay() {}
        
        /// <summary>
        /// 值
        /// </summary>
        public T Value;

        /// <summary>
        /// 表示用文本
        /// </summary>
        public string Text;
    }

    /// <summary>
    /// 枚举体表示用列表
    /// </summary>
    public class EnumDisplayList<T> : JsonDataBase<EnumDisplayList<T>>
        where T : Enum
    {
        /// <summary>
        /// 格式列表
        /// </summary>
        public List<EnumDisplay<T>> List = new List<EnumDisplay<T>>();
        
        /// <summary>
        /// 取得显示用的文本列表
        /// </summary>
        /// <returns>显示用的文本列表</returns>
        public List<string> GetDisplayTextList()
        {
            var list = new List<string>();
                
            // 排序
            List.Sort((iX, iY) => 
                string.Compare(iX.Value.ToString(), iY.Value.ToString(), StringComparison.Ordinal));

            for (var idx = 0; idx < List.Count; ++idx)
            {
                list.Add(List[idx].Text);
            }
                
            return list;
        }
    }

    
    /// <summary>
    /// 枚举体表示用列表借口
    /// </summary>
    public interface IEnumDisplayList<T>
        where T : Enum
    {
        /// <summary>
        /// 取得显示用文本列表
        /// </summary>
        /// <returns>文本列表</returns>
        List<string> GetDisplayTexts();

        /// <summary>
        /// 取得指定值在列表中对应的索引值
        /// </summary>
        /// <param name="iValue">值</param>
        /// <returns>索引值</returns>
        int GetIndexByValue(T iValue);

        /// <summary>
        /// 取得列表中指定索引值对应的值
        /// </summary>
        /// <param name="iIndex">索引值</param>
        /// <returns>值</returns>
        T GetValueByIndex(int iIndex);
    }
}
