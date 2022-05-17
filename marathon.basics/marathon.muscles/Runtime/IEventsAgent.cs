using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



struct ReferenceFrame
{
    Matrix4x4 space;
    Matrix4x4 inverseSpace;

    public Matrix4x4 Matrix { get => space; }
    public Matrix4x4 InverseMatrix { get => inverseSpace; }

    public ReferenceFrame(Vector3 heading, Vector3 centerOfMass)
    {
        // Instead of using the heading as the LookAt direction, we use world up, and set heading as the LookAt "up"
        // This gives us the horizontal projection of the heading for free
        space = Matrix4x4.LookAt(centerOfMass, centerOfMass + Vector3.up, heading);
        // In this representation z -> up, y -> forward, x -> left

        // So this means we have to roll the axes if we want z -> forward, y -> up, x -> right for consistency
        // Note that as long as the state representation from this source was consistent, this step would not actually be necessary
        // It just changes the order the dimension components are fed into the sensor.
        space = new Matrix4x4(-space.GetColumn(0), space.GetColumn(2), space.GetColumn(1), space.GetColumn(3));
        inverseSpace = space.inverse;
    }

    public interface IRememberPreviousActions
    {
        public float[] PreviousActions { get; }
    }
    public interface IEventsAgent
    {
        public event EventHandler<AgentEventArgs> onActionHandler;
        public event EventHandler<AgentEventArgs> onBeginHandler;
    }
}


public class AgentEventArgs : EventArgs
{
    public float[] actions;
    public float reward;

    public AgentEventArgs(float[] actions, float reward)
    {
        this.actions = actions;
        this.reward = reward;
    }

    new public static AgentEventArgs Empty => new AgentEventArgs(new float[0], 0f);

}



public interface IEventsAgent
{
    public event EventHandler<AgentEventArgs> onActionHandler;
    public event EventHandler<AgentEventArgs> onBeginHandler;
}