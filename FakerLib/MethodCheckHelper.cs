using System;
using System.Reflection;

namespace FakerLib
{
    class MethodCheckHelper
    {
        private static readonly string SETTER_PREFIX = "set";
        private static readonly string GETTER_PREFIX = "get";

        public static bool IsSetter(MethodInfo method)
        {
            return method.Name.StartsWith(SETTER_PREFIX, StringComparison.CurrentCultureIgnoreCase)
                && method.GetParameters().Length == 1;
        }

        public static bool IsGetter(MethodInfo method)
        {
            return method.Name.StartsWith(GETTER_PREFIX, StringComparison.CurrentCultureIgnoreCase)
                && method.GetParameters().Length == 0;
        }
    }
}
