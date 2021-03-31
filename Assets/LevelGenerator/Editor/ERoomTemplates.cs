using System.Collections.Generic;
using LevelGenerator.Generator;
using LevelGenerator.Utility;
using UnityEditor;
using UnityEngine;

using ExitDirection = LevelGenerator.Generator.ExitDirection;

namespace LevelGenerator.Editor
{
    public class ERoomTemplates : EditorWindow
    {
        private Vector2 _scrollPos = Vector2.zero;

        private static GeneratorConfig _config;
        
    
        public static void Initialize(GeneratorConfig config)
        {
            _config = config;
            GetWindow<ERoomTemplates>(true, "Room Templates", true);
        }

        private void OnGUI()
        {

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.Space(12);
            
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Room Templates");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                for(var i = 0; i < _config.roomTemplates.Count; i++)
                {
                    GUILayout.BeginVertical("", "box");
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Room " + (i + 1));

                            if (GUILayout.Button("Delete Room", GUILayout.MinHeight(20), GUILayout.MaxWidth(100)))
                            {
                                _config.roomTemplates.RemoveAt(i);
                                continue;
                            }
                        }
                        GUILayout.EndHorizontal();
                    
                        GUILayout.Space(12);

                        _config.roomTemplates[i].prefab = (GameObject)EditorGUILayout.ObjectField("Room Prefab", _config.roomTemplates[i].prefab, typeof(GameObject), false);
                    
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Room Exits");
                        
                            GUILayout.FlexibleSpace();

                            if (_config.roomTemplates[i].exitDirections.Count >= 4)
                                GUI.enabled = false;
                            if (GUILayout.Button("Add Exit", GUILayout.MaxWidth(100)))
                                _config.roomTemplates[i].exitDirections.Add(ExitDirection.Top);

                            GUI.enabled = true;
            
                            GUILayout.BeginVertical();
                            {
                                for(var d = 0; d < _config.roomTemplates[i].exitDirections.Count; d++)
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        var direction = _config.roomTemplates[i].exitDirections[d];
                                        direction = (ExitDirection)EditorGUILayout.EnumPopup(direction, GUILayout.MaxWidth(100));
                                        _config.roomTemplates[i].exitDirections[d] = direction;

                                        if (_config.roomTemplates[i].exitDirections.Count < 2)
                                            GUI.enabled = false;
                                        if (GUILayout.Button("Remove", GUILayout.MaxWidth(70)))
                                            _config.roomTemplates[i].exitDirections.RemoveAt(d);
                                
                                        GUI.enabled = true;  
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();
                    
                        GUILayout.BeginHorizontal();
                        {
                            _config.roomTemplates[i].isEssential = EditorGUILayout.ToggleLeft("Is Essential", _config.roomTemplates[i].isEssential, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                            if(_config.roomTemplates[i].isEssential)
                                _config.roomTemplates[i].hasFixedPosition = EditorGUILayout.ToggleLeft("Has Fixed Position", _config.roomTemplates[i].hasFixedPosition);
                        }
                        GUILayout.EndHorizontal();

                        if (_config.roomTemplates[i].isEssential && _config.roomTemplates[i].hasFixedPosition)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("Grid Position", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(100));
                            
                                EditorGUIUtility.labelWidth = 10;
                            
                                var posX = EditorGUILayout.IntField("X", (int)_config.roomTemplates[i].fixedPosition.x + 1, GUILayout.MaxWidth(80));
                                posX = Mathf.Clamp(posX, 1, _config.gridWidth);
                                GUILayout.Space(2);
                                var label = _config.gridAlignment == GridAlignment.Horizontal ? "Z" : "Y";
                                var posY = EditorGUILayout.IntField(label, (int)_config.roomTemplates[i].fixedPosition.y + 1, GUILayout.MaxWidth(80));
                                posY = Mathf.Clamp(posY, 1, _config.gridHeight);
                            
                                _config.roomTemplates[i].fixedPosition = new Vector2(posX - 1, posY - 1);
                            
                                GUILayout.Label("", GUILayout.MaxWidth(60));
                            
                                EditorGUIUtility.labelWidth = 0;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();
                
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                }
            
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Add Room", GUILayout.MinHeight(25), GUILayout.MaxWidth(110)))
                    {
                        var room = new GridRoom();
                        room.exitDirections.Add(ExitDirection.Top);
                        _config.roomTemplates.Add(room);
                    }
                
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            
                GUILayout.Space(12);

                EditorGUILayout.EndScrollView();
            }
            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
                //ConfigurationSerializer.Serialize(_config);
            }
        }
    }
}