using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using ManyWorlds;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.MLAgents.Actuators;

public class Frequencies2Learn : MonoBehaviour
{
    public bool RenderAsBitmap;
    public bool GraphInput;
    public bool UseLogScaler = true;
    [Range(-1,49)]
    public int JointIndex = -1;
    public string JointName;
    public bool ShowMocap = true;
    public bool ShowRagdoll = true;
    public List<NativeArray<float>> Diffences;
    public List<float> ErrorByRow;
    public float Error;
    public float ErrorSquared;
    public float FrequencyReward;

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
        var dof = 50; // HACK - do this properly
        _mocapStats.OnAgentInitialize(_joints, dof);
        _RagdollStats.OnAgentInitialize(_joints, dof);
        
        _lastJointIndex = JointIndex;
        SetJointName();

        _mocapStats.SetUseLogScaler(UseLogScaler);
        _RagdollStats.SetUseLogScaler(UseLogScaler);
        _lastUseLogScaler = UseLogScaler;
        
        _lastTime = Time.time;
    }


    public void OnStep(float timeDelta)
    {
        if (_lastJointIndex != JointIndex)
        {
            _lastJointIndex = JointIndex;
            SetJointName();
        }
        if (_lastUseLogScaler != UseLogScaler)
        {
            _mocapStats.SetUseLogScaler(UseLogScaler);
            _RagdollStats.SetUseLogScaler(UseLogScaler);
            _lastUseLogScaler = UseLogScaler;
        }
        _mocapStats.OnStep(timeDelta);
        _RagdollStats.OnStep(timeDelta);
        UpdateDifference();

        float realTimeDelta = Time.time-_lastTime;
        StepsPerSecond = 1f/realTimeDelta;
        _lastTime = Time.time;
    }

    void UpdateDifference()
    {
        int numRows = _mocapStats._logScalerRows.Count;
        // int rowLength = _mocapStats._logScalerRows[0].Buffer.Length;
        int rowLength = _mocapStats._rows[0].Spectrum.Length;
        bool recreate = Diffences==null;
        if (Diffences!=null)
        {
            recreate |= numRows != Diffences.Count;
            recreate |= rowLength != Diffences[0].Length;
            if (recreate)
            {
                foreach (var row in Diffences)
                {
                    row.Dispose();
                }
            }
        }
        if (recreate)
        {
            Diffences = new List<NativeArray<float>>();
            ErrorByRow = new List<float>();
            for (int i = 0; i < numRows; i++)
            {
                Diffences.Add(new NativeArray<float>(rowLength, Allocator.Persistent));
                ErrorByRow.Add(0f);
            }
        }
        // 
        Error = 0f;
        for (int r = 0; r < numRows; r++)
        {
            var diffRow = Diffences[r];
            var rowA = _mocapStats._rows[r].Spectrum;
            var rowB = _RagdollStats._rows[r].Spectrum;
            if (UseLogScaler)
            {
                rowA = _mocapStats._logScalerRows[r].Buffer;
                rowB = _RagdollStats._logScalerRows[r].Buffer;
            }
            float rowError = 0f;
            for (int i = 0; i < rowLength; i++)
            {
                var n = Mathf.Abs(rowA[i] - rowB[i]);
                rowError += n;
                diffRow[i] = n;
            }
            rowError /= (float)diffRow.Length;
            ErrorByRow[r] = rowError;
            Error += rowError;
        }
        Error /= (float)ErrorByRow.Count;
        ErrorSquared = Mathf.Pow(Error, 2);
        FrequencyReward = 1f - ErrorSquared;
    }
    public float GetReward()
    {
        return FrequencyReward;
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
            windowRect = GUI.Window(0, windowRect, DrawGraph, "");
        }

    }


    void DrawGraph(int windowID)
    {
        // Make Window Draggable
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        GL.PushMatrix();

        // Draw the graph in the repaint cycle
        if (Event.current.type == EventType.Repaint)
        {
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

            Draw2();
        }
        GL.End();
        GL.PopMatrix();        
    }
    void Draw2()
    {
        // Draw the lines of the graph
        GL.Begin(GL.LINES);

        float yHeight = (float) windowRect.height - 6;
        float yPos = 2;
        if (ShowMocap)
        {
            yPos = Plot(_mocapStats, yPos);
        }
        else
            yPos = PlotDifferences(yPos);
        if (ShowRagdoll)
        {
            yPos = Plot(_RagdollStats, yPos);
        }
        else
            yPos = PlotDifferences(yPos);
    }

    float Plot(FrequencyStats stats, float yPos)
    {
        if (RenderAsBitmap)
        {
            yPos = PlotStatsAsBitmap(stats, yPos);
        }
        else
        {
            yPos = PlotStatsAsGraph(stats, yPos);
        }
        return yPos;
    }
    float PlotStatsAsGraph(FrequencyStats stats, float yPos)
    {
        var rows = StatsToRows(stats);
        return PlotRowsAsGraph(rows, yPos);
    }
    float PlotDifferences(float yPos)
    {
        return PlotRowsAsGraph(Diffences, yPos);
    }

    float PlotRowsAsGraph(List<NativeArray<float>> rows, float yPos)
    {
        bool center = !UseLogScaler || GraphInput;
        float yHeight = (float) (windowRect.height - 4) / 2;
        float yOffset = center ? yHeight / 2 : 0f;
        // yOffset += yPos;
        float yMultiply = center ? yHeight / 2: yHeight;
        float colorIdx = 0f;
        yPos += yHeight;
        foreach (var row in rows)
        {
            bool skip = false;
            if (JointIndex >= 0)
            {
                skip = rows.IndexOf(row) != JointIndex;
            }
            if (!skip)
            {
                float xScale = ((float) windowRect.width - 6) / (float) row.Length;
                GL.Color(FloatToColor(colorIdx));
                for (int i = 1; i < row.Length; i++)
                {
                    float y1 = row[i - 1] * yMultiply + yOffset;
                    float y2 = row[i] * yMultiply + yOffset;
                    GL.Vertex3((i*xScale) + 2, yPos - y2, 0);
                    GL.Vertex3(((i-1)*xScale) + 2, yPos - y1, 0);
                }
            }
            colorIdx += 1f / (float)rows.Count;
        }
        return yPos;
    }
   
    Color FloatToColor(float f)
    {
        Color col;
        if (f> 0f)
        {
            col = new Color(1f, f, 0f);
            return col;
        }
        else
        {
            f = 1f+-f;
            col = new Color(f, 0f, 0f);
        }
        return col;
    }
    float PlotStatsAsBitmap(FrequencyStats stats, float yPos)
    {
        var rows = StatsToRows(stats);
        foreach (var row in rows)
        {
            bool skip = false;
            if (JointIndex >= 0)
            {
                skip = rows.IndexOf(row) != JointIndex;
            }
            if (!skip)
            {
                float xScale = ((float) windowRect.width - 6) / (float) row.Length;
                for (int i = 0; i < row.Length; i++)
                {
                    float y1 = yPos;
                    float y2 = yPos;
                    var value = row[i];
                    GL.Color(FloatToColor(value));
                    GL.Vertex3(((i+1)*xScale) + 2, y2, 0);
                    GL.Vertex3(((i)*xScale) + 2, y1, 0);
                }   
            }
            yPos+=1;
        }
        yPos+=3;
        return yPos;
    }
    List<NativeArray<float>> StatsToRows(FrequencyStats stats)
    {
        List<NativeArray<float>> rows = null;
        if (this.GraphInput)
            rows = stats._rows
                .Select(x=>x.Input)
                .ToList();
        else if (this.UseLogScaler)
            rows = stats._logScalerRows
                .Select(x=>x.Buffer)
                .ToList();
        else
            rows = stats._rows
                .Select(x=>x.Spectrum)
                .ToList();
        return rows;
    }
    void OnDisable()
    {
        if (Diffences != null)
        {
            foreach (var row in Diffences)
            {
                row.Dispose();
            }
            Diffences = null;
        }
    }    
}