using System.Collections;
using Packages.Common.Base;

namespace Packages.BuildSystem.AndroidSDK.Options.OneSDK {

	/// <summary>
	/// 易接专用JSON对象.
	/// </summary>
	public sealed class SfjsonObject :ClassExtension
	{
		public Hashtable nameValuePairs = new Hashtable(); 
		private string _ins;
		private char[] _cins;
		private int _pos;
		public SfjsonObject() {
		}
		public SfjsonObject(string iJson) {
			var readFrom = new JsonTokener(iJson);
			var obj = readFrom.nextValue();

			if(obj is SfjsonObject){
				nameValuePairs = ((SfjsonObject)obj).nameValuePairs;
			}


		}
		public object get(string iName){
			return nameValuePairs[iName];
		}
		public void put(string iKey, object iValue){
			nameValuePairs.Add(iKey,iValue);
		}


		public string toString()
		{
			var svalue = "{";
			foreach (DictionaryEntry de in nameValuePairs)
			{
				svalue += "\""+de.Key+"\""+":"+"\""+de.Value+"\""+",";
			}

			svalue = svalue.Remove(svalue.Length-1);
			svalue += "}";

			return svalue;
		}

		public string toInlineString()
		{
			var svalue = "{";
			foreach (DictionaryEntry de in nameValuePairs)
			{
				svalue += "\\\""+de.Key+"\\\""+":"+"\\\""+de.Value+"\\\""+",";
			}

			svalue = svalue.Remove(svalue.Length-1);
			svalue += "}";

			return svalue;
		}

	}
}