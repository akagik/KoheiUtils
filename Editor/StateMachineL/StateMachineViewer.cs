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
        [HideInInspector] public    bool          explicitSelect;
        [HideInInspector] public    GameObject    selectedView;
        [HideInInspector] public    string        propertyName;
        [HideInInspector] protected StateMachine  fsm;
        [HideInInspector] public    bool          showBaseProperties;


        private Vector2 scrollPosition;
        private bool[]  stateFoldOuts;

        [MenuItem("KoheiUtils/StateMachineL/ShowViewer")]
        private static void ShowWindow()
        {
            // 生成
            GetWindow<StateMachineViewer>("StateMachine Viewer").Show();
        }

#if ODIN_INSPECTOR
#if UNITY_2022_1_OR_NEWER
        protected override void OnImGUI()
        {
            Render();
            base.OnImGUI();
        }
#else
        protected override void OnGUI()
        {
            Render();
            base.OnGUI();
        }
#endif
#else
        void OnGUI()
        {
            Render();
        }
#endif

        private void Render()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                GUILayout.Space(5f);
                EditorGUI.indentLevel += 1;
                GUILayout.Label("Base Settings", EditorStyles.boldLabel);
                
                explicitSelect = EditorGUILayout.Toggle("Explicit select", explicitSelect);

                if (explicitSelect)
                {
                    selectedView =
                        (GameObject) EditorGUILayout.ObjectField("StateMachine", selectedView, typeof(GameObject),
                            true);
                }
                else
                {
                    selectedView = (Selection.activeObject as GameObject);
                }
                
                propertyName          =  EditorGUILayout.TextField("Property Name", propertyName);
                showBaseProperties    =  EditorGUILayout.Toggle("Show base properties", showBaseProperties);
                EditorGUI.indentLevel -= 1;

                if (selectedView != null)
                {
                    GUILayout.Space(5f);
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
                        if (stateFoldOuts == null || stateFoldOuts.Length != fsm.Keys.Count)
                        {
                            stateFoldOuts = new bool[fsm.Keys.Count];
                        }

                        GUILayout.Label("States", EditorStyles.boldLabel);
                        int i = 0;
                        foreach (var id in fsm.Keys)
                        {
                            var state = fsm.GetState(id);

                            Type stateType = state.GetType();

                            Color  color  = (fsm.currentState == state) ? Color.red : GUI.color;
                            string header = $"{stateType} ({state.stateId})";
                            stateFoldOuts[i] = Foldout(header, stateFoldOuts[i], color);

                            if (stateFoldOuts[i])
                            {
                                EditorGUI.indentLevel = 1;

                                using (new EditorGUI.DisabledScope())
                                {
                                    var fields =
                                        stateType.GetFields(
                                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                                    foreach (var f in fields)
                                    {
                                        if (!showBaseProperties && f.DeclaringType == typeof(State))
                                        {
                                            continue;
                                        }

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

                            i++;
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No object selected");
                }
            }
            GUILayout.EndScrollView();
        }

        private StateMachine GetFieldOrPropertyValue(GameObject selectedView)
        {
            foreach (Component m in selectedView.GetComponents<Component>())
            {
                var selectedType = m.GetType();
                
                // プロパティチェック
                {
                    var fsmProperty  = selectedType.GetProperty(propertyName);

                    if (fsmProperty != null)
                    {
                        var fsm = fsmProperty.GetValue(m);
                        
                        if (fsm is StateMachine)
                        {
                            return fsm as StateMachine;
                        }
                    }
                }

                // フィールドチェック
                {
                    var fsmField = selectedType.GetField(propertyName);

                    if (fsmField != null)
                    {
                        var fsm = fsmField.GetValue(m);
                        
                        if (fsm is StateMachine)
                        {
                            return fsm as StateMachine;
                        }
                    }
                }
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

        public static bool Foldout(string title, bool display, Color color)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font          = new GUIStyle(EditorStyles.label).font;
            style.border        = new RectOffset(15, 7, 4, 4);
            style.fixedHeight   = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);

            Color defaultColor = GUI.color;
            GUI.color = color;
            GUI.Box(rect, title, style);
            GUI.color = defaultColor;

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
        public void ChangeState(int stateId)
        {
            fsm.Transition(stateId);
        }
#endif
    }
}
