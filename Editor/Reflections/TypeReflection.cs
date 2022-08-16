using System;
using UnityEngine;

namespace KoheiUtils.Reflections
{
    public class TypeReflection
    {
        public string typeName;
        public string assemblyName;

        public Type type;

        public bool HasAssembly => !string.IsNullOrEmpty(assemblyName);

        public string FullyTypeName => HasAssembly ? typeName + ", " + assemblyName : typeName;

        public static bool TryParse(string arg, out TypeReflection parsed)
        {
            TypeReflection reflection = new();

            if (!reflection.TryParseSplit(arg))
            {
                parsed = null;
                return false;
            }

            if (!reflection.TryGetType())
            {
                parsed = null;
                return false;
            }

            parsed = reflection;
            return true;
        }


        bool TryParseSplit(string arg)
        {
            string[] splits = arg.Split(',');

            if (splits.Length != 1 && splits.Length != 2)
            {
                Debug.LogError($"[型名.メソッド名, アセンブリ名] または [型名.メソッド名] の形式で指定する必要があります: [{arg}]");
                return false;
            }

            // タイプ名を取得
            typeName = splits[0].Trim();

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

            return true;
        }

        bool TryGetType()
        {
            try
            {
                // Get the type of a specified class.
                type = Type.GetType(FullyTypeName);

                if (type == null)
                {
                    Debug.LogError($"Not found type: [{FullyTypeName}]");
                    return false;
                }

                return true;
            }
            catch (TypeLoadException e)
            {
                Debug.LogError(e);
            }

            return false;
        }
    }
}