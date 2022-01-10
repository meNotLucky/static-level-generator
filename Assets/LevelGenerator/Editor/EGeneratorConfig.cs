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

            using (new GUILayout.VerticalScope("Generator Configuration", "box"))
            {
                GUILayout.Space(12);
                
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope())
                    {
                        config.gridWidth = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Horizontal Grid Size", "The number of cells that are generated horizontally."), config.gridWidth), 1, 500);
                        config.gridHeight = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Vertical Grid Size", "The number of cells that are generated vertically."), config.gridHeight), 1, 500);
                    }

                    EditorGUILayout.HelpBox("The grid size defines the bounds of the grid the level will be generated in. It does not define the total size of the level.", MessageType.None);
                }

                GUILayout.Space(12);

                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope())
                    {
                        var maximumSize = EditorGUILayout.IntField(new GUIContent("Maximum Level Size", "Maximum number of rooms allowed to be generated. May cause long loading times if set too close to minimum size."), config.maxLevelSize);
                        var minimumSize = EditorGUILayout.IntField(new GUIContent("Minimum Level Size", "Minimum number of rooms allowed to be generated. May cause long loading times if set too close to maximum size."), config.minLevelSize);

                        var gridSize = config.gridHeight * config.gridWidth;
                        config.maxLevelSize = Mathf.Clamp(maximumSize, config.minLevelSize, gridSize);
                        config.minLevelSize = Mathf.Clamp(minimumSize, 0, maximumSize);
                    }

                    EditorGUILayout.HelpBox("The maximum and minimum level size defines the largest and smallest number of rooms generated respectively.", MessageType.None);
                }

                if (config.minLevelSize > ((float) (config.gridHeight * config.gridWidth) / 2))
                    EditorGUILayout.HelpBox("A high Minimum Level Size can result in longer load times. It is recommended to keep the minimum size bellow half of the total grid size (" + (config.gridHeight * config.gridWidth) / 2 + ").", MessageType.Warning);

                GUILayout.Space(12);

                EditorGUILayout.LabelField(new GUIContent("Level Density", "Determines how closely packed your rooms will be. Only works when corridors (two-exit-rooms) are included in the templates."));
                config.levelDensity = EditorGUILayout.IntSlider(config.levelDensity, 0, 100);

                GUILayout.Space(12);

                config.gridAlignment = (GridAlignment) EditorGUILayout.EnumPopup(new GUIContent("Grid Alignment", "How the grid should be stacked."), config.gridAlignment);

                GUILayout.Space(12);

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Cell Spacing", "The physical space in between each cell, measured in Unity-units."), GUILayout.ExpandWidth(false), GUILayout.MinWidth(120));

                    EditorGUIUtility.labelWidth = 10;

                    var offsetX = Mathf.Abs(EditorGUILayout.FloatField("X", config.cellPositionOffset.x, GUILayout.MinWidth(60)));
                    
                    GUILayout.Space(2);
                    
                    var label = config.gridAlignment == GridAlignment.Horizontal ? "Z" : "Y";
                    var offsetY = Mathf.Abs(EditorGUILayout.FloatField(label, config.cellPositionOffset.y, GUILayout.MinWidth(60)));

                    config.cellPositionOffset = new Vector2(offsetX, offsetY);

                    GUILayout.Label("", GUILayout.MaxWidth(60));

                    EditorGUIUtility.labelWidth = 0;
                }

                GUILayout.Space(2);
                
                _showCellTransform = EditorGUILayout.Foldout(_showCellTransform, new GUIContent("Cell Transform", "The transform of each individual cell in the grid."));

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(20);
                    using (new GUILayout.VerticalScope())
                    {
                        if (_showCellTransform)
                        {
                            config.cellRotation = EditorGUILayout.Vector3Field("Rotation", config.cellRotation);
                            config.cellScale = EditorGUILayout.Vector3Field("Scale", config.cellScale);
                            GUILayout.Space(12);
                        }
                    }
                }
                
                _showLevelTransform = EditorGUILayout.Foldout(_showLevelTransform, new GUIContent("Level Transform", "The transform of the whole generated level."));

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(20);

                    using (new GUILayout.VerticalScope())
                    {
                        if (_showLevelTransform)
                        {
                            config.levelPosition = EditorGUILayout.Vector3Field("Position", config.levelPosition);
                            config.levelRotation = EditorGUILayout.Vector3Field("Rotation", config.levelRotation);
                            config.levelScale = EditorGUILayout.Vector3Field("Scale", config.levelScale);
                        }
                    }
                }

                GUILayout.Space(12);
                
                _showExperimentalSettings = EditorGUILayout.Foldout(_showExperimentalSettings, new GUIContent("Experimental Features", "Have fun with these, at your own risk."));

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(20);

                    using (new GUILayout.VerticalScope())
                    {
                        if (_showExperimentalSettings)
                        {
                            config.disableSceneCaching = EditorGUILayout.ToggleLeft(new GUIContent("Disable Scene Caching", "Disables scene caching. Scene caching exists in order to save info on the levels generated between instances. It is not recommended to turn off."), config.disableSceneCaching);
                            config.automaticSave = EditorGUILayout.ToggleLeft(new GUIContent("Automatic Save", "Automatically save the scene after a level is generated."), config.automaticSave);
                            config.automaticOcclusionCulling = EditorGUILayout.ToggleLeft(new GUIContent("Automatic Occlusion Culling", "Automatically re-bakes the occlusion culling after a level is generated. Requires some objects in the room template prefabs to be static."), config.automaticOcclusionCulling);
                        }
                    }
                }

                GUILayout.Space(12);

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Edit Room Templates", GUILayout.MinHeight(25), GUILayout.MinWidth(200)))
                        ERoomTemplates.Initialize(config);
                    GUILayout.FlexibleSpace();
                }
                
                GUILayout.Space(12);
            }
        }
    }
}
