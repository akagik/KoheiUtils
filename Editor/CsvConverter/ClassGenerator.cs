﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace KoheiUtils
{
    public class ClassGenerator
    {
        const string FIELD_FORMAT = "    public {0} {1};\n";
        public static readonly string ROWS = "rows";

        public static string GenerateClass(string name, Field[] fields, bool isPureClass)
        {
            string classData = "";
            classData = "using UnityEngine;\n";
            classData += "using System.Collections.Generic;\n";
            classData += "\n";

            if (isPureClass)
            {
                classData += "[System.Serializable]\n";
                classData += "public class " + name + "\n";
            }
            else
            {
                classData += "public class " + name + " : ScriptableObject\n";
            }

            classData += "{\n";

            HashSet<string> addedFields = new HashSet<string>();
            for (int col = 0; col < fields.Length; col++)
            {
                Field f = fields[col];
                if (addedFields.Contains(f.fieldNameWithoutIndexing)) continue;

                string fieldName = f.fieldName;
                string typeName = f.typeName;

                if (fieldName == "" || typeName == "")
                {
                    continue;
                }

                if (f.isArrayField)
                {
                    fieldName = f.fieldNameWithoutIndexing;
                    typeName = typeName + "[]";
                }

                classData += string.Format(FIELD_FORMAT, typeName, fieldName);
                addedFields.Add(f.fieldNameWithoutIndexing);
            }

            classData += "}\n";

            return classData;
        }

        public static string GenerateTableClass(ConvertSetting setting, string tableClassName, Field[] keys)
        {
            string className = setting.className;

            string code = "";

            if (setting.isDictionary)
            {
                code = LoadDictTableTemplate();

                if (keys == null)
                {
                    throw new Exception("Dictionary Table にはキーが必要です");
                }

                if (keys.Length != 1)
                {
                    throw new Exception("Dictionary Table はキーを１つだけ指定してください");
                }

                code = code.Replace("%KeyType%", keys[0].typeName);
                code = code.Replace("%KeyName%", keys[0].fieldName);
            }
            // キーが有効な場合はキーから検索できるようにする
            else if (keys != null && keys.All((arg) => arg.isValid))
            {
                code = LoadListTableTemplate();

                string argStr = "";
                string condStr = "";

                for (int i = 0; i < keys.Length; i++)
                {
                    argStr += string.Format("{0} {1}, ", keys[i].typeName, keys[i].fieldName);
                    condStr += string.Format("o.{0} == {0} && ", keys[i].fieldName);
                }

                argStr = argStr.Substring(0, argStr.Length - 2);
                condStr = condStr.Substring(0, condStr.Length - 4);
                code = code.Replace("%FindArguments%", argStr);
                code = code.Replace("%FindPredicate%", condStr);
            }
            // キーなしリストテーブル
            else
            {
                code = LoadNoKeyListTableTemplate();
            }

            code = code.Replace("%TableClassName%", tableClassName);
            code = code.Replace("%ClassName%", className);

            return code;
        }

        public static string LoadNoKeyListTableTemplate()
        {
            TextAsset ta = KoheiUtils.EditorUtils.LoadOnlyOneAsset<TextAsset>("\"template_nokey_list_table\" t:TextAsset");
            return ta.text;
        }

        public static string LoadListTableTemplate()
        {
            TextAsset ta = KoheiUtils.EditorUtils.LoadOnlyOneAsset<TextAsset>("\"template_list_table\" t:TextAsset");
            return ta.text;
        }

        public static string LoadDictTableTemplate()
        {
            TextAsset ta = KoheiUtils.EditorUtils.LoadOnlyOneAsset<TextAsset>("\"template_dict_table\" t:TextAsset");
            return ta.text;
        }

        public static int[] FindKeyIndexes(ConvertSetting setting, Field[] fields)
        {
            List<int> indexes = new List<int>();

            string[] keys = setting.keys;
            // Debug.Log(keys.ToString<string>());

            for (int j = 0; j < keys.Length; j++)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].fieldName == keys[j])
                    {
                        indexes.Add(i);
                    }
                }
            }

            return indexes.ToArray();
        }
    }
}