using System.Reflection;

namespace SMW_ML.Utils
{
    internal static class ReflectionTool
    {
        /// <summary>
        /// Returns the private field of the given instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetField<T>(this object obj, string fieldName)
        {
            T? val = (T?)(obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj));

            return val!;
        }
    }
}
