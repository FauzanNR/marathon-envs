using System;
using System.Collections.Generic;
using UnityEngine;



public class TaskSwitcher
{

    public int taskState { get; set; }
    public TargetTask previousTargetTask => tasks[taskState - 1];
    public TargetTask currentTargetTask => tasks[taskState];

    private List<TargetTask> tasks;

    public TaskSwitcher(List<TargetTask> tasks)
    {
        this.tasks = tasks;
        taskState = 0;
    }

    public bool ReportTask(float reward, float frequences)
    {
        // Debug.Log($"Report\n target reward {reward}\n target freq {frequences}");
        return reward >= tasks[taskState].targetReward && frequences >= tasks[taskState].targetFrequence;
    }

    public void DownTask()
    {
        if (tasks.Count > 0)
            taskState--;
    }
    public void UpdateTask()
    {
        if (taskState < tasks.Count - 1)
        {
            taskState++;
        }
    }
}

[Serializable]
public class TargetTask
{
    [field: SerializeField]
    public float targetReward { get; set; }
    [field: SerializeField]
    public int targetFrequence { get; set; }

}
