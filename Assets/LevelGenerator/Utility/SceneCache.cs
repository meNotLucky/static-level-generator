using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelGenerator.Utility
{
    public class SceneCache : ScriptableObject
    {
        public string sceneID = "";
        
        public CacheData editModeCache = new CacheData();
        public CacheData playModeCache = new CacheData();
    }

    [System.Serializable]
    public class CacheData
    {
        public string levelObjectID;
        public string levelSeed;
    }

    public static class SceneCacheUtility
    {
        private const string CachePath = "Library/LevelGeneratorCache/";
        public static SceneCache DeserializeCache(Scene scene)
        {
            var sceneID = AssetDatabase.AssetPathToGUID(scene.path);
            var path = CachePath + sceneID + ".json";

            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            var cache = ScriptableObject.CreateInstance<SceneCache>();
            JsonUtility.FromJsonOverwrite(json, cache);
            return cache;
        }
        
        public static SceneCache SerializeCache(SceneCache cache)
        {
            if(!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
            
            var path = CachePath + cache.sceneID + ".json";
            var json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(path, json);

            return cache;
        }
    }
}