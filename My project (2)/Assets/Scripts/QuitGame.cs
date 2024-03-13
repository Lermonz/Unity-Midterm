using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class QuitGame : MonoBehaviour
{
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            ExitGame();
        }
        
    }
    void ExitGame() {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else 
            Application.Quit();
        #endif
    }
}
