using System.Collections.Generic;

namespace MyAssets.Scripts.Utils
{
    public static class ListUtils
    {
        public static T TryGetAtIndex<T>(this List<T> list, int index) where T : class
        {
            return list != null && list.Count > index && index >= 0 ? list[index] : default (T);
        }
    }
}