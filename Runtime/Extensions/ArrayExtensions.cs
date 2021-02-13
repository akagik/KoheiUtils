namespace KoheiUtils
{
    using System.Collections.Generic;
    using System.Linq;
    

    public static class ArrayExtensions
    {
        public static string ToString<T>(this ICollection<T> c)
        {
            if (typeof(T) == typeof(string))
            {
                return "[" + string.Join(", ", c.Select(s => "\"" + s.ToString() + "\"").ToArray()) + "]";
            }

            return "[" + string.Join(", ", c.Select(s => s.ToString()).ToArray()) + "]";
        }
    }
}