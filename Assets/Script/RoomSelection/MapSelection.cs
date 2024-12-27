using UnityEngine;
using UnityEngine.UI;

public class MapSelection : MonoBehaviour
{
    // UI Components
    public Button previousButton;
    public Button nextButton;
    public Button mapButton;
    public Button backButon;
    public Text mapText;

    // GameObjects
    public GameObject TotalNumberUI;
    public GameObject MapUI;
    public GameObject ModeUI;
    public GameObject PlayerSetupUI;
    public GameObject RoomSetupUI;

    // Defines
    public int currentMapIndex = 1;
    private const int maxMaps = 4;

    void Start()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        previousButton.onClick.AddListener(OnPreviousClicked);
        mapButton.onClick.AddListener(OnMapButtonClicked);
        backButon.onClick.AddListener(OnBackButtonClicked);

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
        switch (currentMapIndex)
        {
            case 1:
                mapText.text = "Small Map";
                break;
            case 2:
                mapText.text = "Large Map";
                break;
            default:
                mapText.text = "Random Map";
                break;
        }
    }

    void OnMapButtonClicked()
    {
        TotalNumberUI.SetActive(true);
        MapUI.SetActive(false);
        ModeUI.SetActive(false);
    }

    void OnBackButtonClicked()
    {
        TotalNumberUI.SetActive(false);
        MapUI.SetActive(false);
        ModeUI.SetActive(false);
        PlayerSetupUI.SetActive(true);
        RoomSetupUI.SetActive(false);
    }
}
