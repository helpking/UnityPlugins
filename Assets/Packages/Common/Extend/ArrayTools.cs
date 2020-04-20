using System;
using System.Collections.Generic;

namespace Packages.Common.Extend
{
	/// <summary>
	/// 数组工具类
	/// </summary>
	public static class ArrayTools 
	{
#region Array

			/// <summary>
			/// 查到目标对象
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iTarget">目标对象</param>
			/// <returns>目标对象在数组中的索引值(若不存在，则返回-1)</returns>
			public static int Find(this Array iArray, object iTarget)
			{
				for (var i=0; i<iArray.Length; i++)
					if (iArray.GetValue(i) == iTarget) return i;
				return -1;
			}

			/// <summary>
			/// 查到目标对象(泛型)
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iTarget">目标对象</param>
			/// <typeparam name="T">泛型类型</typeparam>
			/// <returns>目标对象在数组中的索引值(若不存在，则返回-1)</returns>
			public static int Find<T> (T[] iArray, T iTarget) where T : class
			{
				for (var i=0; i<iArray.Length; i++)
					if (Equals(iArray[i], iTarget)) return i;
				return -1;
			}

			/// <summary>
			/// 查到目标对象(泛型)
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iTarget">目标对象</param>
			/// <typeparam name="T">泛型类型</typeparam>
			/// <returns>目标对象在数组中的索引值(若不存在，则返回-1)</returns>
			public static int FindEquatable<T> (T[] iArray, T iTarget) where T : IEquatable<T>
			{
				for (var i=0; i<iArray.Length; i++)
					if (Equals(iArray[i], iTarget)) return i;
				return -1;
			}
			
			/// <summary>
			/// 移除对象
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iIndex">移除对象的索引值</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>移除后的新数组(新数组会重新开辟内存)</returns>
			public static void RemoveAt<T> (ref T[] iArray, int iIndex) { iArray = RemoveAt(iArray, iIndex); }
			
			/// <summary>
			/// 移除对象
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iIndex">移除对象的索引值</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>移除后的新数组(新数组会重新开辟内存)</returns>
			public static T[] RemoveAt<T> (T[] iArray, int iIndex)
			{
				var newArray = new T[iArray.Length-1];
				for (var i=0; i<newArray.Length; i++) 
				{
					if (i<iIndex) newArray[i] = iArray[i];
					else newArray[i] = iArray[i+1];
				}
				return newArray;
			}

			/// <summary>
			/// 移除对象
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iTarget">移除对象</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>移除后的新数组(新数组会重新开辟内存)</returns>
			public static void Remove<T> (ref T[] iArray, T iTarget) where T : class  {iArray = Remove(iArray, iTarget); }

			/// <summary>
			/// 移除对象
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iTarget">移除对象</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>移除后的新数组(新数组会重新开辟内存)</returns>
			private static T[] Remove<T> (T[] iArray, T iTarget) where T : class
			{
				var index = Find<T>(iArray, iTarget);
				return RemoveAt<T>(iArray,index);
			}
			
			/// <summary>
			/// 添加对象
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iCreateElement">创建元素方法</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>添加后的数组(新数组会重新开辟内存)</returns>
			public static void Add<T> (ref T[] iArray, Func<T> iCreateElement=null) { iArray = Add(iArray, iCreateElement:iCreateElement); }
			
			/// <summary>
			/// 添加对象
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iCreateElement">创建元素方法</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>添加后的数组(新数组会重新开辟内存)</returns>
			public static T[] Add<T> (T[] iArray, Func<T> iCreateElement = null)
			{
				if (iArray==null || iArray.Length==0)
				{
					return iCreateElement != null ? new T[] {iCreateElement()} : new T[] {default(T)};
				}

				var newArray = new T[iArray.Length+1];
				for (var i=0; i<iArray.Length; i++) 
					newArray[i] = iArray[i];
				
				if (iCreateElement != null) newArray[iArray.Length] = iCreateElement();
				else newArray[iArray.Length] = default(T);
				
				return newArray;
			}

			/// <summary>
			/// 插入数组 - 单个
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iIndex">欲插入目标数组的索引值</param>
			/// <param name="iCreateElement">创建元素方法</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>插入后的数组(新数组会重新开辟内存)</returns>
			public static void Insert<T> (ref T[] iArray, int iIndex, Func<T> iCreateElement=null) { iArray = Insert(iArray, iIndex, iCreateElement:iCreateElement); }
			
			/// <summary>
			/// 插入数组 - 单个
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iIndex">欲插入目标数组的索引值</param>
			/// <param name="iCreateElement">创建元素方法</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>插入后的数组(新数组会重新开辟内存)</returns>
			public static T[] Insert<T> (T[] iArray, int iIndex, Func<T> iCreateElement=null)
			{
				if (iArray==null || iArray.Length==0)
				{
					return iCreateElement != null ? new T[] {iCreateElement()} : new T[] {default(T)};
				}
				if (iIndex > iArray.Length || iIndex < 0) iIndex = iArray.Length;
				
				var newArray = new T[iArray.Length+1];
				for (var i=0; i<newArray.Length; i++) 
				{
					if (i<iIndex) newArray[i] = iArray[i];
					else if (i == iIndex) 
					{
						if (iCreateElement != null) newArray[i] = iCreateElement();
						else newArray[i] = default(T);
					}
					else newArray[i] = iArray[i-1];
				}
				return newArray;
			}

			/// <summary>
			/// 插入数组 - 多个
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iAfterIndex">欲插入数组的目标索引值(插入的新元素都会在它之后)</param>
			/// <param name="iAdds">欲插入新数组</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>插入后的数组(新数组会重新开辟内存)</returns>
			public static T[] InsertRange<T> (T[] iArray, int iAfterIndex, T[] iAdds)
			{
				if (iArray==null || iArray.Length==0) { return iAdds; }
				if (iAfterIndex > iArray.Length || iAfterIndex<0) iAfterIndex = iArray.Length;
				
				var newArray = new T[iArray.Length+iAdds.Length];
				for (var i=0; i<newArray.Length; i++) 
				{
					if (i<iAfterIndex) newArray[i] = iArray[i];
					else if (i == iAfterIndex) 
					{
						for (var j=0; j<iAdds.Length; j++)
							newArray[i+j] = iAdds[j];
						i+= iAdds.Length-1;
					}
					else newArray[i] = iArray[i-iAdds.Length];
				}
				return newArray;
			}

			/// <summary>
			/// 重置数组大小
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iNewSize">新大小</param>
			/// <param name="iCreateElement">创建元素方法(参数:数组索引值)</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>重置大小后的数组(新数组会重新开辟内存)</returns>
			public static void Resize<T> (ref T[] iArray, int iNewSize, Func<int,T> iCreateElement=null) { iArray = Resize(iArray, iNewSize, iCreateElement); }

			/// <summary>
			/// 重置数组大小
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iNewSize">新大小</param>
			/// <param name="iCreateElement">创建元素方法(参数:数组索引值)</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>重置大小后的数组(新数组会重新开辟内存)</returns>
			private static T[] Resize<T> (T[] iArray, int iNewSize, Func<int,T> iCreateElement=null)
			{
				if (iArray.Length == iNewSize) return iArray;

				var newArray = new T[iNewSize];
					
				var min = iNewSize<iArray.Length? iNewSize : iArray.Length;
				for (var i=0; i<min; i++)
					newArray[i] = iArray[i];

				if (iNewSize <= iArray.Length || iCreateElement == null) return newArray;
				{
					for (var i=iArray.Length; i<iNewSize; i++)
						newArray[i] = iCreateElement(i);
				}

				return newArray;
			}

			/// <summary>
			/// 链接数组
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iAppends">链接数组</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>链接后的数组(新数组会重新开辟内存)</returns>
			public static void Append<T> (ref T[] iArray, T[] iAppends) { iArray = Append(iArray, iAppends); }
			
			/// <summary>
			/// 链接数组
			/// </summary>
			/// <param name="iArray">源数组</param>
			/// <param name="iAppends">链接数组</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>链接后的数组(新数组会重新开辟内存)</returns>
			public static T[] Append<T> (T[] iArray, T[] iAppends)
			{
				var newArray = new T[iArray.Length+iAppends.Length];
				for (var i=0; i<iArray.Length; i++) { newArray[i] = iArray[i]; }
				for (var i=0; i<iAppends.Length; i++) { newArray[i+iArray.Length] = iAppends[i]; }
				return newArray;
			}

			/// <summary>
			/// 将数组中指定索引值的两个元素置换
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iIndex1">第一个元素的索引值</param>
			/// <param name="iIndex2">第二个元素的索引值</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			public static void Swap<T> (T[] iArray, int iIndex1, int iIndex2)
			{
				if (iIndex1<0 || iIndex1>=iArray.Length || iIndex2<0 || iIndex2 >=iArray.Length) return;
				
				var temp = iArray[iIndex1];
				iArray[iIndex1] = iArray[iIndex2];
				iArray[iIndex2] = temp;
			}

			/// <summary>
			/// 将数组中的两个对象置换
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iTarget1">第一个目标</param>
			/// <param name="iTarget2">第二个目标</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			public static void Swap<T> (T[] iArray, T iTarget1, T iTarget2) where T : class
			{
				var index1 = Find<T>(iArray, iTarget1);
				var index2 = Find<T>(iArray, iTarget2);
				Swap<T>(iArray, index1, index2);
			}

			/// <summary>
			/// 缩减数组
			/// </summary>
			/// <param name="iSrc">源素组</param>
			/// <param name="iLength">缩减目标长度</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>缩减后的数组(新数组会重新开辟内存)</returns>
			public static T[] Truncated<T> (this T[] iSrc, int iLength)
			{
				var dst = new T[iLength];
				for (var i=0; i<iLength; i++) dst[i] = iSrc[i];
				return dst;
			}

			/// <summary>
			/// 比较两个数组
			/// </summary>
			/// <param name="iArray1">数组1</param>
			/// <param name="iArray2">数组2</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>true:相同; false:不相同;</returns>
			public static bool Equals<T> (T[] iArray1, T[] iArray2) where T : class
			{
				if (iArray1.Length != iArray2.Length) return false;
				for (var i=0; i<iArray1.Length; i++)
					if (iArray1[i] != iArray2[i]) return false;
				return true;
			}

			/// <summary>
			/// 比较两个数组
			/// </summary>
			/// <param name="iArray1">数组1</param>
			/// <param name="iArray2">数组2</param>
			/// <typeparam name="T">数组数据类型</typeparam>
			/// <returns>true:相同; false:不相同;</returns>
			public static bool EqualsEquatable<T> (T[] iArray1, T[] iArray2) where T : IEquatable<T>
			{
				if (iArray1.Length != iArray2.Length) return false;
				for (int i=0; i<iArray1.Length; i++)
					if (!Equals(iArray1[i],iArray2[i])) return false;
				return true;
			}

			/// <summary>
			/// 比较两个Vector3类型的数组
			/// </summary>
			/// <param name="iArray1">数组1</param>
			/// <param name="iArray2">数组2</param>
			/// <param name="iDelta">允许的变化误差(x,y,z。3个方向上允许的误差范围)</param>
			/// <returns>true:相同; false:不相同;</returns>
			public static bool EqualsVector3 (
				UnityEngine.Vector3[] iArray1, UnityEngine.Vector3[] iArray2, 
				float iDelta = float.Epsilon)
			{
				if (iArray1==null || iArray2==null || iArray1.Length != iArray2.Length) return false;
				for (var i=0; i<iArray1.Length; i++)
				{
					var dist = iArray1[i].x-iArray2[i].x;
					if (!(dist<iDelta && -dist<iDelta)) return false;

					dist = iArray1[i].y-iArray2[i].y;
					if (!(dist<iDelta && -dist<iDelta)) return false;

					dist = iArray1[i].z-iArray2[i].z;
					if (!(dist<iDelta && -dist<iDelta)) return false;
				}
				return true;
			}

#endregion

#region Array Sorting

			/// <summary>
			/// 快速排序法
			/// </summary>
			/// <param name="iArray">目标数组</param>
			public static void QSort (float[] iArray) { QSort(iArray, 0, iArray.Length-1); }
			
			/// <summary>
			/// 快速排序法
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iEndIndex">结束索引</param>
			public static void QSort (
				float[] iArray, int iEndIndex = -1)
			{
				QSort(iArray, 0, iEndIndex);
			}

			/// <summary>
			/// 快速排序法
			/// </summary>
			/// <param name="iArray">目标数组</param>
			/// <param name="iStartIndex">开始索引</param>
			/// <param name="iEndIndex">结束索引</param>
			public static void QSort (
				float[] iArray, int iStartIndex = -1, int iEndIndex = -1)
			{
				var startIndex = 0 > iStartIndex ? 0 : iStartIndex;
				var endIndex = 0 > iEndIndex ? iArray.Length -1 : iEndIndex;
				var mid = iArray[startIndex + (endIndex-startIndex) / 2]; //(l+r)/2
				var i = startIndex;
				var j = endIndex;
				
				while (i <= j)
				{
					while (iArray[i] < mid) i++;
					while (iArray[j] > mid) j--;
					if (i > j) continue;
					var temp = iArray[i];
					iArray[i] = iArray[j];
					iArray[j] = temp;
						
					i++; j--;
				}
				if (i < endIndex) QSort(iArray, i, endIndex);
				if (startIndex < j) QSort(iArray, startIndex, j);
			}

			static public void QSort<T> (T[] array, float[] reference) { QSort(array, reference, 0, reference.Length-1); }
			static public void QSort<T> (T[] array, float[] reference, int l, int r)
			{
				float mid = reference[l + (r-l) / 2]; //(l+r)/2
				int i = l;
				int j = r;
				
				while (i <= j)
				{
					while (reference[i] < mid) i++;
					while (reference[j] > mid) j--;
					if (i <= j)
					{
						float temp = reference[i];
						reference[i] = reference[j];
						reference[j] = temp;

						T tempT = array[i];
						array[i] = array[j];
						array[j] = tempT;
						
						i++; j--;
					}
				}
				if (i < r) QSort(array, reference, i, r);
				if (l < j) QSort(array, reference, l, j);
			}

			static public void QSort<T> (List<T> list, float[] reference) { QSort(list, reference, 0, reference.Length-1); }
			static public void QSort<T> (List<T> list, float[] reference, int l, int r)
			{
				float mid = reference[l + (r-l) / 2]; //(l+r)/2
				int i = l;
				int j = r;
				
				while (i <= j)
				{
					while (reference[i] < mid) i++;
					while (reference[j] > mid) j--;
					if (i <= j)
					{
						float temp = reference[i];
						reference[i] = reference[j];
						reference[j] = temp;

						T tempT = list[i];
						list[i] = list[j];
						list[j] = tempT;
						
						i++; j--;
					}
				}
				if (i < r) QSort(list, reference, i, r);
				if (l < j) QSort(list, reference, l, j);
			}

			static public int[] Order (int[] array, int[] order=null, int max=0, int steps=1000000, int[] stepsArray=null) //returns an order int array
			{
				if (max==0) max=array.Length;
				if (stepsArray==null) stepsArray = new int[steps+1];
				else steps = stepsArray.Length-1;
			
				//creating starts array
				int[] starts = new int[steps+1];
				for (int i=0; i<max; i++) starts[ array[i] ]++;
					
				//making starts absolute
				int prev = 0;
				for (int i=0; i<starts.Length; i++)
					{ starts[i] += prev; prev = starts[i]; }

				//shifting starts
				for (int i=starts.Length-1; i>0; i--)
					{ starts[i] = starts[i-1]; }  
				starts[0] = 0;

				//using magic to compile order
				if (order==null) order = new int[max];
				for (int i=0; i<max; i++)
				{
					int h = array[i]; //aka height
					int num = starts[h];
					order[num] = i;
					starts[h]++;
				}
				return order;
			}

			static public T[] Convert<T,Y> (Y[] src)
			{
				T[] result = new T[src.Length];
				for (int i=0; i<src.Length; i++) result[i] = (T)(object)(src[i]);
				return result;
			}

		#endregion
	}

}
