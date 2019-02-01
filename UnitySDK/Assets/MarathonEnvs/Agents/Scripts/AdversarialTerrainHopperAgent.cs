using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
public class AdversarialTerrainHopperAgent : MarathonAgent {

    AdversarialTerrainAgent _adversarialTerrainAgent;
    int _lastXPosInMeters;
    float _pain;
    bool _modeRecover;
    Vector3 _centerOfMass;

    public override void AgentReset()
    {
        base.AgentReset();

        BodyParts["pelvis"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="torso");
        BodyParts["foot"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="foot");

        SetCenterOfMass();

        if (_adversarialTerrainAgent == null)
            _adversarialTerrainAgent = GetComponent<AdversarialTerrainAgent>();
        _lastXPosInMeters = (int) BodyParts["foot"].transform.position.x;
        _adversarialTerrainAgent.Terminate(GetCumulativeReward());

        // set to true this to show monitor while training
        Monitor.SetActive(true);

        StepRewardFunction = StepRewardHopper101;
        TerminateFunction = LocalTerminate;
        ObservationsFunction = ObservationsDefault;
        // OnTerminateRewardValue = -100f;
        _pain = 0f;
        _modeRecover = false;

        base.SetupBodyParts();
        SetCenterOfMass();
    }

    bool LocalTerminate()
    {
        int newXPosInMeters = (int) BodyParts["foot"].transform.position.x;
        if (newXPosInMeters > _lastXPosInMeters) {
            _adversarialTerrainAgent.OnNextMeter();
            _lastXPosInMeters = newXPosInMeters;
        }

        SetCenterOfMass();
        var xpos = _centerOfMass.x;
        var terminate = false;
        if (NonFootHitTerrain)
            terminate = true;
        if (terminate)
            _adversarialTerrainAgent.Terminate(GetCumulativeReward());

        return terminate;
    }
    public override void OnTerrainCollision(GameObject other, GameObject terrain) {
        if (terrain.GetComponent<Terrain>() == null)
            return;

        switch (other.name.ToLowerInvariant().Trim())
        {
            case "thigh": // dm_hopper
            case "pelvis": // dm_hopper
                _pain += 5f;
                NonFootHitTerrain = true;
                _modeRecover = true;
                break;
            case "foot": // dm_hopper
            case "calf": // dm_hopper
                FootHitTerrain = true;
                break;
            default:
                _pain += 5f;
                NonFootHitTerrain = true;
                _modeRecover = true;
                break;
        }
    }


    public override void AgentOnDone()
    {
    }    
    void ObservationsDefault()
    {
        if (ShowMonitor) {
        }
        var pelvis = BodyParts["pelvis"];
        AddVectorObs(pelvis.velocity);
        AddVectorObs(pelvis.transform.forward); // gyroscope 
        AddVectorObs(pelvis.transform.up);
        
        AddVectorObs(SensorIsInTouch);
        JointRotations.ForEach(x=>AddVectorObs(x));
        AddVectorObs(JointVelocity);
        var foot = BodyParts["foot"];
        AddVectorObs(foot.transform.position.y);

        var xpos = pelvis.transform.position.x;
        xpos -= 2f;
        float fraction = (xpos - (Mathf.Floor(xpos*5)/5)) * 5;
        float ypos = pelvis.transform.position.y;
        List<Ray> rays = Enumerable.Range(0, 5*5).Select(x => new Ray(new Vector3(xpos+(x*.2f), AdversarialTerrainAgent._maxHeight, 0f), Vector3.down)).ToList();
        List<float> distances = rays.Select
            ( x=>
                ypos - (AdversarialTerrainAgent._maxHeight - 
                Physics.RaycastAll(x)
                .OrderBy(y=>y.distance)
                .FirstOrDefault()
                .distance)
            ).ToList();
        if (Application.isEditor && ShowMonitor)
        {
            var view = distances.Skip(10).Take(20).Select(x=>x).ToList();
            Monitor.Log("distances", view.ToArray());
            var time = Time.deltaTime;
            time *= agentParameters.numberOfActionsBetweenDecisions;
            for (int i = 0; i < rays.Count; i++)
            {
                var distance = distances[i];
                var origin = new Vector3(rays[i].origin.x, ypos,0f);
                var direction = distance > 0 ? Vector3.down : Vector3.up;
                var color = distance > 0 ? Color.yellow : Color.red;
                Debug.DrawRay(origin, direction*Mathf.Abs(distance), color, time, false);
            }
        }        
        AddVectorObs(distances);
        AddVectorObs(fraction);
    }


    void SetCenterOfMass()
    {
        _centerOfMass = Vector3.zero;
        float c = 0f;
        var bodyParts = this.gameObject.GetComponentsInChildren<Rigidbody>();
 
        foreach (var part in bodyParts)
        {
            _centerOfMass += part.worldCenterOfMass * part.mass;
            c += part.mass;
        }
        _centerOfMass /= c;
    }

    float StepRewardHopper101()
    {
        // float heightPenality = GetHeightPenality(0.5f);
        float uprightBonus = GetForwardBonus("pelvis");
        float velocity = GetVelocity("pelvis");
        float effort = GetEffort();
        // var effortPenality = 1e-2f * (float)effort;
        var effortPenality = 3e-1f * (float)effort;
        var jointsAtLimitPenality = GetJointsAtLimitPenality() * 4;

        //var uprightScaler = Mathf.Clamp(velocity,0,1);
        //uprightBonus *= 0f;//uprightScaler;
        if (_pain > 0f){
            uprightBonus = 0f;
        }
        if (_modeRecover) {
            uprightBonus = 0f;
            effortPenality = 0f;
        }

        var reward = velocity
            // +uprightBonus
            // // -heightPenality
            // -effortPenality
            // -jointsAtLimitPenality
            // -_pain
            ;
        if (ShowMonitor) {
            // var hist = new []{reward,velocity,uprightBonus,-heightPenality,-effortPenality}.ToList();
            var hist = new []{reward, velocity, uprightBonus, -effortPenality, 
            -jointsAtLimitPenality,
            -_pain
            }.ToList();
            Monitor.Log("rewardHist", hist.ToArray().ToString());
        }
        _pain = 0f;
        return reward;
    }
}