using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference dbReference;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void CreateUser(string username, string password)
    {
        User newUser = new User(username, password);
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
            User user = new User(dbUsername, dbPassword);
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
}