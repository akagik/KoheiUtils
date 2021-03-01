namespace KoheiUtils
{
    using System;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;

    public class StateMachineViewer : EditorWindow
    {
        public MonoBehaviour selectedView;
        public string        propertyName;

        [MenuItem("KoheiUtils/StateMachineL/ShowViewer")]
        private static void ShowWindow()
        {
            // 生成
            GetWindow<StateMachineViewer>("StateMachine Viewer");
        }

        void OnGUI()
        {
            Render();
        }

        private void Render()
        {
            selectedView =
                (MonoBehaviour) EditorGUILayout.ObjectField("StateMachine", selectedView, typeof(MonoBehaviour), true);
            propertyName = EditorGUILayout.TextField("Property Name", propertyName);

            if (selectedView != null)
            {
                EditorGUILayout.LabelField("selected: " + selectedView.gameObject.name);

                if (propertyName == null)
                {
                    EditorGUILayout.LabelField("property name is null");
                    return;
                }

                var selectedType = selectedView.GetType();
                var fsmProperty  = selectedType.GetField(propertyName);

                if (fsmProperty == null)
                {
                    EditorGUILayout.LabelField("Specified property name NOT exits");
                    return;
                }

                StateMachine fsm = (StateMachine) fsmProperty.GetValue(selectedView);

                if (fsm == null)
                {
                    EditorGUILayout.LabelField("fsm is null");
                }
                else
                {
                    foreach (var id in fsm.Keys)
                    {
                        var state = fsm.GetState(id);

                        Type stateType = state.GetType();

                        Color defaultColor = GUI.color;
                        GUI.color = (fsm.currentState == state) ? Color.red : Color.black;
                        EditorGUILayout.LabelField($"{stateType} ({state.stateId})");
                        GUI.color = defaultColor;

                        EditorGUI.indentLevel = 1;

                        using (new EditorGUI.DisabledScope())
                        {
                            var fields =
                                stateType.GetFields(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                            foreach (var f in fields)
                            {
                                object value = f.GetValue(state);

                                if (f.FieldType == typeof(float))
                                {
                                    EditorGUILayout.FloatField(f.Name, (float) value);
                                }
                                else
                                {
                                    if (value == null)
                                    {
                                        EditorGUILayout.TextField(f.Name, "null");
                                    }
                                    else
                                    {
                                        EditorGUILayout.TextField(f.Name, value.ToString());
                                    }
                                }
                            }
                        }

                        EditorGUI.indentLevel = 0;
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No object selected");
            }
        }

        void Update()
        {
            if (Time.frameCount % 10 == 0)
            {
                Repaint();
            }
        }
    }
}