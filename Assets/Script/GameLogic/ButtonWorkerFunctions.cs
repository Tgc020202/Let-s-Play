using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonWorkerFunctions : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Button runButton;
    public Button redButton;
    public Button greenButton;

    private bool canUseRunButton = true;

    void Start()
    {
        // Assign button click listeners
        runButton.onClick.AddListener(OnRunButtonClicked);
        redButton.onClick.AddListener(OnRedButtonClicked);
        greenButton.onClick.AddListener(OnGreenButtonClicked);
    }

    void OnRunButtonClicked()
    {
        if (canUseRunButton)
        {
            StartCoroutine(SpeedBoost());
        }
    }

    void OnRedButtonClicked()
    {
        playerMovement.enabled = false; // Disable player movement
    }

    void OnGreenButtonClicked()
    {
        Debug.Log("Help: Use the run button to speed up, red button to stop, green button for help.");
    }

    IEnumerator SpeedBoost()
    {
        canUseRunButton = false;
        playerMovement.speed += 5;
        yield return new WaitForSeconds(5);
        playerMovement.speed -= 5;
        yield return new WaitForSeconds(10); // Wait additional 10 seconds to make it 15 seconds total
        canUseRunButton = true;
    }
}
