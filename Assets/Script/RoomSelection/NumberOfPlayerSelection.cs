using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberOfPlayerSelection : MonoBehaviour
{
    public Button bossNextButton;
    public Button bossPreviousButton;
    public Button staffNextButton;
    public Button staffPreviousButton;
    public Button nextButton;
    public Button backButton;
    public Text bossCountText;
    public Text staffCountText;
    public Text sumText;
    public GameObject TotalNumberUI;
    public GameObject MapUI;
    public GameObject ModeUI;

    private int bossCount = 1;
    private int staffCount = 3;

    void Start()
    {
        // Assign listeners for boss and staff count adjustment
        bossNextButton.onClick.AddListener(() => ChangeBossCount(1));
        bossPreviousButton.onClick.AddListener(() => ChangeBossCount(-1));
        staffNextButton.onClick.AddListener(() => ChangeStaffCount(1));
        staffPreviousButton.onClick.AddListener(() => ChangeStaffCount(-1));

        // Assign setup selections
        backButton.onClick.AddListener(OnBackButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);

        // Initialize the text display
        UpdateBossStaffText();
    }

    public void ChangeBossCount(int change)
    {
        // Prevent the count from going below 0 or exceeding staffCount
        bossCount = Mathf.Clamp(bossCount + change, 0, staffCount);
        UpdateBossStaffText();
    }

    public void ChangeStaffCount(int change)
    {
        // Prevent staff count from going below 1
        staffCount = Mathf.Max(1, staffCount + change);

        // Ensure boss count does not exceed staff count
        if (bossCount > staffCount)
        {
            bossCount = staffCount;
        }

        UpdateBossStaffText();
    }

    private void UpdateBossStaffText()
    {
        // Update the displayed text
        bossCountText.text = bossCount.ToString();
        staffCountText.text = staffCount.ToString();
        VariableHolder.totalNumberOfPlayer = bossCount + staffCount;
        sumText.text = "Total Player: " + (VariableHolder.totalNumberOfPlayer).ToString();

        // Enable/Disable buttons
        bossNextButton.interactable = bossCount < staffCount;
        bossPreviousButton.interactable = bossCount > 1;

        staffPreviousButton.interactable = staffCount > 1;
    }

    void OnBackButtonClicked()
    {
        TotalNumberUI.SetActive(false);
        MapUI.SetActive(true);
        ModeUI.SetActive(false);
    }

    void OnNextButtonClicked()
    {
        TotalNumberUI.SetActive(false);
        MapUI.SetActive(false);
        ModeUI.SetActive(true);
    }
}
