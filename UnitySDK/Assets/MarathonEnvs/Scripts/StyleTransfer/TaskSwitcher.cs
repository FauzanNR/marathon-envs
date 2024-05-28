using System.Collections.Generic;
using UnityEngine;



public class TaskSwitcher
{

    public int taskState { get; set; }

    private List<TargetTask> tasks;

    public TaskSwitcher(List<TargetTask> tasks)
    {
        this.tasks = tasks;
        taskState = 0;
    }

    public void ReportTask(float reward, float frequences)
    {
        // Debug.Log($"Report\n target reward {reward}\n target freq {frequences}");
        if (reward >= tasks[taskState].targetReward && frequences >= tasks[taskState].targetFrequence)
        {
            UpdateTask();
        }
    }


    public void UpdateTask()
    {
        if (taskState < tasks.Count - 1)
            taskState++;
    }

    public class TargetTask
    {
        public float targetReward { get; set; }
        public int targetFrequence { get; set; }

    }
}
