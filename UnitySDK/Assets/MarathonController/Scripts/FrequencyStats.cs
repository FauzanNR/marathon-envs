using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lasp;
using Unity.Collections;
using UnityEngine;

public class FrequencyStats : MonoBehaviour
{
    public bool ScrollWindow = true;
    public List<NativeArray<float>> _inputs;
    public List<NativeArray<float>> _outputs;
    public List<FftBuffer> _rows;
    public List<LogScaler> _logScalerRows;
    public List<BurstFft> _burstRows;

    public float _denoise = 0f;
    int _winSize = 64;
    int _dataIndex = 0;
    int _jointIndex = 0;
    bool _useBurstFft;
    bool _useLogScaler;

    int _dof;

    ArticulationBody[] _articulationBodyJoints;
    Rigidbody[] _rigidBodyJoints;
    GameObject[] _jointsToTrack;

    void InitData(int size)
    {
        var numJoints = _articulationBodyJoints.Length;
        if (_rows != null)
        {
            foreach (var r in _rows)
            {
                r.Dispose();
            }
        }
        if (_burstRows != null)
        {
            foreach (var r in _burstRows)
            {
                r.Dispose();
            }
        }
        if (_logScalerRows != null)
        {
            foreach (var l in _logScalerRows)
            {
                l.Dispose();
            }
        }
        _rows = new List<FftBuffer>();
        _logScalerRows = new List<LogScaler>();
        _burstRows = new List<BurstFft>();
        for (int i = 0; i < _dof; i++)
        {
            _rows.Add(new FftBuffer(size*2));
            _burstRows.Add(new BurstFft(size*2));
            _logScalerRows.Add(new LogScaler());
        }
        _dataIndex = 0;
    }

    public void OnAgentInitialize(ArticulationBody[] articulationBodyJoints, int dof)
    {
        _dof = dof;
        // strip names
        var jointNames = articulationBodyJoints
            .Select(x=>x.name)
            .Select(x=>x.Replace("articulation:", ""))
            .Select(x=>x.Replace("mixamorig:", ""))
            .ToArray();
        _articulationBodyJoints = articulationBodyJoints;

        // _fft.A = -1;
        // _fft.B = 1;
        InitData(_winSize);
        _rigidBodyJoints = GetComponentsInChildren<Rigidbody>();
        if (_rigidBodyJoints.Length > 0)
        {
            _jointsToTrack = jointNames
                .Select(x=>_rigidBodyJoints.First(y => y.name.EndsWith(x)))
                .Select(x=>x.gameObject)
                .ToArray();
        }
        else
        {
            _jointsToTrack = jointNames
                .Select(x=>articulationBodyJoints.First(y => y.name.EndsWith(x)))
                .Select(x=>x.gameObject)
                .ToArray();
        }
    }
    public void SetJointIndex(int jointIndex)
    {
        _jointIndex = jointIndex;
        InitData(_winSize);
    }
    public void SetFftType(bool useBurstFft)
    {
        _useBurstFft = useBurstFft;
    }
    public void SetUseLogScaler(bool useLogScaler)
    {
        _useLogScaler = useLogScaler;
    }

    public void OnStep(float timeDelta)
    {
        if (_articulationBodyJoints == null || _articulationBodyJoints.Length == 0)
            return;

        int rowIdx = 0;
        for (int j = 0; j < _jointsToTrack.Length; j++)
        {
            var joint = _jointsToTrack[j];
            var reference = _articulationBodyJoints[j];
            Vector3 decomposedRotation = Utils.GetSwingTwist(joint.transform.localRotation);
            if (reference.twistLock == ArticulationDofLock.LimitedMotion)
            {
                // var drive = reference.xDrive;
                // var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                // var midpoint = drive.lowerLimit + scale;
                var deg = decomposedRotation.x;
                // var pos = (deg - midpoint) / scale;
                var pos = deg * Mathf.Deg2Rad; // all pos within +/- Pi
                NativeArray<float> nativeArray = new NativeArray<float>(new[] {pos}, Allocator.Temp);
                NativeSlice<float> slice = new NativeSlice<float>(nativeArray);
                _rows[rowIdx++].Push(slice);
            }
            if (reference.swingYLock == ArticulationDofLock.LimitedMotion)
            {
                // var drive = reference.yDrive;
                // var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                // var midpoint = drive.lowerLimit + scale;
                var deg = decomposedRotation.y;
                // var pos = (deg - midpoint) / scale;
                var pos = deg * Mathf.Deg2Rad; // all pos within +/- Pi
                NativeArray<float> nativeArray = new NativeArray<float>(new[] {pos}, Allocator.Temp);
                NativeSlice<float> slice = new NativeSlice<float>(nativeArray);
                _rows[rowIdx++].Push(slice);
            }
            if (reference.swingZLock == ArticulationDofLock.LimitedMotion)
            {
                // var drive = reference.zDrive;
                // var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                // var midpoint = drive.lowerLimit + scale;
                var deg = decomposedRotation.z;
                // var pos = (deg - midpoint) / scale;
                var pos = deg * Mathf.Deg2Rad; // all pos within +/- Pi
                NativeArray<float> nativeArray = new NativeArray<float>(new[] {pos}, Allocator.Temp);
                NativeSlice<float> slice = new NativeSlice<float>(nativeArray);
                _rows[rowIdx++].Push(slice);
            }            
        }
        foreach (var row in _rows)
        {
            row.Analyze(floor, head);
        }
        if (_useBurstFft)
        {
            for (int i = 0; i < _burstRows.Count; i++)
            {
                _burstRows[i].Transform(_rows[i].Input);
            }
        }
        if (_useLogScaler)
        {
            for (int i = 0; i < _rows.Count ; i++)
            {
                if (_useBurstFft)
                    _logScalerRows[i].ResampleAndStore(_burstRows[i].Spectrum);
                else
                    _logScalerRows[i].ResampleAndStore(_rows[i].Spectrum);
            }
        }
    }

    public float head = 0f;
    public float floor = -60f;

    void OnDisable()
    {
        foreach (var r in _rows)
        {
            r.Dispose();
        }
        if (_burstRows != null)
        {
            foreach (var r in _burstRows)
            {
                r.Dispose();
            }
        }
        foreach (var l in _logScalerRows)
        {
            l.Dispose();
        }
        _rows = new List<FftBuffer>();
        _logScalerRows = new List<LogScaler>();
        _burstRows = new List<BurstFft>();
    }
}
