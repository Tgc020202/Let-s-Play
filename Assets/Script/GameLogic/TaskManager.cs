using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TaskManager : NetworkBehaviour
{
    // UI Components
    public Text taskText;

    // Scripts
    public GameViewTextBehaviour gameViewTextBehaviour;
    public GameManager gameManager;

    // Defines
    public List<string> tasks = new List<string>
    {
        "Task 1: Use Red Button 5 times.",
        "Task 2: Use Run Button 3 times.",
        "Task 3: Close to Boss 1 time."
    };
    private bool isTaskClosed = false;

    // Network Variables
    private NetworkVariable<int> currentTaskIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> taskIndicator1 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> taskIndicator2 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> taskIndicator3 = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Messages
    private const string EmptyMessage = "";

    private void Start()
    {
        if (IsServer)
        {
            GenerateNewTask();
        }

        currentTaskIndex.OnValueChanged += UpdateTaskText;
        taskIndicator1.OnValueChanged += UpdateRedButtonCount;
        taskIndicator2.OnValueChanged += UpdateRunButtonCount;
        taskIndicator3.OnValueChanged += UpdateTask3Completion;

        UpdateTaskText(0, currentTaskIndex.Value);
    }

    private void Update()
    {
        if (gameViewTextBehaviour.timerDuration.Value <= 50f && !isTaskClosed)
        {
            TaskClosed();
        }

        if (IsServer && currentTaskIndex.Value == 2 && !isTaskClosed)
        {
            CheckProximityToBoss();
        }
    }

    public void RedButtonPressed()
    {
        if (IsClient)
        {
            RedButtonPressedServerRpc();
        }
    }

    public void RunButtonPressed()
    {
        if (IsClient)
        {
            RunButtonPressedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RedButtonPressedServerRpc()
    {
        if (isTaskClosed) return;

        if (currentTaskIndex.Value == 0)
        {
            taskIndicator1.Value++;
            if (taskIndicator1.Value >= 5)
            {
                CompleteTaskServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RunButtonPressedServerRpc()
    {
        if (isTaskClosed) return;

        if (currentTaskIndex.Value == 1)
        {
            taskIndicator2.Value++;
            if (taskIndicator2.Value >= 3)
            {
                CompleteTaskServerRpc();
            }
        }
    }

    private void CheckProximityToBoss()
    {
        if (isTaskClosed) return;

        List<ulong> bossClientIds = GetBossClientIds();
        if (bossClientIds.Count == 0) return;

        foreach (ulong bossClientId in bossClientIds)
        {
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(bossClientId))
            {
                Debug.LogWarning("Boss ID not found in connected clients.");
                continue;
            }

            var boss = NetworkManager.Singleton.ConnectedClients[bossClientId].PlayerObject;
            if (boss == null) continue;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.ClientId != bossClientId)
                {
                    var player = client.PlayerObject;
                    if (player != null)
                    {
                        float distance = Vector3.Distance(player.transform.position, boss.transform.position);
                        Debug.Log("Distance to boss: " + distance);

                        if (distance < 3.0f)
                        {
                            taskIndicator3.Value = true;
                            break;
                        }
                    }
                }
            }
        }
    }


    private List<ulong> GetBossClientIds()
    {
        if (gameManager.listBossAssigned.Count > 0)
        {
            foreach (var bossId in gameManager.listBossAssigned)
            {
                Debug.Log("BossId: " + bossId);
            }
            return new List<ulong>(gameManager.listBossAssigned);
        }
        return new List<ulong>();
    }



    private void UpdateTaskText(int oldValue, int newValue)
    {
        if (isTaskClosed) return;

        switch (newValue)
        {
            case 0:
                taskText.text = $"{tasks[newValue]} (0/5)";
                break;
            case 1:
                taskText.text = $"{tasks[newValue]} (0/3)";
                break;
            case 2:
                taskText.text = $"{tasks[newValue]} (Incomplete)";
                break;
        }
    }

    private void UpdateRedButtonCount(int oldValue, int newValue)
    {
        if (isTaskClosed || currentTaskIndex.Value != 0) return;

        taskText.text = $"{tasks[currentTaskIndex.Value]} ({newValue}/5)";
    }

    private void UpdateRunButtonCount(int oldValue, int newValue)
    {
        if (isTaskClosed || currentTaskIndex.Value != 1) return;

        taskText.text = $"{tasks[currentTaskIndex.Value]} ({newValue}/3)";
    }

    private void UpdateTask3Completion(bool oldValue, bool newValue)
    {
        if (isTaskClosed || currentTaskIndex.Value != 2 || !newValue) return;

        taskText.text = $"{tasks[currentTaskIndex.Value]} (Completed)";
        CompleteTaskServerRpc();
    }

    // Complete Task
    [ServerRpc(RequireOwnership = false)]
    public void CompleteTaskServerRpc()
    {
        if (isTaskClosed) return;

        ResetTaskIndicators();
        gameViewTextBehaviour.ReduceTimeDurationServerRpc(10f);
        GenerateNewTask();
    }

    private void ResetTaskIndicators()
    {
        taskIndicator1.Value = 0;
        taskIndicator2.Value = 0;
        taskIndicator3.Value = false;
    }

    private void GenerateNewTask()
    {
        if (isTaskClosed) return;

        int newTaskIndex = Random.Range(0, tasks.Count);
        currentTaskIndex.Value = newTaskIndex;
    }

    private void TaskClosed()
    {
        isTaskClosed = true;

        taskText.text = EmptyMessage;
        taskText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        currentTaskIndex.OnValueChanged -= UpdateTaskText;
        taskIndicator1.OnValueChanged -= UpdateRedButtonCount;
        taskIndicator2.OnValueChanged -= UpdateRunButtonCount;
        taskIndicator3.OnValueChanged -= UpdateTask3Completion;
    }
}