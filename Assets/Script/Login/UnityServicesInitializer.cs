using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

public class UnityServicesInitializer : MonoBehaviour
{
    // This will initialize Unity Services and handle the authentication
    public async void InitializeServices()
    {
        try
        {
            // Initialize Unity Services (ensure you have enabled them in the Unity Dashboard)
            await UnityServices.InitializeAsync();

            // Check if the user is signed in, if not, sign in anonymously
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Unity Services Initialized and User Signed In");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Unity Services: " + e.Message);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Call the initialization function when the game starts
        InitializeServices();
    }
}
