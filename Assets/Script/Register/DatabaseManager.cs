using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;

public class DatabaseManager : MonoBehaviour
{
    public InputField Username;
    public InputField Password;
    private string userID;
    private DatabaseReference dbReference;
    void Start()
    {
        userID = SystemInfo.deviceUniqueIdentifier;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void CreateUser()
    {
        User newUser = new User(Username.text, Password.text);
        string json = JsonUtility.ToJson(newUser);
        dbReference.Child("users").Child(userID).SetRawJsonValueAsync(json);
    }
}
