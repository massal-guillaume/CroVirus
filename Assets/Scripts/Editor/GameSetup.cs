using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSetup
{
    [MenuItem("Tools/Setup Game Scene")]
    public static void SetupGameScene()
    {
        // Créer une nouvelle scène
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        
        // Créer un GameObject pour le GameManager
        GameObject gameManagerObj = new GameObject("GameManager");
        gameManagerObj.AddComponent<GameManager>();
        
        // Sauvegarder la scène
        string scenePath = "Assets/Scenes/MainScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        
        Debug.Log("✅ Scène créée et sauvegardée : Assets/Scenes/MainScene.unity");
        Debug.Log("✅ GameManager attaché et prêt à être lancé !");
        Debug.Log("\n>>> Appuyez sur Play (▶) pour tester !");
    }
}
