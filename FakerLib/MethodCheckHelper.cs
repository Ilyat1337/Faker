using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
    }
}
