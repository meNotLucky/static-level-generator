using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using LevelGenerator.Generator;
using LevelGenerator.Utility;

namespace LevelGenerator.Editor
{
    public class ELevelGenerator : EditorWindow
    {
        public bool settingsChangedWarning;
    
        private bool _showCellTransform;
        private bool _showLevelTransform;

        private static GeneratorConfig _config;
        
        [MenuItem("Window/Level Generator")]
        public static void Initialize()
        {
            FindGeneratorConfig();
            GetWindow<ELevelGenerator>(false, "Grid Level Generator", true);
        }

        private static void FindGeneratorConfig()
        {
            _config = GeneratorConfigUtility.GetConfiguration("Config");
            if (_config == null)
                _config = GeneratorConfigUtility.CreateConfiguration("Config");
        }
        
        public void OnGUI()
        {
            if (_config == null)
                FindGeneratorConfig();
            
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
                            Generator.LevelGenerator.ClearLevel();
                            Generator.LevelGenerator.SetConfiguration(_config);
                            Generator.LevelGenerator.GenerateNewLevel();
                            settingsChangedWarning = false;
                            GUI.UnfocusWindow();
                        }
                        if (GUILayout.Button("Generate Level From Seed", GUILayout.MinHeight(30), GUILayout.MinWidth(180)))
                        {
                            Generator.LevelGenerator.ClearLevel();
                            Generator.LevelGenerator.SetConfiguration(_config);
                            Generator.LevelGenerator.GenerateLevelFromSeed();
                            settingsChangedWarning = false;
                        }
                        if (GUILayout.Button("Clear Level", GUILayout.MinHeight(30), GUILayout.MinWidth(180)))
                        {
                            Generator.LevelGenerator.ClearLevel();
                        }
                    }
                    GUILayout.EndVertical();

                    EditorGUILayout.HelpBox("Levels can be generated both in and out of play mode. If a level is generated in play mode, it will be reset upon exiting play mode. To save a level generated in play mode, make sure to copy the seed from the \"Generator Settings\" section. Generating a new level will automatically clear the last one.", MessageType.None);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(12);
            
            // note : settings
            
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginVertical("Generator Settings", "box");
                {

                    GUILayout.Space(12);

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    var centerLabelText = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
                    var centerSeedText = new GUIStyle(GUI.skin.textField) {alignment = TextAnchor.MiddleCenter};
                    EditorGUILayout.LabelField("", "Level Seed", centerLabelText);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        var seed = EditorGUILayout.TextField(new GUIContent("", ""), Generator.LevelGenerator.GetSeed(), centerSeedText, GUILayout.MaxWidth(320));
                        if(seed != null)
                            seed = Regex.Replace(seed, @"[^0-9 -]", "");
                        Generator.LevelGenerator.SetSeed(seed);
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();


                    GUILayout.Space(12);

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical();
                        {
                            _config.gridWidth = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Horizontal Grid Size", "The max number of cells that can be generated horizontally."), _config.gridWidth), 1, 500);
                            _config.gridHeight = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Vertical Grid Size", "The max number of cells that can be generated vertically."), _config.gridHeight), 1, 500);
                            GUILayout.Space(12);
                            var minimumSize = EditorGUILayout.IntField(new GUIContent("Minimum Level Size", "The minimum number of rooms allowed to be generated. This may cause long loading times if set too high."), _config.minLevelSize);
                            var gridSize = _config.gridHeight * _config.gridWidth;
                            _config.minLevelSize = Mathf.Clamp(minimumSize, 0, gridSize);
                        }
                        GUILayout.EndVertical();

                        EditorGUILayout.HelpBox("The Grid Size defines the bounds of the grid the level will be generated in. It does not define the total size of the level.\n\nThe Minimum Level Size defines the smallest number of rooms generated.", MessageType.None);
                    }
                    GUILayout.EndHorizontal();

                    if (_config.minLevelSize > ((float) (_config.gridHeight * _config.gridWidth) / 3) * 2)
                        EditorGUILayout.HelpBox("A high Minimum Level Size can result in longer load times. It is recommended to keep the minimum size bellow 2/3 of the total grid size (" + _config.gridHeight * _config.gridWidth + ").", MessageType.Warning);

                    GUILayout.Space(12);

                    _config.forcedLevelGeneration = EditorGUILayout.Toggle(new GUIContent("Forced Level Generation", "Forcing the generator to regenerate the level until all essential rooms are placed and valid, as well as all exits having a connected room. This may cause long loading times and will crash the engine if the level sizing does not allow for valid placement. Does not affect essential rooms with fixed positions."), _config.forcedLevelGeneration);

                    if (_config.forcedLevelGeneration)
                        EditorGUILayout.HelpBox("Forced Level Generation may cause long loading times or crashing! See tooltip for more info.", MessageType.Warning);

                    GUILayout.Space(12);

                    _config.gridAlignment = (GridAlignment) EditorGUILayout.EnumPopup("Grid Alignment", _config.gridAlignment);

                    GUILayout.Space(12);

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Cell Spacing", GUILayout.ExpandWidth(false), GUILayout.MinWidth(120));

                        EditorGUIUtility.labelWidth = 10;

                        var offsetX = Mathf.Abs(EditorGUILayout.FloatField("X", _config.cellPositionOffset.x, GUILayout.MinWidth(60)));
                        
                        GUILayout.Space(2);
                        
                        var label = _config.gridAlignment == GridAlignment.Horizontal ? "Z" : "Y";
                        var offsetY = Mathf.Abs(EditorGUILayout.FloatField(label, _config.cellPositionOffset.y, GUILayout.MinWidth(60)));

                        _config.cellPositionOffset = new Vector2(offsetX, offsetY);

                        GUILayout.Label("", GUILayout.MaxWidth(60));

                        EditorGUIUtility.labelWidth = 0;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(2);

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(12);
                        GUILayout.BeginVertical();
                        {
                            _showCellTransform = EditorGUILayout.Foldout(_showCellTransform, "Cell Transform");

                            if (_showCellTransform)
                            {
                                _config.cellRotation = EditorGUILayout.Vector3Field("Rotation", _config.cellRotation);
                                _config.cellScale = EditorGUILayout.Vector3Field("Scale", _config.cellScale);
                                GUILayout.Space(12);
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(12);
                        GUILayout.BeginVertical();
                        {
                            _showLevelTransform = EditorGUILayout.Foldout(_showLevelTransform, "Level Transform");

                            if (_showLevelTransform)
                            {
                                _config.levelPosition = EditorGUILayout.Vector3Field("Position", _config.levelPosition);
                                _config.levelRotation = EditorGUILayout.Vector3Field("Rotation", _config.levelRotation);
                                _config.levelScale = EditorGUILayout.Vector3Field("Scale", _config.levelScale);
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(12);

                    if (GUILayout.Button("Edit Room Templates", GUILayout.MinHeight(25)))
                    {
                        ERoomTemplates.Initialize(_config);
                    }

                    if (settingsChangedWarning)
                    {
                        GUILayout.Space(12);
                        EditorGUILayout.HelpBox("Any changed settings will only be applied once a new level is generated.", MessageType.Info);
                    }
                }
                GUILayout.EndVertical();
            }
            
            if (!EditorGUI.EndChangeCheck())
                return;
            
            AssetDatabase.SaveAssets();
            settingsChangedWarning = true;
        }
    }
}
