using System.Collections;
using System.Collections.Generic;

namespace MathExpParser
{
    public class UtilityFunc
    {

        /// <summary>
        /// Insert variable to dictionary and return it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_dictionary"></param>
        /// <param name="p_key"></param>
        /// <param name="p_varaible"></param>
        /// <returns></returns>
        public static Dictionary<string, T> SaveDictionary<T>(Dictionary<string, T> p_dictionary, string p_key, T p_varaible) {
            if (p_dictionary == null) return p_dictionary;

            if (p_dictionary.ContainsKey(p_key))
            {
                p_dictionary[p_key] = p_varaible;
            }
            else {
                p_dictionary.Add(p_key, p_varaible);
            }

            return p_dictionary;
        }

    }
}