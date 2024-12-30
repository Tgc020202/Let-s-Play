using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SpectacleUIController : MonoBehaviour
{
    public Text roleText;
    public GameObject BossControllerUI;
    public GameObject WorkerControllerUI;
    public GameObject SpectacleUI;

    private Renderer playerRenderer;
    private bool wasRendererEnabled;

    private bool playerPrefabInstantiated = false;

    void Start()
    {
        StartCoroutine(WaitForPlayerPrefab());
    }

    private IEnumerator WaitForPlayerPrefab()
    {
        while (!playerPrefabInstantiated)
        {
            foreach (var playerObject in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (playerObject.GetComponent<NetworkObject>().IsOwner)
                {
                    playerRenderer = playerObject.GetComponent<Renderer>();
                    if (playerRenderer == null)
                    {
                        Debug.LogError($"Renderer component not found on {playerObject.name}.");
                        yield break;
                    }

                    wasRendererEnabled = playerRenderer.enabled;

                    if (SpectacleUI != null)
                    {
                        SpectacleUI.SetActive(false);
                    }
                    else
                    {
                        Debug.LogError("SpectacleUI reference is not assigned.");
                    }

                    playerPrefabInstantiated = true;
                    break;
                }
            }

            yield return null;
        }
    }

    void Update()
    {
        if (playerRenderer == null || SpectacleUI == null)
            return;

        if (playerRenderer.enabled != wasRendererEnabled)
        {
            wasRendererEnabled = playerRenderer.enabled;

            if (!playerRenderer.enabled)
            {
                roleText.gameObject.SetActive(false);
                BossControllerUI?.SetActive(false);
                WorkerControllerUI?.SetActive(false);
                SpectacleUI.SetActive(true);
            }
            else
            {
                SpectacleUI.SetActive(false);
            }
        }
    }
}
