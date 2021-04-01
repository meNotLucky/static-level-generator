using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelGenerator.Utility
{
    public class SceneCache
    {
        public string sceneName;
        public string sceneID;
        public string levelObjectID;
        public string lastSeed;
    }

    public static class SceneCacheUtility
    {
        private const string CachePath = "Library/LevelGeneratorCache/";
        public static SceneCache GetCache(Scene scene)
        {
            var sceneID = AssetDatabase.AssetPathToGUID(scene.path);
            var path = CachePath + sceneID + ".xml";
            
            if (!File.Exists(path))
                return null;
            
            var serializer = new XmlSerializer(typeof(SceneCache));
            using (var reader = new StreamReader(path))
                return (SceneCache) serializer.Deserialize(reader);
        }
        
        public static SceneCache SaveCache(SceneCache cache)
        {  
            var writer = new XmlSerializer(typeof(SceneCache));
        
            if(!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
            
            var path = CachePath + cache.sceneID + ".xml";
            var file = File.Create(path);  

            writer.Serialize(file, cache);
            file.Close();

            return cache;
        }
    }
}