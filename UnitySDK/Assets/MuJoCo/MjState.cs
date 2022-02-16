using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Mujoco
{
    public static class MjState
    {
        public static unsafe (IEnumerable<double[]>, IEnumerable<double[]>) GetMjKinematics(this MjScene mjScene, MjBody rootBody)
        {
            MujocoLib.mjModel_* Model = mjScene.Model;
            MujocoLib.mjData_* Data = mjScene.Data;
            var joints = rootBody.GetComponentsInChildren<MjBaseJoint>();
            var positions = new List<double[]>();
            var velocities = new List<double[]>();
            foreach (var joint in joints)
            {
                switch (Model->jnt_type[joint.MujocoId])
                {
                    default:
                    case (int)MujocoLib.mjtJoint.mjJNT_HINGE:
                    case (int)MujocoLib.mjtJoint.mjJNT_SLIDE:
                        positions.Add(new double[] { Data->qpos[joint.QposAddress] });
                        velocities.Add(new double[] { Data->qvel[joint.DofAddress] });
                        break;
                    case (int)MujocoLib.mjtJoint.mjJNT_BALL:

                        positions.Add(new double[] { Data->qpos[joint.QposAddress],
                                                     Data->qpos[joint.QposAddress+1],
                                                     Data->qpos[joint.QposAddress+2],
                                                     Data->qpos[joint.QposAddress+3]});

                        velocities.Add(new double[] { Data->qvel[joint.DofAddress],
                                                      Data->qvel[joint.DofAddress+1],
                                                      Data->qvel[joint.DofAddress+2]});
                        break;
                    case (int)MujocoLib.mjtJoint.mjJNT_FREE:
                        positions.Add(new double[] {
                                                        Data->qpos[joint.QposAddress],
                                                        Data->qpos[joint.QposAddress+1],
                                                        Data->qpos[joint.QposAddress+2],
                                                        Data->qpos[joint.QposAddress+3],
                                                        Data->qpos[joint.QposAddress+4],
                                                        Data->qpos[joint.QposAddress+5],
                                                        Data->qpos[joint.QposAddress+6]});
                        velocities.Add( new double[] {
                                                        Data->qvel[joint.DofAddress],
                                                        Data->qvel[joint.DofAddress+1],
                                                        Data->qvel[joint.DofAddress+2],
                                                        Data->qvel[joint.DofAddress+3],
                                                        Data->qvel[joint.DofAddress+4],
                                                        Data->qvel[joint.DofAddress+5]});
                        break;
                    }
                }
            return (positions, velocities);
        }

        public static unsafe void SetMjKinematics(this MjScene mjScene, MjBody rootBody, IEnumerable<double[]> positions, IEnumerable<double[]> velocities)
        {
            MujocoLib.mjModel_* Model = mjScene.Model;
            MujocoLib.mjData_* Data = mjScene.Data;
            var joints = rootBody.GetComponentsInChildren<MjBaseJoint>();
            foreach ((var joint, (var position, var velocity)) in joints.Zip( positions.Zip(velocities, Tuple.Create), Tuple.Create))
            {
                switch (Model->jnt_type[joint.MujocoId])
                {
                    default:
                    case (int)MujocoLib.mjtJoint.mjJNT_HINGE:
                    case (int)MujocoLib.mjtJoint.mjJNT_SLIDE:
                        Data->qpos[joint.QposAddress] = position[0];
                        Data->qvel[joint.DofAddress] = velocity[0];
                        break;
                    case (int)MujocoLib.mjtJoint.mjJNT_BALL:
                        Data->qpos[joint.QposAddress] = position[0];
                        Data->qpos[joint.QposAddress + 1] = position[1];
                        Data->qpos[joint.QposAddress + 2] = position[2];
                        Data->qpos[joint.QposAddress + 3] = position[3];
                        Data->qvel[joint.DofAddress] = velocity[0];
                        Data->qvel[joint.DofAddress + 1] = velocity[1];
                        Data->qvel[joint.DofAddress + 2] = velocity[2];
                        break;
                    case (int)MujocoLib.mjtJoint.mjJNT_FREE:
                        Data->qpos[joint.QposAddress] = position[0];
                        Data->qpos[joint.QposAddress + 1] = position[1];
                        Data->qpos[joint.QposAddress + 2] = position[2];
                        Data->qpos[joint.QposAddress + 3] = position[3];
                        Data->qpos[joint.QposAddress + 4] = position[4];
                        Data->qpos[joint.QposAddress + 5] = position[5];
                        Data->qpos[joint.QposAddress + 6] = position[6];
                        Data->qvel[joint.DofAddress] = velocity[0];
                        Data->qvel[joint.DofAddress + 1] = velocity[1];
                        Data->qvel[joint.DofAddress + 2] = velocity[2];
                        Data->qvel[joint.DofAddress + 3] = velocity[3];
                        Data->qvel[joint.DofAddress + 4] = velocity[4];
                        Data->qvel[joint.DofAddress + 5] = velocity[5];
                        break;
                }

            }
            // update mj transforms:
            MujocoLib.mj_kinematics(Model, Data);
            mjScene.SyncUnityToMjState();
        }

        public static unsafe void TeleportMjRoot(this MjScene mjScene, MjFreeJoint root, Vector3 unityPos, Quaternion unityRot)
        {
            MujocoLib.mjData_* Data = mjScene.Data;
            MujocoLib.mjModel_* Model = mjScene.Model;

            Quaternion oldUnityRotation = MjEngineTool.UnityQuaternion(Data->xquat, root.MujocoId);
            var startOffset = root.MujocoId * 7;
            Quaternion oldMjQuat = new Quaternion(w: (float)Data->qpos[startOffset + 3],
                                                  x: (float)Data->qpos[startOffset + 4],
                                                  y: (float)Data->qpos[startOffset + 5],
                                                  z: (float)Data->qpos[startOffset + 6]);
            Quaternion manualUnityQuat = MjEngineTool.UnityQuaternion(oldMjQuat);

            MjEngineTool.SetMjTransform(Data->qpos, unityPos, unityRot, root.MujocoId);

            



            /*Quaternion rotationOffset = unityRot * Quaternion.Inverse(manualUnityQuat);
            Debug.Log($"rotationOffset: {rotationOffset}");*/

            Quaternion rotationOffset = unityRot * Quaternion.Inverse(manualUnityQuat);
            Vector3 fromUnityLinVel = MjEngineTool.UnityVector3(Data->qvel, root.MujocoId * 2);

            startOffset = root.MujocoId * 6;
            
            Vector3 toMjLinVel = MjEngineTool.MjVector3(rotationOffset * fromUnityLinVel);
            Data->qvel[startOffset] = toMjLinVel[0];
            Data->qvel[startOffset+1] = toMjLinVel[1];
            Data->qvel[startOffset + 2] = toMjLinVel[2];


            Vector3 fromUnityAngVel = MjEngineTool.UnityVector3(Data->qvel, (root.MujocoId *2 )+1);
            Vector3 toMjAngVel = MjEngineTool.MjVector3(rotationOffset * fromUnityAngVel);

            Data->qvel[startOffset + 3] = toMjAngVel[0];
            Data->qvel[startOffset + 4] = toMjAngVel[1];
            Data->qvel[startOffset + 5] = toMjAngVel[2];

            MujocoLib.mj_kinematics(Model, Data);
            mjScene.SyncUnityToMjState();

        }

        public static unsafe Vector3 GlobalVelocity(this MjBody body)
        {
            var mjScene = MjScene.Instance;
            MujocoLib.mjModel_* Model = mjScene.Model;
            MujocoLib.mjData_* Data = mjScene.Data;
            Vector3 bodyVel = Vector3.one;
            double[] mjBodyVel = new double[6];
            fixed (double* res = mjBodyVel)
            {
                MujocoLib.mj_objectVelocity(
                    mjScene.Model, mjScene.Data, (int)MujocoLib.mjtObj.mjOBJ_BODY, body.MujocoId, res, 0);
                // linear velocity is in the last 3 entries
                bodyVel = MjEngineTool.UnityVector3(res, 1);
            }
            return bodyVel;
        }

        public static unsafe Vector3 GlobalAngularVelocity(this MjBody body)
        {
            var mjScene = MjScene.Instance;
            MujocoLib.mjModel_* Model = mjScene.Model;
            MujocoLib.mjData_* Data = mjScene.Data;
            Vector3 bodyAngVel = Vector3.one;
            double[] mjBodyAngVel = new double[6];
            fixed (double* res = mjBodyAngVel)
            {
                MujocoLib.mj_objectVelocity(
                    mjScene.Model, mjScene.Data, (int)MujocoLib.mjtObj.mjOBJ_BODY, body.MujocoId, res, 0);
                bodyAngVel = MjEngineTool.UnityVector3(res, 0);
            }
            return bodyAngVel;
        }

        public static unsafe Vector3 LocalVelocity(this MjBody body)
        {
            var mjScene = MjScene.Instance;
            MujocoLib.mjModel_* Model = mjScene.Model;
            MujocoLib.mjData_* Data = mjScene.Data;
            Vector3 bodyAngVel = Vector3.one;
            double[] mjBodyAngVel = new double[6];
            fixed (double* res = mjBodyAngVel)
            {
                MujocoLib.mj_objectVelocity(
                    mjScene.Model, mjScene.Data, (int)MujocoLib.mjtObj.mjOBJ_BODY, body.MujocoId, res, 1);
                bodyAngVel = MjEngineTool.UnityVector3(res, 0);
            }
            return bodyAngVel;
        }

        public static unsafe Vector3 LocalAngularVelocity(this MjBody body)
        {
            var mjScene = MjScene.Instance;
            MujocoLib.mjModel_* Model = mjScene.Model;
            MujocoLib.mjData_* Data = mjScene.Data;
            Vector3 bodyAngVel = Vector3.one;
            double[] mjBodyAngVel = new double[6];
            fixed (double* res = mjBodyAngVel)
            {
                MujocoLib.mj_objectVelocity(
                    mjScene.Model, mjScene.Data, (int)MujocoLib.mjtObj.mjOBJ_BODY, body.MujocoId, res, 1);
                bodyAngVel = MjEngineTool.UnityVector3(res, 0);
            }
            return bodyAngVel;
        }

    }
}