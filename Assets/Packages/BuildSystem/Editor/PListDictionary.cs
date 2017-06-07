using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;

namespace BuildSystem
{
	// Class for parsing a plist into Dictionary hierarchy
    public class PListDictionary : Dictionary<string, object>
    {
		public PListDictionary() {}

        public PListDictionary(string inputString)
        {
            Parse(inputString);
        }

        public void Parse(string inputString)
        {
            Clear();
			var settings = new XmlReaderSettings() {
				XmlResolver = null,
				ProhibitDtd = false
			};
			var reader = XmlReader.Create(new StringReader(inputString), settings);
            XDocument doc = XDocument.Load(reader);
            XElement plist = doc.Element("plist");
            XElement dict = plist.Element("dict");

            var dictElements = dict.Elements();
            Parse(this, dictElements);
        }

        void Parse(PListDictionary dict, IEnumerable<XElement> elements)
        {
            for (int i = 0; i < elements.Count(); i += 2)
            {
                XElement key = elements.ElementAt(i);
                XElement val = elements.ElementAt(i + 1);

                dict[key.Value] = ParseValue(val);
            }
        }

        List<object> ParseArray(IEnumerable<XElement> elements)
        {
            List<object> list = new List<object>();
            foreach (XElement e in elements)
            {
                object one = ParseValue(e);
                list.Add(one);
            }

            return list;
        }

        object ParseValue(XElement val)
        {
            switch (val.Name.ToString())
            {
                case "string":
				case "date":
				case "data":
                    return val.Value;
                case "integer":
                    return int.Parse(val.Value);
                case "real":
                    return float.Parse(val.Value);
                case "true":
                    return true;
                case "false":
                    return false;
                case "dict":
                    PListDictionary plist = new PListDictionary();
                    Parse(plist, val.Elements());
                    return plist;
                case "array":
                    List<object> list = ParseArray(val.Elements());
                    return list;

                default:
                    throw new ArgumentException("Malformed PList: trying to parse " + val.Name.ToString());
            }
        }
    }
}