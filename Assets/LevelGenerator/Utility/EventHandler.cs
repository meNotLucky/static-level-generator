using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LevelGenerator.Utility
{
    public static class EventHandler
    {
        // delegates
        public delegate void GeneratorRecompililationCallback();

        // events
        public static event GeneratorRecompililationCallback generatorRecompiled;
        
        
        // event invocators
        public static void OnGeneratorRecompile()
        {
            generatorRecompiled?.Invoke();
        }
    }
}
