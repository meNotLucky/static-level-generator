using LevelGenerator.Generator;
using LevelGenerator.Utility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelGenerator.Editor
{
    public class ERoomTemplates : EditorWindow
    {
        private static EGridLevelGenerator _editor;
        private static Object _target;
    
        private Vector2 _scrollPos = Vector2.zero;
    
        public static void Initialize(EGridLevelGenerator editor)
        {
            _editor = editor;
            _target = editor.target;
            GetWindow<ERoomTemplates>(true, "Room Templates", true);
        }

        private void OnGUI()
        {
            if (_target == null)
            {
                this.Close();
                return;
            }
        
            var levelGen = (GridLevelGenerator)_target;
        
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

                for(var i = 0; i < levelGen.roomTemplates.Count; i++)
                {
                    GUILayout.BeginVertical("", "box");
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Room " + (i + 1));

                            if (GUILayout.Button("Delete Room", GUILayout.MinHeight(20), GUILayout.MaxWidth(100)))
                            {
                                levelGen.roomTemplates.RemoveAt(i);
                                continue;
                            }
                        }
                        GUILayout.EndHorizontal();
                    
                        GUILayout.Space(12);

                        levelGen.roomTemplates[i].prefab = (GameObject)EditorGUILayout.ObjectField("Room Prefab", levelGen.roomTemplates[i].prefab, typeof(GameObject), false);
                    
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Room Exits");
                        
                            GUILayout.FlexibleSpace();

                            if (levelGen.roomTemplates[i].exitDirections.Count >= 4)
                                GUI.enabled = false;
                            if (GUILayout.Button("Add Exit", GUILayout.MaxWidth(100)))
                                levelGen.roomTemplates[i].exitDirections.Add(ExitDirection.Top);

                            GUI.enabled = true;
            
                            GUILayout.BeginVertical();
                            {
                                for(var d = 0; d < levelGen.roomTemplates[i].exitDirections.Count; d++)
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        var direction = levelGen.roomTemplates[i].exitDirections[d];
                                        direction = (ExitDirection)EditorGUILayout.EnumPopup(direction, GUILayout.MaxWidth(100));
                                        levelGen.roomTemplates[i].exitDirections[d] = direction;

                                        if (levelGen.roomTemplates[i].exitDirections.Count < 2)
                                            GUI.enabled = false;
                                        if (GUILayout.Button("Remove", GUILayout.MaxWidth(70)))
                                            levelGen.roomTemplates[i].exitDirections.RemoveAt(d);
                                
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
                            levelGen.roomTemplates[i].isEssential = EditorGUILayout.ToggleLeft("Is Essential", levelGen.roomTemplates[i].isEssential, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                            if(levelGen.roomTemplates[i].isEssential)
                                levelGen.roomTemplates[i].hasFixedPosition = EditorGUILayout.ToggleLeft("Has Fixed Position", levelGen.roomTemplates[i].hasFixedPosition);
                        }
                        GUILayout.EndHorizontal();

                        if (levelGen.roomTemplates[i].isEssential && levelGen.roomTemplates[i].hasFixedPosition)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("Grid Position", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(100));
                            
                                EditorGUIUtility.labelWidth = 10;
                            
                                var posX = EditorGUILayout.IntField("X", (int)levelGen.roomTemplates[i].fixedPosition.x + 1, GUILayout.MaxWidth(80));
                                posX = Mathf.Clamp(posX, 1, levelGen.gridWidth);
                                GUILayout.Space(2);
                                var label = levelGen.gridAlignment == GridAlignment.Horizontal ? "Z" : "Y";
                                var posY = EditorGUILayout.IntField(label, (int)levelGen.roomTemplates[i].fixedPosition.y + 1, GUILayout.MaxWidth(80));
                                posY = Mathf.Clamp(posY, 1, levelGen.gridHeight);
                            
                                levelGen.roomTemplates[i].fixedPosition = new Vector2(posX - 1, posY - 1);
                            
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
                        levelGen.roomTemplates.Add(room);
                    }
                
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            
                GUILayout.Space(12);

                EditorGUILayout.EndScrollView();
            }
            if (EditorGUI.EndChangeCheck())
            {
                _editor.settingsChangedWarning = true;
                EditorUtility.SetDirty(_target);
            }
        }
    }
}