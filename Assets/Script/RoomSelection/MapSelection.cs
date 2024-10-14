using UnityEngine;
using UnityEngine.UI;

public class MapSelection : MonoBehaviour
{
    public Button previousButton;
    public Button nextButton;
    public Button mapButton;
    public Text mapText;
    public GameObject TotalNumberUI;
    public GameObject MapUI;
    public GameObject ModeUI;

    private int currentMapIndex = 1;
    private const int maxMaps = 4;

    void Start()
    {
        // Assign listeners for map selection
        nextButton.onClick.AddListener(OnNextClicked);
        previousButton.onClick.AddListener(OnPreviousClicked);
        mapButton.onClick.AddListener(OnMapButtonClicked);

        // Initialize the map name and button states
        UpdateMapText();
        previousButton.interactable = false;

        TotalNumberUI.SetActive(false);
        ModeUI.SetActive(false);
    }

    void OnNextClicked()
    {
        if (currentMapIndex < maxMaps)
        {
            currentMapIndex++;
            UpdateMapText();
        }
        previousButton.interactable = currentMapIndex > 1;
        nextButton.interactable = currentMapIndex < maxMaps;
    }

    void OnPreviousClicked()
    {
        if (currentMapIndex > 1)
        {
            currentMapIndex--;
            UpdateMapText();
        }
        previousButton.interactable = currentMapIndex > 1;
        nextButton.interactable = currentMapIndex < maxMaps;
    }

    void UpdateMapText()
    {
        mapText.text = "Map " + currentMapIndex.ToString();
    }

    void OnMapButtonClicked()
    {
        TotalNumberUI.SetActive(true);
        MapUI.SetActive(false);
        ModeUI.SetActive(false);
    }
}
