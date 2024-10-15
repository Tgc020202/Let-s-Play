using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonBossFunctions : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Button runButton;
    public Button catchButton;
    public GameObject Player;
    public GameObject GameViewUI;
    public GameObject MapDesign;
    public GameObject GuidanceUI;

    private bool canUseRunButton = true;
    private Text runButtonText;
    private float timer = 5f;

    void Start()
    {
        // Assign button click listeners
        runButton.onClick.AddListener(OnRunButtonClicked);
        catchButton.onClick.AddListener(OnCatchButtonClicked);

        runButtonText = runButton.GetComponentInChildren<Text>();
        Player.SetActive(false);
        GameViewUI.SetActive(false);
        MapDesign.SetActive(false);
    }

    void Update()
    {
        // Press Space key to skip guidance
        if (Input.GetKey(KeyCode.Space)) OnSkipGuidanceUI();

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            OnSkipGuidanceUI();
        }
    }

    void OnRunButtonClicked()
    {
        if (canUseRunButton)
        {
            StartCoroutine(SpeedBoost());
        }
    }

    void OnCatchButtonClicked()
    {
        Debug.Log("Catch someone.");
    }

    IEnumerator SpeedBoost()
    {
        canUseRunButton = false;
        playerMovement.speed += 5;
        // Countdown from 20 seconds
        for (int i = 20; i > 0; i--)
        {
            runButtonText.text = i + "s"; // Update button text to show countdown
            yield return new WaitForSeconds(1); // Wait 1 second per count
            if (i == 16)
            {
                playerMovement.speed -= 5;
            }
        }
        runButtonText.text = "Run";
        canUseRunButton = true;
    }

    void OnSkipGuidanceUI()
    {
        Player.SetActive(true);
        MapDesign.SetActive(true);
        GameViewUI.SetActive(true);
        GuidanceUI.SetActive(false);
    }
}
