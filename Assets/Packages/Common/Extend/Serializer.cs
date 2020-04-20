using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Packages.Common.Base;
using Packages.GUIExtend.Extend;
using UnityEngine;

namespace Packages.Common.Extend
{
	[Serializable]
	public class Serializer : ClassExtension 
	{
		/// <summary>
		/// 基本值
		/// </summary>
		[Serializable] public struct BaseValue { public object val; public string name; public bool property; }
		
		/// <summary>
		/// 对象值
		/// </summary>
		[Serializable] public struct ObjectValue { public int link; public string name; public bool property; }
		
		/// <summary>
		/// 布尔值
		/// </summary>
		[Serializable] public struct BoolValue { public bool val; public string name; public bool property; }
		
		/// <summary>
		/// 整形值
		/// </summary>
		[Serializable] public struct IntValue { public int val; public string name; public bool property; }
		
		/// <summary>
		/// 单精度浮点值
		/// </summary>
		[Serializable] public struct FloatValue { public float val; public string name; public bool property; }
		
		/// <summary>
		/// 单精度数组值
		/// </summary>
		[Serializable] public struct FloatArray { public float[] val; public string name; public bool property; }
		
		/// <summary>
		/// 字符串值
		/// </summary>
		[Serializable] public struct StringValue { public string val; public string name; public bool property; }
		
		/// <summary>
		/// 字符值
		/// </summary>
		[Serializable] public struct CharValue { public char val; public string name; public bool property; }
		
		/// <summary>
		/// 矩形值
		/// </summary>
		[Serializable] public struct RectValue { public Rect val; public string name; public bool property; }
		
		/// <summary>
		/// Unity对象值
		/// </summary>
		[Serializable] public struct UnityObjValue { public UnityEngine.Object val; public string name; public bool property; }
	
		/// <summary>
		/// 序列化对象
		/// </summary>
		[Serializable]
		public class SerializedObject
		{
			/// <summary>
			/// 序列化的目标对象
			/// </summary>
			public object obj; //to skip already added objs
			
			/// <summary>
			/// 类型名<BR/>
			///  备注：值与Type的AssemblyQualifiedName属性相同<BR/>
			///   如：Anytao.Learning.ExpressionTree.One, Anytao.Learning.ExpressionTree, Version=1.0. 0.0, Culture=neutral, PublicKeyToken=null 
			/// </summary>
			public string typeName;
			public List<ObjectValue> links = new List<ObjectValue>();

			public List<BoolValue> bools = new List<BoolValue>();
			public List<IntValue> ints = new List<IntValue>();
			public List<FloatValue> floats = new List<FloatValue>();
			public List<StringValue> strings = new List<StringValue>();
			public List<CharValue> chars = new List<CharValue>();
			public List<FloatArray> floatArrays = new List<FloatArray>();
			public List<RectValue> rects = new List<RectValue>();
			public List<UnityObjValue> unityObjs = new List<UnityObjValue>();

			/// <summary>
			/// 根据类型取得列表
			/// </summary>
			/// <param name="iType">类型</param>
			/// <returns>列表</returns>
			public IList GetListByType (Type iType)
			{
				if (iType == typeof(bool)) return bools;
				if (iType == typeof(int)) return ints;
				return links;
			}

			/// <summary>
			/// 添加值
			/// </summary>
			/// <param name="iType">类型</param>
			/// <param name="iValue">值</param>
			/// <param name="iName">名字</param>
			/// <param name="iSer">序列化容器</param>
			public void AddValue (Type iType, object iValue, string iName, Serializer iSer) //serializer needed to store obj links
			{
				//if (val == null) links.Add( new ObjectValue() {link=-1, name=name } );
				if (iType == typeof(bool))
				{
					bools.Add( new BoolValue { val = (bool)iValue, name = iName } );
				}
				else if (iType == typeof(int))
				{
					ints.Add( new IntValue { val = (int)iValue, name = iName } );
				}
				else if (iType == typeof(float))
				{
					floats.Add( new FloatValue { val = (float)iValue, name = iName } );
				}
				else if (iType == typeof(string))
				{
					strings.Add( new StringValue { val = (string)iValue, name = iName, property = false } );
				}
				else if (iType == typeof(char))
				{
					chars.Add( new CharValue { val = (char)iValue, name = iName, property = false } );
				}
				else if (iType == typeof(Rect))
				{
					rects.Add( new RectValue { val = (Rect)iValue, name = iName, property = false } );
				}
				else if (iType.IsSubclassOf(typeof(UnityEngine.Object)))
				{
					unityObjs.Add( new UnityObjValue { val = (UnityEngine.Object)iValue, name = iName, property = false } );
				}
				else if (iType == typeof(float[]))
				{
					floatArrays.Add( new FloatArray { val = (float[])iValue, name = iName, property = false } );
				}
				// 动画曲线
				else if (iType == typeof(AnimationCurve))
				{
					var link = iSer.Store(iValue);
					iSer.entities[link].AddValue(typeof(Keyframe[]), ((AnimationCurve)iValue).keys, "keys", iSer);
					links.Add( new ObjectValue { link = iSer.Store(iValue), name = iName } );
				}
				// 关键帧
				else if (iType == typeof(Keyframe))
				{
					var link = iSer.Store(iValue);
					
					var tval = (Keyframe)iValue;
					iSer.entities[link].AddValue(typeof(float), tval.time, "time", iSer);
					iSer.entities[link].AddValue(typeof(float), tval.value, "value", iSer);
					iSer.entities[link].AddValue(typeof(float), tval.inTangent, "inTangent", iSer);
					iSer.entities[link].AddValue(typeof(float), tval.outTangent, "outTangent", iSer);

					links.Add( new ObjectValue() { link = iSer.Store(iValue), name = iName} );
				}
				// 矩阵
				else if (iType == typeof(Matrix) ||
				         iType == typeof(CoordRect) ||
				         iType == typeof(Coord))
				{
					links.Add( new ObjectValue { link = iSer.Store(iValue, iWriteProperties:false), name = iName} );
				}
				else
				{
					links.Add( new ObjectValue { link = iSer.Store(iValue), name = iName } );
				}
			}

			/// <summary>
			/// 追加值
			/// </summary>
			/// <param name="iType">类型</param>
			/// <param name="iArray">值数组</param>
			/// <param name="iSer">序列化容器</param>
			public void AddValues (Type iType, Array iArray, Serializer iSer) //serializer needed to store obj links
			{
				if (iType == typeof(bool))
				{
					for (var i = 0; i < iArray.Length; i++)
					{
						bools.Add( new BoolValue { val = (bool)iArray.GetValue(i) } );
					}
				}
				else if (iType == typeof(int))
				{
					for (var i = 0; i < iArray.Length; i++)
					{
						ints.Add( new IntValue { val = (int)iArray.GetValue(i) } );
					}
				}
				else if (iType == typeof(float))
				{
					for (var i = 0; i < iArray.Length; i++)
					{
						floats.Add( new FloatValue { val = (float)iArray.GetValue(i) } );
					}
				}
				else 
				{
					for (var i = 0; i < iArray.Length; i++) 
						AddValue(iType, iArray.GetValue(i), "", iSer); 
				}
			}

			/// <summary>
			/// 取值
			/// </summary>
			/// <param name="iType">类型</param>
			/// <param name="iName">名字</param>
			/// <param name="iSer">序列化容器</param>
			/// <returns>值</returns>
			public object GetValue (Type iType, string iName, Serializer iSer)
			{
				if (iType == typeof(bool))
				{
					for (var i = 0; i < bools.Count; i++)
					{
						if(bools[i].name == iName)  
							return bools[i].val;
					}
				}
				else if (iType == typeof(int))
				{
					for (var i = 0; i < ints.Count; i++)
					{
						if(ints[i].name == iName)  
							return ints[i].val;
					}
				}
				else if (iType == typeof(float))
				{
					for (var i = 0; i < floats.Count; i++)
					{
						if(floats[i].name == iName)  
							return floats[i].val;
					}
				}
				else if (iType == typeof(string))
				{
					for (var i = 0; i < strings.Count; i++)
					{
						if (strings[i].name == iName)
							return strings[i].val;
					}
				}
				else if (iType == typeof(char))
				{
					for (var i = 0; i < chars.Count; i++)
					{
						if(chars[i].name==iName)  
							return chars[i].val;
					}
				}
				else if (iType == typeof(Rect))
				{
					for (var i = 0; i < rects.Count; i++)
					{
						if(rects[i].name==iName)  
							return rects[i].val;
					}
				}
				else if(iType.IsSubclassOf(typeof(UnityEngine.Object)))  
				{ 
					for(var i = 0; i < unityObjs.Count; i++)  
						if(unityObjs[i].name == iName)
						{
							try
							{
								if (unityObjs[i].val.GetType() == typeof(UnityEngine.Object)) 
									return null;
							}//else if (unityObjs[i].val.GetInstanceID() == 0) return null; 
							catch { return null; }
							return unityObjs[i].val;
						}   
				}
				else if (iType == typeof(float[]))
				{
					for (var i = 0; i < floatArrays.Count; i++)
					{
						if(floatArrays[i].name==iName)  
							return floatArrays[i].val;
					}
				}
				else
				{
					for (var i = 0; i < links.Count; i++)
					{
						if(links[i].name == iName)  
							return iSer.Retrieve(links[i].link);
					}
				}

				return null;
			}

			/// <summary>
			/// 根据类型取得相应的值数组
			/// </summary>
			/// <param name="iElementType">类型</param>
			/// <param name="iSer">序列化容器</param>
			/// <returns>相应的值数组</returns>
			public Array GetValues (Type iElementType, Serializer iSer)
			{
				var list = GetListByType(iElementType);
				var array = Array.CreateInstance(iElementType, list.Count);

				if (iElementType == typeof(bool))
				{
					for(var i = 0; i < bools.Count; i++) 
						array.SetValue(bools[i].val, i);
				}
				else if (iElementType == typeof(int))
				{
					for(var i = 0; i < ints.Count; i++) 
						array.SetValue(ints[i].val, i);
				}
				else if (iElementType == typeof(float))
				{
					for(var i=0;i<floats.Count;i++) 
						array.SetValue(floats[i].val, i);
				}
				else if (iElementType == typeof(string))
				{
					for(var i=0;i<strings.Count;i++) 
						array.SetValue(strings[i].val, i);
				}
				else if (iElementType == typeof(char))
				{
					for(var i=0;i<chars.Count;i++) 
						array.SetValue(chars[i].val, i);
				}
				else if (iElementType == typeof(Rect))
				{
					for(var i=0;i<rects.Count;i++) 
						array.SetValue(rects[i].val, i);
				}
				else if (iElementType.IsSubclassOf(typeof(UnityEngine.Object)))
				{
					for(var i=0;i<unityObjs.Count;i++) 
						array.SetValue(unityObjs[i].val, i);
				}
				else if (iElementType == typeof(float[]))
				{
					for(var i=0;i<floatArrays.Count;i++) 
						array.SetValue(floatArrays[i].val, i);
				}
				else
				{
					for(var i=0;i<links.Count;i++) 
						array.SetValue(iSer.Retrieve(links[i].link), i);
				}
				return array;
			}

			/// <summary>
			/// 比较方法
			/// </summary>
			/// <param name="iObj">比较的对象</param>
			/// <returns>true:相等; false:不相等;</returns>
			public bool Equals (SerializedObject iObj)
			{
				if (bools.Count != iObj.bools.Count) return false; 
				for (var i=bools.Count-1; i>=0; i--) 
					if (bools[i].val != iObj.bools[i].val || bools[i].name != iObj.bools[i].name) return false;
				
				if (ints.Count != iObj.ints.Count) return false; 
				for (var i=ints.Count-1; i>=0; i--) 
					if (ints[i].val != iObj.ints[i].val || ints[i].name != iObj.ints[i].name) return false;
				
				if (floats.Count != iObj.floats.Count) return false; 
				for (var i=floats.Count-1; i>=0; i--) 
					if (floats[i].val != iObj.floats[i].val || floats[i].name != iObj.floats[i].name) return false;
				
				if (strings.Count != iObj.strings.Count) return false; 
				for (var i=strings.Count-1; i>=0; i--) 
					if (strings[i].val != iObj.strings[i].val || strings[i].name != iObj.strings[i].name) return false;
				
				if (chars.Count != iObj.chars.Count) return false; 
				for (var i=chars.Count-1; i>=0; i--) 
					if (chars[i].val != iObj.chars[i].val || chars[i].name != iObj.chars[i].name) return false;
				
				// if (rects.Count != iObj.rects.Count) return false; 
				// for (var i=rects.Count-1; i>=0; i--) 
				// 	if (rects[i].val.position != iObj.rects[i].val.position && 
				// 	    rects[i].val.width != iObj.rects[i].val.width && 
				// 	    rects[i].val.height != iObj.rects[i].val.height || 
				// 	    rects[i].name != iObj.rects[i].name) return false;
				
				if (unityObjs.Count != iObj.unityObjs.Count) return false; 
				for (var i=unityObjs.Count-1; i>=0; i--) 
					if (unityObjs[i].val != iObj.unityObjs[i].val || unityObjs[i].name != iObj.unityObjs[i].name) return false;
				
				if (floatArrays.Count != iObj.floatArrays.Count) return false; 
				for (var i = floatArrays.Count - 1; i >= 0; i--) 
				{
					if (floatArrays[i].name!=iObj.floatArrays[i].name) return false;
					if (floatArrays[i].val.Length!=iObj.floatArrays[i].val.Length) return false;
					for (var j=0; j<floatArrays[i].val.Length; j++)
						if (floatArrays[i].val[j] != iObj.floatArrays[i].val[j]) return false;
				}
				return true;
			}
		}

		/// <summary>
		/// 实体列表
		/// </summary>
		public List<SerializedObject> entities = new List<SerializedObject>();
		
		/// <summary>
		/// 存储对象
		/// </summary>
		/// <param name="iObj">对象</param>
		/// <param name="iWriteProperties">属性写入标识位</param>
		/// <returns>-1:失败或者未写入任何内容;其他:被写入实体列表中的索引值;</returns>
		public int Store (object iObj, bool iWriteProperties = true) 
		{
			//storing nulls to -1
			if (iObj == null) return -1;
			
			// 遍历当前实体列表，看是否已经存在。若存在，则返回索引
			var entitiesCount = entities.Count; 
			for (var i=0; i<entitiesCount; i++)
				if (iObj == entities[i].obj) return i;
			
			// 创建新的序列化对象
			var entity = new SerializedObject();
			var objType = iObj.GetType();
			entity.typeName = objType.AssemblyQualifiedName;
			entity.obj = iObj;

			//adding entity to list before storing other objs
			entities.Add(entity);
			var index = entities.Count-1;

			// 数组
			if (objType.IsArray)
			{
				var array = (Array)iObj;
				var elementType = objType.GetElementType();
				entity.AddValues(elementType, array, this);
				//for (int i=0;i<array.Length;i++) entity.AddValue(elementType, array.GetValue(i), "", this);
				return index;
			}

			// 获取字段信息
			var fields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance); //BindingFlags.NonPublic - does not work in web player
			for (var i=0; i<fields.Length; i++)
			{
				var field = fields[i];
				// 若是编译时写入，且不能修改
				if (field.IsLiteral) continue; //leaving constant fields blank
				// 指针类型
				if (field.FieldType.IsPointer) continue; //skipping pointers (they make unity crash. Maybe require unsafe)
				// 非序列化对象
				if (field.IsNotSerialized) continue;
				
				entity.AddValue(field.FieldType, field.GetValue(iObj), field.Name, this);
			}

			//writing properties
			if (!iWriteProperties) return index;
			
			var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			for (var i=0;i<properties.Length;i++) 
			{
				var prop = properties[i];
				// 不可写
				if (!prop.CanWrite) continue;
				if (prop.Name == "Item") continue; //ignoring this[x] 

				entity.AddValue(prop.PropertyType, prop.GetValue(iObj,null), prop.Name, this);
			}

			return index;
		}
		
		/// <summary>
		/// 检索并取回已存储对象
		/// </summary>
		/// <param name="iIndex">索引</param>
		/// <returns>已存储对象</returns>
		public object Retrieve (int iIndex)
		{
			// checking if object is null
			if (iIndex < 0) return null;
			
			//checking if this object was already retrieved
			if (entities[iIndex].obj != null) return entities[iIndex].obj;
			
			var entity = entities[iIndex];
			var type = Type.GetType(entity.typeName);
			if (type == null) type = Type.GetType(entity.typeName.Substring(0, entity.typeName.IndexOf(","))); //trying to get type using it's short name
			if (type == null) return null; //in case this type do not exists anymore

			// retrieving arrays
			if (type.IsArray)
			{
				var array = entity.GetValues(type.GetElementType(),this);
				entity.obj = array; 
				return array;
			}

			// 实力化目标类型
			var obj = Activator.CreateInstance(type);
			entity.obj = obj; //signing record.obj before calling Retrieve to avoid infinite loop

			// 加载字段值
			var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			for (var f=0; f<fields.Length; f++)
			{
				var field = fields[f];
				// 编译中写入的值
				if (field.IsLiteral) continue; //leaving constant fields blank
				// 指针类型
				if (field.FieldType.IsPointer) continue; //skipping pointers (they make unity crash. Maybe require unsafe)
				// 非序列化对象
				if (field.IsNotSerialized) continue;

				object val = null;
				try
				{
					val = entity.GetValue(field.FieldType, field.Name, this);
				}
				catch (Exception e)
				{
					Fatal("Retrieve():Serialization error(Field):\n" + e);
				}

				field.SetValue(obj, val);
			}

			// 加载属性
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			for (var p=0; p<properties.Length; p++) 
			{
				var prop = properties[p];
				// 不可写
				if (!prop.CanWrite) continue;
				if (prop.Name == "Item") continue; //ignoring this[x] 

				object val = null;
				try
				{
					val = entity.GetValue(prop.PropertyType, prop.Name, this);
				}
				catch (Exception e)
				{
					Fatal("Retrieve():Serialization error(Properties):\n" + e);
				}
				
				if (val != null) 
					prop.SetValue(obj, val, null);
			}

			return obj;
		}

		/// <summary>
		/// 清空实体链接
		/// </summary>
		public void ClearLinks()
		{
			for (var i = 0; i < entities.Count; i++)
			{
				entities[i].obj=null;
			}
				
		} //use this both after all save and load to avoid remaining obj links
		
		/// <summary>
		/// 清空
		/// </summary>
		public void Clear () { entities.Clear(); }

		/// <summary>
		/// 比较两个序列化容器
		/// </summary>
		/// <param name="iSer"></param>
		/// <returns></returns>
		public bool Equals (Serializer iSer)
		{
			if (entities.Count != iSer.entities.Count) return false;

			var count = entities.Count;
			for (var i=0; i<count; i++)
				if (!entities[i].Equals(iSer.entities[i]))
				{
					Info($"Equals():index:{i} Type:{entities[i].typeName} != {iSer.entities[i].typeName}");
					return false;
				}
					
			return true;
		}

		/// <summary>
		/// 深度拷贝
		/// </summary>
		/// <param name="iObj">被拷贝对象</param>
		/// <returns>拷贝完闭对象</returns>
		public static object DeepCopy (object iObj)
		{
			var serializer = new Serializer();
			
			serializer.Store(iObj); 
			
			serializer.ClearLinks();
			var deepCopy = serializer.Retrieve(0);
			serializer.ClearLinks();

			return deepCopy;
		}
	}
}