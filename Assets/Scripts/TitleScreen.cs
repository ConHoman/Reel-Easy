using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public string sceneToLoad = "scene_6"; // change this to your actual gameplay scene name

    void Update()
    {
        Debug.Log("Title Screen Running");  // <-- Add this

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - loading scene"); // <-- Add this
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
