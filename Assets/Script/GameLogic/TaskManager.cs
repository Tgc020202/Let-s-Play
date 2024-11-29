using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TaskManager : NetworkBehaviour
{
    public List<string> tasks = new List<string> { "Task 1: Collect Items", "Task 2: Solve Puzzle", "Task 3: Fix Machine" };
    public Text taskText;
    private NetworkVariable<int> currentTaskIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private GameViewTextBehaviour gameViewTextBehaviour;

    void Start()
    {
        gameViewTextBehaviour = FindObjectOfType<GameViewTextBehaviour>();

        if (IsServer)
        {
            // Initialize the first task
            GenerateNewTaskServerRpc();
        }

        // Update the task text when the task changes
        currentTaskIndex.OnValueChanged += UpdateTaskText;
        UpdateTaskText(0, currentTaskIndex.Value);
    }

    /// <summary>
    /// Called when a staff completes a task
    /// </summary>
    public void CompleteTask()
    {
        if (IsServer)
        {
            // Reduce game duration by 10 seconds
            gameViewTextBehaviour.ReduceTimeDurationServerRpc(10f);

            // Generate a new task
            GenerateNewTaskServerRpc();
        }
    }

    /// <summary>
    /// Randomly selects a new task
    /// </summary>
    [ServerRpc]
    private void GenerateNewTaskServerRpc()
    {
        currentTaskIndex.Value = Random.Range(0, tasks.Count);
    }

    /// <summary>
    /// Updates the task text UI for all staff
    /// </summary>
    private void UpdateTaskText(int oldValue, int newValue)
    {
        taskText.text = tasks[newValue];
    }

    private void OnDestroy()
    {
        currentTaskIndex.OnValueChanged -= UpdateTaskText;
    }
}
