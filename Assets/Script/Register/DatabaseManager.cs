using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;

public class DatabaseManager : MonoBehaviour
{
    // Scripts
    private DatabaseReference dbReference;
    
    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void CreateUser(string username, string password, bool status)
    {
        User newUser = new User(username, password, status);
        string json = JsonUtility.ToJson(newUser);
        dbReference.Child("users").Child(username).SetRawJsonValueAsync(json);
    }

    public IEnumerator GetUser(string username, Action<User> callback)
    {
        var userData = dbReference.Child("users").Child(username).GetValueAsync();

        yield return new WaitUntil(() => userData.IsCompleted);

        if (userData != null && userData.Result.Exists)
        {
            DataSnapshot snapshot = userData.Result;
            string dbUsername = snapshot.Child("username").Value.ToString();
            string dbPassword = snapshot.Child("password").Value.ToString();
            bool dbStatus = Convert.ToBoolean(snapshot.Child("status").Value);

            User user = new User(dbUsername, dbPassword, dbStatus);
            callback(user);
        }
        else
        {
            callback(null);
        }
    }

    public IEnumerator CheckUsernameExists(string username, Action<bool> callback)
    {
        var userData = dbReference.Child("users").Child(username).GetValueAsync();

        yield return new WaitUntil(() => userData.IsCompleted);

        if (userData != null && userData.Result.Exists)
        {
            callback(true);
        }
        else
        {
            callback(false);
        }
    }

    public void UpdateUserStatus(string username, bool status)
    {
        dbReference.Child("users").Child(username).Child("status").SetValueAsync(status)
            .ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"Successfully updated status for {username} to {status}");
                }
                else
                {
                    Debug.LogError($"Failed to update status for {username}: {task.Exception}");
                }
            });
    }
}