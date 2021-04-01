using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LevelGenerator.Utility
{
    public static class EventHandler
    {
        // delegates
        public delegate void GeneratorLostReferenceCallback(Scene scene, OpenSceneMode sceneMode);

        // events
        public static event GeneratorLostReferenceCallback generatorLostReference;
        
        // event invocators
        private static void OnGeneratorLostReference(Scene scene, OpenSceneMode sceneMode)
        {
            generatorLostReference?.Invoke(scene, sceneMode);
        }
        
        // methods
        
        public static void GeneratorLostReference()
        {
            OnGeneratorLostReference(SceneManager.GetActiveScene(), OpenSceneMode.Single);
        }
    }
}
