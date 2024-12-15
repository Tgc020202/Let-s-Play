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
        backButon.onClick.AddListener(OnBackButonClicked);

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
        VariableHolder.modeCode = "Map" + currentMapIndex.ToString();
    }

    void OnMapButtonClicked()
    {
        TotalNumberUI.SetActive(true);
        MapUI.SetActive(false);
        ModeUI.SetActive(false);
    }

    void OnBackButonClicked()
    {
        TotalNumberUI.SetActive(false);
        MapUI.SetActive(false);
        ModeUI.SetActive(false);
        PlayerSetupUI.SetActive(true);
        RoomSetupUI.SetActive(false);
    }
}
