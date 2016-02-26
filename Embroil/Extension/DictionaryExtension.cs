using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Extension
{
    public static class DictionaryExtension
    {
        public static Dictionary<T1,T2> Merge<T1,T2>(this Dictionary<T1,T2> baseDictionary, Dictionary<T1,T2> addDictionary)
        {
            return baseDictionary.Concat(addDictionary).GroupBy(k => k.Key).ToDictionary(k => k.Key, v => v.First().Value);
        }
    }
}
