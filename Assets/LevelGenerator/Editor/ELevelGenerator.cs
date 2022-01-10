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
        
        [MenuItem("Window/Level Generator/Open Generator", false, 2500)]
        public static void Initialize()
        {
            GetWindow<ELevelGenerator>(false, "Level Generator", true);
        }

        private void OnEnable()
        {
            Generator.LevelGenerator.EnableCaching();
            Utility.EventHandler.OnGeneratorRecompile();
        }

        private void OnDisable()
        {
            Generator.LevelGenerator.DisableCaching();
        }

        private void OnGUI()
        {
            if (_config == null)
            {
                _config = GeneratorConfigUtility.GetConfiguration("Config") ??
                          GeneratorConfigUtility.CreateConfiguration("Config");
            }

            if (EditorApplication.isCompiling)
            {
                Utility.EventHandler.OnGeneratorRecompile();
            }

            var myIcon = EditorGUIUtility.Load("Assets/LevelGenerator/Editor/Resources/DG_Icon.png") as Texture2D;
            titleContent.image = myIcon;

            GUILayout.Space(24);

            GUILayout.BeginHorizontal();
            {
                var icon = EditorGUIUtility.Load("Assets/LevelGenerator/Editor/Resources/DG_Logo.png") as Texture;
                GUILayout.FlexibleSpace();
                GUILayout.Label(icon, new GUIStyle() { fixedHeight = 75, alignment = TextAnchor.MiddleCenter});
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(24);

            // note : commands
            
            GUILayout.BeginVertical("Generator Commands", "box");
            {
                GUILayout.Space(12);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
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
                    GUILayout.EndVertical();

                    EditorGUILayout.HelpBox("Levels can be generated both in and out of play mode. If a level is generated in play mode, it will be reset upon exiting play mode. Generating a new level will automatically clear the last one.\n\nTo save a generated level, make sure to copy and save the seed from the \"Generator Seed\" section bellow. Note that the seed may yield different results with different configurations.", MessageType.None);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(12);
            }
            GUILayout.EndVertical();

            GUILayout.Space(12);
            
            // note : seed
            
            GUILayout.BeginVertical("Generator Seed", "box");
            {
                GUILayout.Space(12);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                var centerSeedText = new GUIStyle(GUI.skin.textField) {alignment = TextAnchor.MiddleCenter};
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    var seed = EditorGUILayout.TextField(new GUIContent("", ""), Generator.LevelGenerator.GetSeed(), centerSeedText, GUILayout.MaxWidth(320));
                    GUILayout.FlexibleSpace();

                    if(seed != null) seed = Regex.Replace(seed, @"[^0-9 -]", "");
                    Generator.LevelGenerator.SetSeed(seed);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(12);
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(12);

            // note : configuration

            EditorGUI.BeginChangeCheck();
            
            var configEditor = UnityEditor.Editor.CreateEditor(_config);
            configEditor.OnInspectorGUI();

            if (settingsChangedWarning)
            {
                EditorGUILayout.HelpBox("Any changed settings will only be applied once a new level is generated.", MessageType.Info);
            }
            
            if (!EditorGUI.EndChangeCheck())
                return;
            
            configEditor.ResetTarget();
            configEditor.Repaint();
            AssetDatabase.SaveAssets();
            settingsChangedWarning = true;
        }
    }
}
