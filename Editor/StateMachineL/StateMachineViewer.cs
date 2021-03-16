namespace KoheiUtils
{
    using System;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;

#if ODIN_INSPECTOR
    public class StateMachineViewer : Sirenix.OdinInspector.Editor.OdinEditorWindow
#else
    public class StateMachineViewer : EditorWindow
#endif
    {
        [HideInInspector] public    MonoBehaviour selectedView;
        [HideInInspector] public    string        propertyName;
        [HideInInspector] protected StateMachine  fsm;

        [MenuItem("KoheiUtils/StateMachineL/ShowViewer")]
        private static void ShowWindow()
        {
            // 生成
            GetWindow<StateMachineViewer>("StateMachine Viewer").Show();
        }

        void OnGUI()
        {
            Render();
            base.OnGUI();
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

                fsm = GetFieldOrPropertyValue(selectedView);

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

        private StateMachine GetFieldOrPropertyValue(MonoBehaviour selectedView)
        {
            var selectedType = selectedView.GetType();
            var fsmProperty  = selectedType.GetProperty(propertyName);

            if (fsmProperty != null)
            {
                return (StateMachine) fsmProperty.GetValue(selectedView);
            }

            var fsmField = selectedType.GetField(propertyName);

            if (fsmField != null)
            {
                return (StateMachine) fsmField.GetValue(selectedView);
            }

            return null;
        }

        void Update()
        {
            if (Time.frameCount % 10 == 0)
            {
                Repaint();
            }
        }
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
        public void ChangeState(int stateId)
        {
            fsm.ChangeStateWithoutParam(stateId);
        }
#endif
    }
}