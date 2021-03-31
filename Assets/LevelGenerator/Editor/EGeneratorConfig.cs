using LevelGenerator.Generator;
using LevelGenerator.Utility;
using UnityEngine;
using UnityEditor;

namespace LevelGenerator.Editor
{
    [CustomEditor(typeof(GeneratorConfig))]
    public class EGeneratorConfig : UnityEditor.Editor
    {
        private bool _showCellTransform;
        private bool _showLevelTransform;
        
        public override void OnInspectorGUI()
        {
            var config = (GeneratorConfig)target;

            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginVertical("Generator Settings", "box");
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

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(12);
                        GUILayout.BeginVertical();
                        {
                            _showCellTransform = EditorGUILayout.Foldout(_showCellTransform, "Cell Transform");

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

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(12);
                        GUILayout.BeginVertical();
                        {
                            _showLevelTransform = EditorGUILayout.Foldout(_showLevelTransform, "Level Transform");

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

                    if (GUILayout.Button("Edit Room Templates", GUILayout.MinHeight(25)))
                    {
                        ERoomTemplates.Initialize(config);
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
