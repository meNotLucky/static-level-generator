using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using LevelGenerator.Utility;

namespace LevelGenerator.Editor
{
    public class ELevelGenerator : EditorWindow
    {
        public bool settingsChangedWarning;
        
        private bool _isInPlayMode;
        private GeneratorConfig _config;

        private Vector2 _scrollPos = Vector2.zero;

        [MenuItem("Window/Level Generator/Open Generator", false, 2500)]
        public static void Initialize()
        {
            GetWindow<ELevelGenerator>(false, "Level Generator", true);
        }

        private void OnEnable()
        {
            Generator.LevelGenerator.EnableCaching();
        }

        private void OnDisable()
        {
            Generator.LevelGenerator.DisableCaching();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (_config == null)
            {
                _config = GeneratorConfigUtility.GetConfiguration("Config") ??
                          GeneratorConfigUtility.CreateConfiguration("Config");
            }

            var myIcon = EditorGUIUtility.Load("Assets/LevelGenerator/Editor/Resources/DG_Icon.png") as Texture2D;
            titleContent.image = myIcon;

            GUILayout.Space(24);

            using (new GUILayout.HorizontalScope())
            {
                var icon = EditorGUIUtility.Load("Assets/LevelGenerator/Editor/Resources/DG_Logo.png") as Texture;
                GUILayout.FlexibleSpace();
                GUILayout.Label(icon, new GUIStyle() { fixedHeight = 75, alignment = TextAnchor.MiddleCenter});
                GUILayout.FlexibleSpace();
            }
            
            GUILayout.Space(24);

            // note : commands

            using (new GUILayout.VerticalScope("Generator Commands", "box"))
            {
                GUILayout.Space(12);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope())
                    {
                        if (GUILayout.Button("Generate New Level", GUILayout.MinHeight(30), GUILayout.MinWidth(180)))
                        {
                            Generator.LevelGenerator.SetConfiguration(_config);
                            Generator.LevelGenerator.GenerateNewLevel();
                            settingsChangedWarning = false;
                            GUI.UnfocusWindow();
                        }
                        if (GUILayout.Button("Generate Level From Seed", GUILayout.MinHeight(30), GUILayout.MinWidth(180)))
                        {
                            Generator.LevelGenerator.SetConfiguration(_config);
                            Generator.LevelGenerator.GenerateLevelFromSeed();
                            settingsChangedWarning = false;
                        }
                        if (GUILayout.Button("Clear Level", GUILayout.MinHeight(30), GUILayout.MinWidth(180)))
                        {
                            Generator.LevelGenerator.ClearLevel();
                        }
                        if (GUILayout.Button("Clear Cache", GUILayout.MinHeight(30), GUILayout.MinWidth(180)))
                        {
                            Generator.LevelGenerator.ClearCache();
                        }
                    }

                    EditorGUILayout.HelpBox("Levels can be generated both in and out of play mode. If a level is generated in play mode, it will be reset upon exiting play mode. Generating a new level will automatically clear the last one.\n\nTo save a generated level, make sure to copy and save the seed from the \"Generator Seed\" section bellow. Note that the seed may yield different results with different configurations.", MessageType.None);
                }
                
                GUILayout.Space(12);
            }

            GUILayout.Space(12);

            // note : seed

            using (new GUILayout.VerticalScope("Generator Seed", "box"))
            {
                GUILayout.Space(12);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                var centerSeedText = new GUIStyle(GUI.skin.textField) {alignment = TextAnchor.MiddleCenter};
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    var seed = EditorGUILayout.TextField(new GUIContent("", ""), Generator.LevelGenerator.GetSeed(), centerSeedText, GUILayout.MaxWidth(320));
                    GUILayout.FlexibleSpace();

                    if(seed != null) seed = Regex.Replace(seed, @"[^0-9 -]", "");
                    Generator.LevelGenerator.SetSeed(seed);
                }
                
                GUILayout.Space(12);
            }
            
            GUILayout.Space(12);

            // note : configuration

            EditorGUI.BeginChangeCheck();
            
            var configEditor = UnityEditor.Editor.CreateEditor(_config);
            configEditor.OnInspectorGUI();

            if (settingsChangedWarning)
            {
                EditorGUILayout.HelpBox("Any changed settings will only be applied once a new level is generated.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            if (!EditorGUI.EndChangeCheck())
                return;
            
            configEditor.ResetTarget();
            configEditor.Repaint();
            AssetDatabase.SaveAssets();
            settingsChangedWarning = true;
        }
    }
}
