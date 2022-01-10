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
            GetWindow<ERoomTemplates>(false, "Room Templates", true);
        }

        private void OnGUI()
        {

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.Space(12);

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Room Templates");
                    GUILayout.FlexibleSpace();
                }
            
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                for(var i = 0; i < _config.roomTemplates.Count; i++)
                {
                    using (new GUILayout.VerticalScope("box"))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(4);

                            GUILayout.Label("Room " + (i + 1));

                            if (GUILayout.Button("Delete Room", GUILayout.MinHeight(20), GUILayout.MaxWidth(100)))
                            {
                                _config.roomTemplates.RemoveAt(i);
                                continue;
                            }

                            GUILayout.Space(4);
                        }
                    
                        GUILayout.Space(12);

                        using (new GUILayout.HorizontalScope())
                        {
                            using (new GUILayout.VerticalScope(GUILayout.Height(120)))
                            {
                                GUILayout.Label("Room Prefab");

                                GUILayout.Label("Room Exits");

                                GUILayout.FlexibleSpace();

                                _config.roomTemplates[i].isEssential = EditorGUILayout.ToggleLeft("Essential", _config.roomTemplates[i].isEssential, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                                _config.roomTemplates[i].isStartRoom = EditorGUILayout.ToggleLeft("Start Room", _config.roomTemplates[i].isStartRoom, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                            }

                            GUILayout.FlexibleSpace();

                            using (new GUILayout.VerticalScope())
                            {
                                _config.roomTemplates[i].prefab = (GameObject)EditorGUILayout.ObjectField(_config.roomTemplates[i].prefab, typeof(GameObject), false, GUILayout.MaxWidth(220));

                                using (new GUILayout.HorizontalScope())
                                {
                                    if (_config.roomTemplates[i].exitDirections.Count >= 4)
                                        GUI.enabled = false;
                                    if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
                                        _config.roomTemplates[i].exitDirections.Add(ExitDirection.Top);

                                    GUI.enabled = true;

                                    using (new GUILayout.VerticalScope())
                                    {
                                        for (var d = 0; d < _config.roomTemplates[i].exitDirections.Count; d++)
                                        {
                                            using (new GUILayout.HorizontalScope())
                                            {
                                                var direction = _config.roomTemplates[i].exitDirections[d];
                                                direction = (ExitDirection)EditorGUILayout.EnumPopup(direction, GUILayout.MaxWidth(70));
                                                _config.roomTemplates[i].exitDirections[d] = direction;

                                                if (_config.roomTemplates[i].exitDirections.Count < 2)
                                                    GUI.enabled = false;
                                                if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
                                                    _config.roomTemplates[i].exitDirections.RemoveAt(d);

                                                GUI.enabled = true;
                                            }
                                        }
                                    }

                                    Texture2D texture = AssetPreview.GetAssetPreview(_config.roomTemplates[i].prefab);
                                    if (texture)
                                        GUILayout.Label(texture, GUILayout.MaxHeight(100));
                                }
                            }
                        }
                    }

                    GUILayout.Space(12);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Add Template", GUILayout.MinHeight(25), GUILayout.MaxWidth(110)))
                    {
                        var room = new GridRoomLayout();
                        room.exitDirections.Add(ExitDirection.Top);
                        _config.roomTemplates.Add(room);
                    }
                
                    GUILayout.FlexibleSpace();
                }
            
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