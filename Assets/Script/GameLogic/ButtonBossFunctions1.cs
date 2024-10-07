using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonBossFunctions : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Button runButton;
    public Button catchButton;

    private bool canUseRunButton = true;

    void Start()
    {
        // Assign button click listeners
        runButton.onClick.AddListener(OnRunButtonClicked);
        catchButton.onClick.AddListener(OnCatchButtonClicked);
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
        yield return new WaitForSeconds(5);
        playerMovement.speed -= 5;
        yield return new WaitForSeconds(10); // Wait additional 10 seconds to make it 15 seconds total
        canUseRunButton = true;
    }
}
