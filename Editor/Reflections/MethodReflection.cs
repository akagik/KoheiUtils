using System;
using System.Reflection;
using UnityEngine;

namespace KoheiUtils.Reflections
{
    public class MethodReflection
    {
        public string typeName;
        public string methodName;
        public string assemblyName;

        public Type type;
        public MethodInfo methodInfo;

        public bool HasAssembly => !string.IsNullOrEmpty(assemblyName);

        public string FullyTypeName => HasAssembly ? typeName + ", " + assemblyName : typeName;

        public static bool TryParse(string arg, out MethodReflection parsed, bool suppressLog = false)
        {
            MethodReflection reflection = new MethodReflection();

            if (!reflection.TryParseSplit(arg, suppressLog))
            {
                parsed = null;
                return false;
            }

            if (!reflection.TryGetType(suppressLog))
            {
                parsed = null;
                return false;
            }

            parsed = reflection;
            return true;
        }

        bool TryParseSplit(string arg, bool suppressLog = false)
        {
            string[] splits = arg.Split(',');

            if (splits.Length != 1 && splits.Length != 2)
            {
                if (!suppressLog)
                    Debug.LogError($"[型名.メソッド名, アセンブリ名] または [型名.メソッド名] の形式で指定する必要があります: [{arg}]");
                return false;
            }

            // アセンブリ名を解析
            if (splits.Length == 2)
            {
                assemblyName = splits[1].Trim();
            }
            else
            {
                // 指定なしの場合は "Assembly-CSharp" を使う
                assemblyName = "Assembly-CSharp";
            }

            arg = splits[0].Trim();
            int lastIndex = arg.LastIndexOf('.');

            if (lastIndex == -1)
            {
                if (!suppressLog)
                    Debug.LogError("メソッド呼び出しには TypeName. を頭につける必要があります: " + arg);
                return false;
            }

            typeName = arg.Substring(0, lastIndex);
            methodName = arg.Substring(lastIndex + 1, arg.Length - (lastIndex + 1));

            return true;
        }

        bool TryGetType(bool suppressLog = false)
        {
            try
            {
                // Get the type of a specified class.
                type = Type.GetType(FullyTypeName);

                if (type == null)
                {
                    if (!suppressLog)
                        Debug.LogError($"Not found type: [{FullyTypeName}]");
                    return false;
                }

                methodInfo = type.GetMethod(methodName);

                if (methodInfo == null)
                {
                    if (!suppressLog)
                        Debug.LogError("Not found methodInfo: " + methodName);
                    return false;
                }

                return true;
            }
            catch (TypeLoadException e)
            {
                if (!suppressLog)
                    Debug.LogError(e);
            }

            return false;
        }
    }
}