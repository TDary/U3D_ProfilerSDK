using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KProfilerExtension
{
    public static class DictionaryHelper {
        /// <summary>
        /// 根据Key获取对应Value，如果Key不存在，则返回该类型默认值，默认值可指定。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue)) {
            TValue result;
            if (dict.TryGetValue(key, out result) == false) {
                result = defaultValue;
            }
            return result;
        }
    }
}
