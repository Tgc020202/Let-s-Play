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

    // Panels
    public GameObject TotalNumberUI;
    public GameObject MapUI;
    public GameObject ModeUI;

    public int bossCount;
    public int staffCount;
    public int totalNumberOfPlayer;

    void Start()
    {
        // Default numbers
        bossCount = VariableHolder.defaultMaxNumberOfBosses;
        staffCount = VariableHolder.defaultMaxNumberOfWorkers;
        totalNumberOfPlayer = VariableHolder.defaultTotalNumberOfPlayer;

        bossNextButton.onClick.AddListener(() => ChangeBossCount(1));
        bossPreviousButton.onClick.AddListener(() => ChangeBossCount(-1));
        staffNextButton.onClick.AddListener(() => ChangeStaffCount(1));
        staffPreviousButton.onClick.AddListener(() => ChangeStaffCount(-1));

        backButton.onClick.AddListener(OnBackButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);

        UpdateBossStaffText();
    }

    public void ChangeBossCount(int change)
    {
        bossCount = Mathf.Clamp(bossCount + change, 0, staffCount);
        UpdateBossStaffText();
    }

    public void ChangeStaffCount(int change)
    {
        staffCount = Mathf.Max(1, staffCount + change);

        if (bossCount > staffCount)
        {
            bossCount = staffCount;
        }

        UpdateBossStaffText();
    }

    private void UpdateBossStaffText()
    {
        bossCountText.text = bossCount.ToString();
        staffCountText.text = staffCount.ToString();
        totalNumberOfPlayer = bossCount + staffCount;
        sumText.text = "Total Player: " + (totalNumberOfPlayer).ToString();

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
