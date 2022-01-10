using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using LevelGenerator.Generator;

namespace LevelGenerator.Utility
{
    public enum GridAlignment { Horizontal, Vertical }

    [System.Serializable]
    public class GeneratorConfig : ScriptableObject
    {
        public int gridWidth = 5, gridHeight = 5;
        public int minLevelSize = 6;
        public int maxLevelSize = 12;

        public int levelDensity = 50;

        public bool forcedLevelGeneration = false;

        public GridAlignment gridAlignment = GridAlignment.Horizontal;
    
        public Vector2 cellPositionOffset = new Vector2(1.0f, 1.0f);
    
        public Vector3 cellRotation = new Vector3();
        public Vector3 cellScale = new Vector3(1.0f, 1.0f, 1.0f);
    
        public Vector3 levelPosition = new Vector3();
        public Vector3 levelRotation = new Vector3();
        public Vector3 levelScale = new Vector3(1.0f, 1.0f, 1.0f);

        public List<GridRoom> roomTemplates = new List<GridRoom>();
        
        // EXPERIMENTAL

        public bool disableSceneCaching;
        public bool automaticSave;
        public bool automaticOcclusionCulling;
    }
    
    public static class GeneratorConfigUtility
    {
        public static GeneratorConfig GetConfiguration(string config)
        {
            config += ".asset";
            var configs = AssetDatabase.FindAssets("t:GeneratorConfig");
            return (from cfg in configs select AssetDatabase.GUIDToAssetPath(cfg)
                into path where Path.GetFileName(path) == config select AssetDatabase.LoadAssetAtPath<GeneratorConfig>(path)).FirstOrDefault();
        }

        public static GeneratorConfig CreateConfiguration(string name)
        {
            // if(GetConfiguration(name) != null)
            //     Debug.LogWarning("Level Configuration '" + name + "' already exists and will be overwritten.");

            var config = ScriptableObject.CreateInstance<GeneratorConfig>();
            AssetDatabase.CreateAsset(config, "Assets/LevelGenerator/Configurations/" + name + ".asset");
            AssetDatabase.SaveAssets();

            return config;
        }
    }
}
