using UnityEngine;
using Firebase.Database;

public class SessionManager : MonoBehaviour
{
    // Scripts
    public DatabaseManager dbManager;

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
            dbManager.UpdateUserStatus(username, false);
        }
    }
}
