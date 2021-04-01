using LevelGenerator.Generator;
using LevelGenerator.Utility;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace LevelGenerator.Editor
{
    [CustomEditor(typeof(GeneratorConfig))]
    public class EGeneratorConfig : UnityEditor.Editor
    {
        private static bool _showCellTransform;
        private static bool _showLevelTransform;
        private static bool _showExperimentalSettings;

        public override void OnInspectorGUI()
        {
            var config = (GeneratorConfig)target;
            GUILayout.BeginVertical("Generator Configuration", "box");
            {
                GUILayout.Space(12);
                
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        config.gridWidth = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Horizontal Grid Size", "The max number of cells that can be generated horizontally."), config.gridWidth), 1, 500);
                        config.gridHeight = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Vertical Grid Size", "The max number of cells that can be generated vertically."), config.gridHeight), 1, 500);
                        GUILayout.Space(12);
                        var minimumSize = EditorGUILayout.IntField(new GUIContent("Minimum Level Size", "The minimum number of rooms allowed to be generated. This may cause long loading times if set too high."), config.minLevelSize);
                        var gridSize = config.gridHeight * config.gridWidth;
                        config.minLevelSize = Mathf.Clamp(minimumSize, 0, gridSize);
                    }
                    GUILayout.EndVertical();

                    EditorGUILayout.HelpBox("The Grid Size defines the bounds of the grid the level will be generated in. It does not define the total size of the level.\n\nThe Minimum Level Size defines the smallest number of rooms generated.", MessageType.None);
                }
                GUILayout.EndHorizontal();

                if (config.minLevelSize > ((float) (config.gridHeight * config.gridWidth) / 3) * 2)
                    EditorGUILayout.HelpBox("A high Minimum Level Size can result in longer load times. It is recommended to keep the minimum size bellow 2/3 of the total grid size (" + config.gridHeight * config.gridWidth + ").", MessageType.Warning);

                GUILayout.Space(12);

                config.forcedLevelGeneration = EditorGUILayout.Toggle(new GUIContent("Forced Level Generation", "Forcing the generator to regenerate the level until all essential rooms are placed and valid, as well as all exits having a connected room. This may cause long loading times and will crash the engine if the level sizing does not allow for valid placement. Does not affect essential rooms with fixed positions."), config.forcedLevelGeneration);

                if (config.forcedLevelGeneration)
                    EditorGUILayout.HelpBox("Forced Level Generation may cause long loading times or crashing! See tooltip for more info.", MessageType.Warning);

                GUILayout.Space(12);

                config.gridAlignment = (GridAlignment) EditorGUILayout.EnumPopup("Grid Alignment", config.gridAlignment);

                GUILayout.Space(12);

                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Cell Spacing", GUILayout.ExpandWidth(false), GUILayout.MinWidth(120));

                    EditorGUIUtility.labelWidth = 10;

                    var offsetX = Mathf.Abs(EditorGUILayout.FloatField("X", config.cellPositionOffset.x, GUILayout.MinWidth(60)));
                    
                    GUILayout.Space(2);
                    
                    var label = config.gridAlignment == GridAlignment.Horizontal ? "Z" : "Y";
                    var offsetY = Mathf.Abs(EditorGUILayout.FloatField(label, config.cellPositionOffset.y, GUILayout.MinWidth(60)));

                    config.cellPositionOffset = new Vector2(offsetX, offsetY);

                    GUILayout.Label("", GUILayout.MaxWidth(60));

                    EditorGUIUtility.labelWidth = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(2);
                
                _showCellTransform = EditorGUILayout.Foldout(_showCellTransform, "Cell Transform");

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUILayout.BeginVertical();
                    {
                        if (_showCellTransform)
                        {
                            config.cellRotation = EditorGUILayout.Vector3Field("Rotation", config.cellRotation);
                            config.cellScale = EditorGUILayout.Vector3Field("Scale", config.cellScale);
                            GUILayout.Space(12);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
                
                _showLevelTransform = EditorGUILayout.Foldout(_showLevelTransform, "Level Transform");

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUILayout.BeginVertical();
                    {
                        if (_showLevelTransform)
                        {
                            config.levelPosition = EditorGUILayout.Vector3Field("Position", config.levelPosition);
                            config.levelRotation = EditorGUILayout.Vector3Field("Rotation", config.levelRotation);
                            config.levelScale = EditorGUILayout.Vector3Field("Scale", config.levelScale);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(12);
                
                _showExperimentalSettings = EditorGUILayout.Foldout(_showExperimentalSettings, "Experimental Features");

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUILayout.BeginVertical();
                    {
                        if (_showExperimentalSettings)
                        {
                            config.disableSceneCaching = EditorGUILayout.ToggleLeft(new GUIContent("Disable Scene Caching", "Disables scene caching. Scene caching exists in order to save info on the levels generated between instances. It is not recommended to turn off."), config.disableSceneCaching);
                            config.automaticSave = EditorGUILayout.ToggleLeft(new GUIContent("Automatic Save", "Automatically save the scene after a level is generated."), config.automaticSave);
                            config.automaticOcclusionCulling = EditorGUILayout.ToggleLeft(new GUIContent("Automatic Occlusion Culling", "Automatically re-bakes the occlusion culling after a level is generated. Requires some objects in the room template prefabs to be static."), config.automaticOcclusionCulling);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();   

                GUILayout.Space(12);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Edit Room Templates", GUILayout.MinHeight(25), GUILayout.MinWidth(200)))
                        ERoomTemplates.Initialize(config);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(12);
            }
            GUILayout.EndVertical();
        }
    }
}
