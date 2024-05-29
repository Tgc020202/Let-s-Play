using UnityEngine;
using UnityEngine.SceneManagement;

public class CarCollisionHandler2D : MonoBehaviour
{
    private string nextScene;

    public void SetNextScene(string sceneName)
    {
        nextScene = sceneName;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("outside");

        if (collision.gameObject.name == "Goal")
        {
            Debug.Log("Inside");
            // Move to the specified scene
            SceneManager.LoadScene(nextScene);
        }
    }
}
