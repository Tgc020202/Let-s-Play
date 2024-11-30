using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TaskManager : NetworkBehaviour
{
    public List<string> tasks = new List<string>
    {
        "Task 1: Use Red Button 5 times.",
        "Task 2: Use Run Button 3 times.",
        "Task 3: Close to Boss 1 time."
    };

    public Text taskText;

    private NetworkVariable<int> currentTaskIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> taskIndicator1 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> taskIndicator2 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> taskIndicator3 = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public GameViewTextBehaviour gameViewTextBehaviour;
    public GameManager gameManager;

    private void Start()
    {
        if (IsServer)
        {
            GenerateNewTask(); // Initialize the first task
        }

        currentTaskIndex.OnValueChanged += UpdateTaskText;
        taskIndicator1.OnValueChanged += UpdateRedButtonCount;
        taskIndicator2.OnValueChanged += UpdateRunButtonCount;
        taskIndicator3.OnValueChanged += UpdateTask3Completion;

        UpdateTaskText(0, currentTaskIndex.Value);
    }

    private void Update()
    {
        if (IsServer && currentTaskIndex.Value == 2)
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
        ulong bossClientId = GetBossClientId();
        if (bossClientId == 0) return;

        var boss = NetworkManager.Singleton.ConnectedClients[bossClientId].PlayerObject;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != bossClientId)
            {
                var player = client.PlayerObject;
                if (player != null)
                {
                    float distance = Vector3.Distance(player.transform.position, boss.transform.position);
                    if (distance < 2.0f)
                    {
                        taskIndicator3.Value = true;
                        break;
                    }
                }
            }
        }
    }

    private ulong GetBossClientId()
    {
        foreach (var client in gameManager.rolesAssigned)
        {
            if (client.Value)
            {
                return client.Key;
            }
        }
        return 0;
    }

    private void UpdateTaskText(int oldValue, int newValue)
    {
        taskText.text = tasks[newValue];
        Debug.Log($"Task Updated: {tasks[newValue]}");
        if (newValue == 0)
        {
            taskText.text = $"{tasks[newValue]} (0/5)";
        }
        else if (newValue == 1)
        {
            taskText.text = $"{tasks[newValue]} (0/3)";
        }
        else if (newValue == 2)
        {
            taskText.text = $"{tasks[newValue]} (Incomplete)";
        }
    }

    private void UpdateRedButtonCount(int oldValue, int newValue)
    {
        if (currentTaskIndex.Value == 0)
        {
            taskText.text = $"{tasks[currentTaskIndex.Value]} ({newValue}/5)";
        }
    }

    private void UpdateRunButtonCount(int oldValue, int newValue)
    {
        if (currentTaskIndex.Value == 1)
        {
            taskText.text = $"{tasks[currentTaskIndex.Value]} ({newValue}/3)";
        }
    }

    private void UpdateTask3Completion(bool oldValue, bool newValue)
    {
        if (currentTaskIndex.Value == 2 && newValue)
        {
            taskText.text = $"{tasks[currentTaskIndex.Value]} (Completed)";
            CompleteTaskServerRpc();
        }
    }

    // Complete Task
    [ServerRpc(RequireOwnership = false)]
    public void CompleteTaskServerRpc()
    {
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
        int newTaskIndex = Random.Range(0, tasks.Count);
        currentTaskIndex.Value = newTaskIndex;
    }


    private void OnDestroy()
    {
        currentTaskIndex.OnValueChanged -= UpdateTaskText;
        taskIndicator1.OnValueChanged -= UpdateRedButtonCount;
        taskIndicator2.OnValueChanged -= UpdateRunButtonCount;
        taskIndicator3.OnValueChanged -= UpdateTask3Completion;
    }
}
