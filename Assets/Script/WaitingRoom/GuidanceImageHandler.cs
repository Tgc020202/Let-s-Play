using UnityEngine;
using UnityEngine.UI;

public class GuidanceImageHandler : MonoBehaviour
{
    // UI Components
    public Button showGuidanceButton;

    // GameObjects
    public GameObject guidanceImage;

    // Defines
    private bool isGuidanceVisible = false;

    void Start()
    {
        guidanceImage.SetActive(false);

        showGuidanceButton.onClick.AddListener(ToggleGuidanceImage);
    }

    void Update()
    {
        if (isGuidanceVisible && Input.GetMouseButtonDown(0))
        {
            CloseGuidanceImage();
        }
    }

    private void ToggleGuidanceImage()
    {
        isGuidanceVisible = !isGuidanceVisible;
        guidanceImage.SetActive(isGuidanceVisible);
    }

    private void CloseGuidanceImage()
    {
        isGuidanceVisible = false;
        guidanceImage.SetActive(false);
    }
}
