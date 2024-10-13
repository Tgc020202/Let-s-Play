using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapSelection : MonoBehaviour
{
    public Button previousButton;
    public Button nextButton;
    public Button mapButton;
    public Text mapText;

    private int currentMapIndex = 1;
    private const int maxMaps = 4;

    void Start()
    {
        // Ensure everything is assigned
        if (previousButton == null || nextButton == null || mapButton == null || mapText == null)
        {
            Debug.LogError("UI elements are not assigned in the Inspector.");
            return;
        }

        // Initialize the map name and button states
        UpdateMapText();
        previousButton.interactable = false; // Disable Previous button initially

        // Add button listeners
        nextButton.onClick.AddListener(OnNextClicked);
        previousButton.onClick.AddListener(OnPreviousClicked);
        mapButton.onClick.AddListener(OnMapButtonClicked); // Handle the map button click
    }

    void OnNextClicked()
    {
        // Increment map index if not at max
        if (currentMapIndex < maxMaps)
        {
            currentMapIndex++;
            UpdateMapText();
        }

        // Update button states
        previousButton.interactable = currentMapIndex > 1;
        nextButton.interactable = currentMapIndex < maxMaps;
    }

    void OnPreviousClicked()
    {
        // Decrement map index if not at minimum
        if (currentMapIndex > 1)
        {
            currentMapIndex--;
            UpdateMapText();
        }

        // Update button states
        previousButton.interactable = currentMapIndex > 1;
        nextButton.interactable = currentMapIndex < maxMaps;
    }

    void UpdateMapText()
    {
        // Update the displayed map text
        mapText.text = "Map " + currentMapIndex.ToString(); // Change to "Map 1", "Map 2", etc.
    }

    void OnMapButtonClicked()
    {
        Debug.Log("testing");
    }
}
