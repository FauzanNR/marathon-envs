using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lasp;
using Lomont;
using Unity.Collections;
using UnityEngine;

public class FrequencyStats : MonoBehaviour
{
    LomontFFT _lomontFFT;
    FftBuffer _fftBuffer;
    LogScaler _logScaler;
    public bool ScrollWindow = true;
    public float[] _input;
    public float[] _output;
    public List<NativeArray<float>> _inputs;
    public List<NativeArray<float>> _outputs;
    public List<FftBuffer> _rows;
    public List<LogScaler> _logScalerRows;
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
    void Start()
    {
    }
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
        if (_logScalerRows != null)
        {
            foreach (var l in _logScalerRows)
            {
                l.Dispose();
            }
        }
        _rows = new List<FftBuffer>();
        _logScalerRows = new List<LogScaler>();
        for (int i = 0; i < _dof; i++)
        {
            _rows.Add(new FftBuffer(size*2));
            _logScalerRows.Add(new LogScaler());
        }
        _input = Enumerable.Range(0, size)
            .Select(x=>0f)
            .ToArray();
        _output = Enumerable.Range(0, size)
            .Select(x=>0f)
            .ToArray();
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

        _lomontFFT = new LomontFFT();
        _fftBuffer = new FftBuffer(_winSize*2);
        _logScaler = new LogScaler();
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
        OrginalOnStep(timeDelta);

        int rowIdx = 0;
        for (int j = 0; j < _jointsToTrack.Length; j++)
        {
            var joint = _jointsToTrack[j];
            var reference = _articulationBodyJoints[j];
            Vector3 decomposedRotation = DecomposeQuanterium(joint.transform.localRotation);
            if (reference.twistLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = reference.xDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var deg = decomposedRotation.x;
                var pos = (deg - midpoint) / scale;
                NativeArray<float> nativeArray = new NativeArray<float>(new[] {pos}, Allocator.Temp);
                NativeSlice<float> slice = new NativeSlice<float>(nativeArray);
                _rows[rowIdx++].Push(slice);
            }
            if (reference.swingYLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = reference.yDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var deg = decomposedRotation.y;
                var pos = (deg - midpoint) / scale;
                NativeArray<float> nativeArray = new NativeArray<float>(new[] {pos}, Allocator.Temp);
                NativeSlice<float> slice = new NativeSlice<float>(nativeArray);
                _rows[rowIdx++].Push(slice);
            }
            if (reference.swingZLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = reference.zDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var deg = decomposedRotation.z;
                var pos = (deg - midpoint) / scale;
                NativeArray<float> nativeArray = new NativeArray<float>(new[] {pos}, Allocator.Temp);
                NativeSlice<float> slice = new NativeSlice<float>(nativeArray);
                _rows[rowIdx++].Push(slice);
            }            
        }
        foreach (var row in _rows)
        {
            row.Analyze(floor, head);
            
            if (_useLogScaler)
            {

            }
            var output = _useLogScaler ? _logScaler.Resample(row.Spectrum) : row.Spectrum;
        }
        if (_useLogScaler)
        {
            for (int i = 0; i < _rows.Count ; i++)
            {
                _logScalerRows[i].ResampleAndStore(_rows[i].Spectrum);
            }
        }
    }
    public void OrginalOnStep(float timeDelta)
    {
        // rotate window
        if (ScrollWindow)
        {
            Array.Copy(_input, 1, _input, 0, _input.Length-1);
            _dataIndex = _input.Length-1;
        }

        var value = 0f;
        var minIndex = 0;
        var maxIndex = _jointsToTrack.Length;
        if (_jointIndex != -1)
        {
            minIndex = _jointIndex;
            maxIndex = _jointIndex+1;
        }
        int idx = 0;
        for (int j = 0; j < _jointsToTrack.Length; j++)
        {
            var joint = _jointsToTrack[j];
            var reference = _articulationBodyJoints[j];
            Vector3 decomposedRotation = DecomposeQuanterium(joint.transform.localRotation);
            if (reference.twistLock == ArticulationDofLock.LimitedMotion)
            {
                if (idx>=minIndex && idx<maxIndex)
                {
                    var drive = reference.xDrive;
                    var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                    var midpoint = drive.lowerLimit + scale;
                    // var deg = joint.jointPosition[0] * Mathf.Rad2Deg;
                    var deg = decomposedRotation.x;
                    var pos = (deg - midpoint) / scale;
                    value += pos;
                }
                idx++;
            }
            if (reference.swingYLock == ArticulationDofLock.LimitedMotion)
            {
                if (idx>=minIndex && idx<maxIndex)
                {
                    var drive = reference.yDrive;
                    var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                    var midpoint = drive.lowerLimit + scale;
                    // var deg = reference.jointPosition[0] * Mathf.Rad2Deg;
                    var deg = decomposedRotation.y;
                    var pos = (deg - midpoint) / scale;
                    value += pos;
                }
                idx++;
            }
            if (reference.swingZLock == ArticulationDofLock.LimitedMotion)
            {
                if (idx>=minIndex && idx<maxIndex)
                {
                    var drive = reference.zDrive;
                    var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                    var midpoint = drive.lowerLimit + scale;
                    // var deg = joint.jointPosition[0] * Mathf.Rad2Deg;
                    var deg = decomposedRotation.z;
                    var pos = (deg - midpoint) / scale;
                    value += pos;
                }
                idx++;
            }            
        }
        _input[_dataIndex] = value;
        // _input.CopyTo(_output, 0);
        // _fft.RealFFT(_output, true);
        // _output = SpectrumAnalysis(_input, _winSize);
        _output = _useBurstFft ? ApplyBurstFFT(_input) : ApplyFFT(_input);
        for (int i = 0; i < _output.Length; i++)
        {
            if(Math.Abs(_output[i]) < _denoise)
            {
                _output[i] = 0;
            }
        }
        _dataIndex = (_dataIndex + 1) % (_winSize);
    }
    public float head = 0f;
    public float floor = -60f;
    float[] ApplyBurstFFT(float[] samples)
    {
        NativeArray<float> nativeArray = new NativeArray<float>(samples, Allocator.Temp);
        NativeSlice<float> nativeSlice = new NativeSlice<float>(nativeArray);
        _fftBuffer.Push(nativeSlice);
        _fftBuffer.Analyze(floor, head);
        var output = _useLogScaler ? _logScaler.Resample(_fftBuffer.Spectrum) : _fftBuffer.Spectrum;
        var scale = 1f;
        var result = output
                .Select(x=>x/scale)
                .ToArray();
        return result;
    }


    float[] ApplyFFT(float[] samples)
    {
        var complexSignal = new double[2*samples.Length];
        for (int j = 0; (j < samples.Length/2); j++)
        {
            complexSignal[2*j] = (double) samples[j];
            complexSignal[2*j + 1] = 0;
        }

        // _fft.FFT(complexSignal, true);
        _lomontFFT.RealFFT(complexSignal, true);
        var n = samples.Length;
        double lengthSqrt = Math.Sqrt(n);
        var bands = new float[n];
        var reals = new float[n];
        var imgs = new float[n];
        for (int j = 0; j < n; j++)
        {
            // double re = complexSignal[2*j] * lengthSqrt;
            // double img = complexSignal[2*j + 1] * lengthSqrt;            
            // // band[j] = (float) (Math.Sqrt(re*re + img*img) * lengthSqrt);
            // // double re = complexSignal[2*j];
            // reals[j] = (float) re;
            // imgs[j] = (float) img;
            // bands[j] = (float) (Math.Sqrt(re*re + img*img) * lengthSqrt);
            double re = complexSignal[2*j];
            double img = complexSignal[2*j + 1];
            bands[j] = (float)Math.Sqrt(re*re + img*img) * 1;
        }
        // return reals;
        return bands;         
    }

    float[] SpectrumAnalysis(float[] samples, int winSize)
    {
        int numberOfSamples = samples.Length;
        
        double[] windowArray = CreateWindow(winSize);

        var complexSignal = new double[2*winSize];

        // apply Hanning Window
        for (int j = 0; (j < winSize) && (samples.Length > j); j++)
        {
            complexSignal[2*j] = (double) (windowArray[j] * samples[j]);
            complexSignal[2*j + 1] = 0;
        }
        _lomontFFT.FFT(complexSignal, true);

        var band = new float[winSize/2];
        double lengthSqrt = Math.Sqrt(winSize);
        for (int j = 0; j < winSize/2; j++)
        {
            // double re = complexSignal[2*j] * lengthSqrt;
            // double img = complexSignal[2*j + 1] * lengthSqrt;
            
            // // do the Abs calculation and add with Math.Sqrt(audio_data.Length);
            // // i.e. the magnitude spectrum
            // band[j] = (float) (Math.Sqrt(re*re + img*img) * lengthSqrt);

        }
        return band;        
    }
    double[] CreateWindow(int winSize)
    {
        var array = new double[winSize];
        for (int i = 0; i < winSize; i++) {
            array[i] = 1;
        }

        for (int i = 0; i < winSize; ++i)
        {
            array[i] *= (0.5
                - 0.5 * Math.Cos((2 * Math.PI * i) / winSize)
                + 0.0 * Math.Cos((4 * Math.PI * i) / winSize)
                - 0.0 * Math.Cos((6 * Math.PI * i) / winSize));
        }
        return array;
    }
    Vector3 DecomposeQuanterium(Quaternion localRotation)
	{
		//the decomposition in swing-twist, typically works like this:
		Quaternion swing = new Quaternion(0.0f, localRotation.y, localRotation.z, localRotation.w);
		// Quaternion swing = new Quaternion(localRotation.x, localRotation.y, localRotation.z, localRotation.w);
		swing = swing.normalized;

		Quaternion twist = Quaternion.Inverse(swing) * localRotation;

		Vector3 decomposition = new Vector3(twist.eulerAngles.x, swing.eulerAngles.y, swing.eulerAngles.z);

		//we make sure we keep the values nearest to 0 (with a modulus)
		if (Mathf.Abs(decomposition.x - 360) < Mathf.Abs(decomposition.x))
			decomposition.x = (decomposition.x - 360);
		if (Mathf.Abs(decomposition.y - 360) < Mathf.Abs(decomposition.y))
			decomposition.y = (decomposition.y - 360);
		if (Mathf.Abs(decomposition.z - 360) < Mathf.Abs(decomposition.z))
			decomposition.z = (decomposition.z - 360);
		return decomposition;
	}
    void OnDisable()
    {
        _fftBuffer?.Dispose();
        _fftBuffer = null;

        _logScaler?.Dispose();
        _logScaler = null;

        foreach (var r in _rows)
        {
            r.Dispose();
        }
        foreach (var l in _logScalerRows)
        {
            l.Dispose();
        }
        _rows = new List<FftBuffer>();
        _logScalerRows = new List<LogScaler>();
    }
}
