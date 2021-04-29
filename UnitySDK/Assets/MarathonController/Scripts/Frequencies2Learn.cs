using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using ManyWorlds;
using UnityEngine.Assertions;

public class Frequencies2Learn : MonoBehaviour
{
    public bool GraphInput;
    [Range(-1,49)]
    public int JointIndex;
    public string JointName;
    public bool ShowRagdoll = true;
    public bool ShowMocap = true;
    public bool UseLogScaler = true;
    public bool UseBurstFft = true;
    public float StepsPerSecond;

    public string[] JointNames;

    int _lastJointIndex;
    bool _lastUseBurstFft;
    float _lastTime;
    bool _lastUseLogScaler;
    FrequencyStats _mocapStats;
    FrequencyStats _RagdollStats;
    ArticulationBody[] _joints;

    public void OnAgentInitialize(GameObject ragdoll, GameObject mocap, ArticulationBody[] joints)
    {
        JointNames = joints
            .Select(x=>x.name)
            .Select(x=>x.Replace("articulation:", ""))
            .Select(x=>x.Replace("mixamorig:", ""))
            .ToArray();

        _joints = joints;
        _mocapStats = mocap.AddComponent<FrequencyStats>();
        _RagdollStats = ragdoll.AddComponent<FrequencyStats>();
        _mocapStats.OnAgentInitialize(_joints);
        _RagdollStats.OnAgentInitialize(_joints);
        
        _mocapStats.SetJointIndex(JointIndex);
        _RagdollStats.SetJointIndex(JointIndex);
        _lastJointIndex = JointIndex;
        SetJointName();

        _mocapStats.SetFftType(UseBurstFft);
        _RagdollStats.SetFftType(UseBurstFft);
        _lastUseBurstFft = UseBurstFft;

        _mocapStats.SetUseLogScaler(UseLogScaler);
        _RagdollStats.SetUseLogScaler(UseLogScaler);
        _lastUseLogScaler = UseLogScaler;
        
        _lastTime = Time.time;
    }

    public void OnStep(float timeDelta)
    {
        if (_lastJointIndex != JointIndex)
        {
            _mocapStats.SetJointIndex(JointIndex);
            _RagdollStats.SetJointIndex(JointIndex);
            _lastJointIndex = JointIndex;
            SetJointName();
        }
        if (_lastUseBurstFft != UseBurstFft)
        {
            _mocapStats.SetFftType(UseBurstFft);
            _RagdollStats.SetFftType(UseBurstFft);
            _lastUseBurstFft = UseBurstFft;
        }
        if (_lastUseLogScaler != UseLogScaler)
        {
            _mocapStats.SetUseLogScaler(UseLogScaler);
            _RagdollStats.SetUseLogScaler(UseLogScaler);
            _lastUseLogScaler = UseLogScaler;
        }
        _mocapStats.OnStep(timeDelta);
        _RagdollStats.OnStep(timeDelta);
        float realTimeDelta = Time.time-_lastTime;
        StepsPerSecond = 1f/realTimeDelta;
        _lastTime = Time.time;
    }
    void SetJointName()
    {
        if (JointIndex < 0)
        {
            JointName = "all joints";
            return;
        }
        int jointIdx = JointIndex;
        for (int i = 0; i < _joints.Length; i++)
        {
            var joint = _joints[i];
            var jointName = JointNames[i];
            if (joint.twistLock == ArticulationDofLock.LimitedMotion && jointIdx >= 0)
            {
                JointName = $"{jointName}.twistLock";
                jointIdx--;
            }
            if (joint.swingYLock == ArticulationDofLock.LimitedMotion && jointIdx >= 0)
            {
                JointName = $"{jointName}.swingYLock";
                jointIdx--;
            }
            if (joint.swingZLock == ArticulationDofLock.LimitedMotion && jointIdx >= 0)
            {
                JointName = $"{jointName}.swingZLock";
                jointIdx--;
            }
            if (jointIdx < 0)
                return;
        }
    }

// / Draws a basic oscilloscope type graph in a GUI.Window()
// / Michael Hutton May 2020
// / This is just a basic 'as is' do as you wish...
// / Let me know if you use it as I'd be interested if people find it useful.
// / I'm going to keep experimenting wih the GL calls...eg GL.LINES etc 
// / from: https://stackoverflow.com/questions/37137110/creating-graphs-in-unity

    Material mat;
    private Rect windowRect = new Rect(20, 20, 512, 256);

    // The list the drawing function uses...
    private float[] drawValuesA;
    private float[] drawValuesB;

    // List of Windows
    private bool showWindow0 = true;    

    private void OnGUI()
    {
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        }
        // Create a GUI.toggle to show graph window
        showWindow0 = GUI.Toggle(new Rect(10, 10, 100, 20), showWindow0, "Show Graph");

        if (showWindow0)
        {
            // Set out drawValue list equal to the values list 
            drawValuesA = GraphInput ? _mocapStats._input : _mocapStats._output;
            drawValuesB = GraphInput ? _RagdollStats._input : _RagdollStats._output;
            windowRect = GUI.Window(0, windowRect, DrawGraph, "");
        }

    }


    void DrawGraph(int windowID)
    {
        // Make Window Draggable
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));

        // Draw the graph in the repaint cycle
        if (Event.current.type == EventType.Repaint)
        {
            GL.PushMatrix();

            GL.Clear(true, false, Color.black);
            mat.SetPass(0);

            // Draw a black back ground Quad 
            GL.Begin(GL.QUADS);
            GL.Color(Color.black);
            GL.Vertex3(4, 4, 0);
            GL.Vertex3(windowRect.width - 4, 4, 0);
            GL.Vertex3(windowRect.width - 4, windowRect.height - 4, 0);
            GL.Vertex3(4, windowRect.height - 4, 0);
            GL.End();

            // Draw the lines of the graph
            GL.Begin(GL.LINES);
            GL.Color(Color.green);

            bool center = !UseLogScaler || GraphInput;
            float yHeight = (float) windowRect.height - 4;
            float yOffset = center ? yHeight / 2 : 0f;
            float yMultiply = center ? yHeight / 2: yHeight;
            float xScale = ((float) windowRect.width - 4) / (float) drawValuesA.Length;
            if (ShowMocap)
            {
                for (int i = 1; i < drawValuesA.Length; i++)
                {
                    float y1 = drawValuesA[i - 1] * yMultiply + yOffset;
                    float y2 = drawValuesA[i] * yMultiply + yOffset;
                    GL.Vertex3((i*xScale) + 2, yHeight - y2, 0);
                    GL.Vertex3(((i-1)*xScale) + 2, yHeight - y1, 0);
                }
            }
            // Draw 2nd
            GL.Color(Color.yellow);
            yHeight = (float) windowRect.height - 4;
            xScale = ((float) windowRect.width - 4) / (float) drawValuesB.Length;
            if (ShowRagdoll)
            {
                for (int i = 1; i < drawValuesB.Length; i++)
                {
                    float y1 = drawValuesB[i - 1] * yMultiply + yOffset;
                    float y2 = drawValuesB[i] * yMultiply + yOffset;
                    GL.Vertex3((i*xScale) + 2, yHeight - y2, 0);
                    GL.Vertex3(((i-1)*xScale) + 2, yHeight - y1, 0);
                }
            }
            GL.End();

            GL.PopMatrix();
        }
    }    
}