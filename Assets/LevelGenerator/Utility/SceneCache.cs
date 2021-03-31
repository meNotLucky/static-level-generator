using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

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
        public static SceneCache GetCache(string config)
        {
            if (config == null) return default;
            var path = "Library/LevelGeneratorCache/" + config + ".xml";
            var serializer = new XmlSerializer(typeof(SceneCache));
            using (var reader = new StreamReader(path))
            {
                return (SceneCache) serializer.Deserialize(reader);
            }
        }
        
        public static void SaveCache(SceneCache cache)
        {  
            var writer = new XmlSerializer(typeof(SceneCache));
        
            Directory.CreateDirectory("Library/LevelGeneratorCache");
            var path = "Library/LevelGeneratorCache/" + cache.sceneID + ".xml";
            var file = File.Create(path);  

            writer.Serialize(file, cache);
            file.Close();
        }
    }
}