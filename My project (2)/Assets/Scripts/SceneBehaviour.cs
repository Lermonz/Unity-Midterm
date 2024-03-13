using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBehaviour : MonoBehaviour
{
    void Update()
    {
        if(Input.anyKeyDown){
            SceneManager.LoadScene("Gameplay");
        }
    }
}
