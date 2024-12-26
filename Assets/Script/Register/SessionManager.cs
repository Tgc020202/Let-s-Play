using UnityEngine;
using Firebase.Database;

public class SessionManager : MonoBehaviour
{
    // Defines
    public string username; // here

    // Instance
    public static SessionManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        if (!string.IsNullOrEmpty(username))
        {
            DatabaseManager.Instance.UpdateUserStatus(username, false);
        }
    }
}
