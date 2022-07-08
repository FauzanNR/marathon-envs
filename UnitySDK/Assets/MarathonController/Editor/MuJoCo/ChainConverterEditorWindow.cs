using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Mujoco
{

    public class ChainConverterEditorWindow : EditorWindow
    {
        [MenuItem("GameObject/Convert Chain/Convert MuJoCo Chain to ArticulationBodies")]
        public static void ConvertMjToArticulationBody()
        {
            foreach (var mjBody in Selection.activeGameObject.GetComponentsInChildren<MjBody>())
            {
                SwitchToArticulationBody(mjBody);
            }
        }

        [MenuItem("GameObject/Convert Chain/Convert ArticulationBody Chain to MuJoCo")]
        public static void ConvertArticulationBodyToMj()
        {

            Transform actuatorParent = new GameObject("actuators").transform;
            actuatorParent.parent = Selection.activeGameObject.transform.parent;
            ArticulationBody buffBody = Selection.activeGameObject.transform.parent.gameObject.AddComponent<ArticulationBody>();
            foreach (var articulationBody in Selection.activeGameObject.GetComponentsInChildren<ArticulationBody>())
            {
                SwitchToMjBody(articulationBody, actuatorParent);
            }
            DestroyImmediate(buffBody);
        }

        public static void SwitchToMjBody(ArticulationBody articulationBody, Transform actuatorParent)
        {

            var mjB = articulationBody.gameObject.AddComponent<MjBody>();
            //artB.mass = mjBody.GetComponentsInDirectChildren<MjGeom>().Select(g => g.GetMass()).Sum();

            if (articulationBody.transform.parent.GetComponent<ArticulationBody>() && articulationBody.transform.parent.GetComponent<ArticulationBody>().isRoot)
            {
                var joint = new GameObject($"{mjB.name}root").AddComponent<MjFreeJoint>();
                joint.transform.parent = mjB.transform;
            }

            else if (articulationBody.jointType == ArticulationJointType.RevoluteJoint)
            {
                var joint = new GameObject($"{mjB.name}x").AddComponent<MjHingeJoint>();
                joint.transform.localPosition = articulationBody.anchorPosition;
                joint.transform.localRotation = articulationBody.anchorRotation;
                if (articulationBody.xDrive.forceLimit > 0) AddActuator(actuatorParent, joint);
                joint.transform.parent = mjB.transform;
            }

            else if (articulationBody.jointType == ArticulationJointType.SphericalJoint)
            {
                if (!(articulationBody.twistLock == ArticulationDofLock.LockedMotion))
                {
                    var twistJoint = new GameObject($"{mjB.name}x").AddComponent<MjHingeJoint>();
                    twistJoint.transform.localPosition = articulationBody.anchorPosition;
                    twistJoint.transform.localRotation = articulationBody.anchorRotation;
                    if (articulationBody.xDrive.forceLimit > 0) AddActuator(actuatorParent, twistJoint);
                    UpdateMjJointFromArticulationDrive(ref twistJoint, articulationBody.xDrive);
                    twistJoint.transform.parent = mjB.transform;
                }
                if (!(articulationBody.swingYLock == ArticulationDofLock.LockedMotion))
                {
                    var swingYJoint = new GameObject($"{mjB.name}y").AddComponent<MjHingeJoint>();
                    swingYJoint.transform.localPosition = articulationBody.anchorPosition;
                    swingYJoint.transform.localRotation = Quaternion.FromToRotation(Vector3.right, Vector3.up) * articulationBody.anchorRotation;
                    if (articulationBody.yDrive.forceLimit > 0) AddActuator(actuatorParent, swingYJoint);
                    UpdateMjJointFromArticulationDrive(ref swingYJoint, articulationBody.yDrive);
                    swingYJoint.transform.parent = mjB.transform;

                }
                if (!(articulationBody.swingZLock == ArticulationDofLock.LockedMotion))
                {
                    var swingZJoint = new GameObject($"{mjB.name}z").AddComponent<MjHingeJoint>();
                    swingZJoint.transform.localPosition = articulationBody.anchorPosition;
                    swingZJoint.transform.localRotation = Quaternion.FromToRotation(Vector3.right, Vector3.forward) * articulationBody.anchorRotation;
                    if (articulationBody.zDrive.forceLimit > 0) AddActuator(actuatorParent, swingZJoint);
                    UpdateMjJointFromArticulationDrive(ref swingZJoint, articulationBody.zDrive);
                    swingZJoint.transform.parent = mjB.transform;
                }
            }

            foreach (var joint in mjB.GetComponentsInDirectChildren<MjBaseJoint>())
            {
                joint.transform.position = articulationBody.anchorPosition + mjB.transform.position;
                var hinge = joint as MjHingeJoint;
                if(hinge != null) hinge.Settings.Armature = 0.01f;
            }

/*            else if (articulationBody.dofCount == 3)
            {
                var ballJoint = new GameObject($"{mjB.name}ball").AddComponent<MjBallJoint>();
                ballJoint.transform.localPosition = articulationBody.anchorPosition;
                ballJoint.transform.localRotation = articulationBody.anchorRotation;
                float rotAngle = new Vector3(articulationBody.xDrive.upperLimit - articulationBody.xDrive.lowerLimit, articulationBody.yDrive.upperLimit - articulationBody.yDrive.lowerLimit, articulationBody.zDrive.upperLimit - articulationBody.zDrive.lowerLimit).magnitude;
                ballJoint.RangeUpper = rotAngle;
                if (articulationBody.xDrive.forceLimit > 0) AddActuator(actuatorParent, ballJoint);
                if (articulationBody.yDrive.forceLimit > 0) AddActuator(actuatorParent, ballJoint, gearIdx: 1);
                if (articulationBody.zDrive.forceLimit > 0) AddActuator(actuatorParent, ballJoint, gearIdx: 2);
                ballJoint.transform.parent = mjB.transform;

            }*/


            foreach (var coll in articulationBody.GetComponentsInDirectChildren<Collider>())
            {
                SwitchColliderToMjGeom(coll);
            }

            if (!mjB.GetComponentInDirectChildren<MjGeom>())
            {
                var go = new GameObject("PointMass");
                go.transform.parent = mjB.transform;
                var geom = go.AddComponent<MjGeom>();
                Debug.Log(mjB.name);
                geom.transform.localPosition = mjB.GetComponentInDirectChildren<MjBaseJoint>().transform.localPosition;

                geom.ShapeType = MjShapeComponent.ShapeTypes.Sphere;
                geom.Sphere.Radius = 0.02f;

                geom.Mass = articulationBody.mass;
            }

            DestroyImmediate(articulationBody);

        }

        private static void SwitchToArticulationBody(MjBody mjBody)
        {
            var artB = mjBody.gameObject.AddComponent<ArticulationBody>();
            artB.mass = mjBody.GetComponentsInDirectChildren<MjGeom>().Select(g => g.GetMass()).Sum();

            if (mjBody.GetComponentInDirectChildren<MjBaseJoint>())
            {

                artB.anchorPosition = mjBody.GetComponentInDirectChildren<MjBaseJoint>().transform.localPosition;

                artB.anchorRotation = AnchorRotationFromJoints(mjBody.GetComponentsInDirectChildren<MjBaseJoint>());

                int mjDofCount = mjBody.GetComponentsInDirectChildren<MjBaseJoint>().Select(j => j.DoFCount()).Sum();

                if (mjDofCount == 1)
                {
                    artB.jointType = ArticulationJointType.RevoluteJoint;
                    artB.twistLock = ArticulationDofLock.LimitedMotion;
                    artB.xDrive = ArticulationDriveFromHinge(mjBody.GetComponentInDirectChildren<MjHingeJoint>());
                }
                else
                {
                    artB.jointType = ArticulationJointType.SphericalJoint;

                    artB.twistLock = ArticulationDofLock.LimitedMotion;
                    artB.swingYLock = ArticulationDofLock.LimitedMotion;
                    artB.swingZLock = ArticulationDofLock.LimitedMotion;

                    List<MjHingeJoint> hinges = mjBody.GetComponentsInDirectChildren<MjHingeJoint>().ToList();
                    if (hinges.Count > 0) artB.xDrive = ArticulationDriveFromHinge(hinges[0]);
                    if (hinges.Count > 1) artB.yDrive = ArticulationDriveFromHinge(hinges[1]);
                    if (hinges.Count > 2) artB.zDrive = ArticulationDriveFromHinge(hinges[2]);

                    if (mjDofCount == 2) artB.swingZLock = ArticulationDofLock.LockedMotion;
                }
            }


            foreach (var geom in mjBody.GetComponentsInDirectChildren<MjGeom>())
            {
                SwitchMjGeomToCollider(geom);
            }

            foreach (var joint in mjBody.GetComponentsInDirectChildren<MjBaseJoint>())
            {
                var act = joint.FindActuator();
                if (act) DestroyImmediate(act);
                DestroyImmediate(joint.gameObject);
            }

            DestroyImmediate(mjBody);
        }

        private static ArticulationDrive ArticulationDriveFromHinge(MjHingeJoint joint)
        {
            var drive = new ArticulationDrive();
            drive.stiffness = joint.Settings.Spring.Stiffness / Mathf.Rad2Deg;
            drive.damping = joint.Settings.Spring.Damping / Mathf.Rad2Deg;
            drive.upperLimit = -joint.RangeLower;
            drive.lowerLimit = -joint.RangeUpper;
            var act = joint.FindActuator();

            drive.forceLimit = 0f;

            if (act)
            {
                float ctrlLimit = act.CommonParams.CtrlLimited ? Mathf.Min(Mathf.Abs(act.CommonParams.Gear[0] * act.CommonParams.CtrlRange.x), Mathf.Abs(act.CommonParams.Gear[0] * act.CommonParams.CtrlRange.y)) : float.PositiveInfinity;
                float forceLimit = act.CommonParams.ForceLimited ? Mathf.Min(Mathf.Abs(act.CommonParams.ForceRange.x), Mathf.Abs(act.CommonParams.ForceRange.y)) : float.PositiveInfinity;
                drive.forceLimit = Mathf.Min(ctrlLimit, forceLimit);
            }

            return drive;
        }


        private static void UpdateMjJointFromArticulationDrive(ref MjHingeJoint joint, ArticulationDrive drive)
        {
            joint.Settings.Spring.Stiffness = drive.stiffness / Mathf.Deg2Rad;
            joint.Settings.Spring.Damping = drive.damping / Mathf.Deg2Rad;
            joint.RangeUpper = -drive.lowerLimit;
            joint.RangeLower = -drive.upperLimit;
            var act = joint.FindActuator();

            if (act)
            {
                if (drive.forceLimit == float.PositiveInfinity) return;
                act.CommonParams.ForceRange = new Vector2(-drive.forceLimit, drive.forceLimit);
                act.CommonParams.CtrlRange = new Vector2(-drive.forceLimit / act.CommonParams.Gear[0], drive.forceLimit / act.CommonParams.Gear[0]);
            }
        }

        private static Quaternion AnchorRotationFromJoints(IEnumerable<MjBaseJoint> joints)
        {
            if (joints.Count() == 1) return joints.First().transform.localRotation;

            var hingeList = joints.Cast<MjHingeJoint>().ToList();

            return Quaternion.LookRotation(hingeList[0].transform.localRotation * Vector3.right, hingeList[1].transform.localRotation * Vector3.right) * Quaternion.Euler(0f, -90f, 0f);
        }

        private static void SwitchMjGeomToCollider(MjGeom geom)
        {
            var geomGO = geom.gameObject;
            //GameObject primitive;
            switch (geom.ShapeType)
            {
                case MjShapeComponent.ShapeTypes.Box:
                    {
                        geomGO.AddComponent<BoxCollider>();
                        /*primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        primitive.transform.SetParent(geomGO.transform);
                        primitive.transform.localScale = geomGO.GetComponent<BoxCollider>().size;*/
                        break;
                    }

                case MjShapeComponent.ShapeTypes.Capsule:
                    {
                        var capsule = geomGO.AddComponent<CapsuleCollider>();
                        /*primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        primitive.transform.SetParent(geomGO.transform);
                        primitive.transform.localScale = new Vector3(capsule.radius*2, capsule.height/2, capsule.radius*2);*/
                        break;
                    }

                case MjShapeComponent.ShapeTypes.Sphere:
                    {
                        var sphere = geomGO.AddComponent<SphereCollider>();
                        /*primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        primitive.transform.SetParent(geomGO.transform);
                        primitive.transform.localScale = new Vector3(sphere.radius*2, sphere.radius * 2, sphere.radius * 2);*/
                        break;
                    }

                case MjShapeComponent.ShapeTypes.Ellipsoid:
                    {
                        geomGO.AddComponent<BoxCollider>();
                        /*primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        primitive.transform.SetParent(geomGO.transform);
                        primitive.transform.localScale = geomGO.GetComponent<BoxCollider>().size;*/
                        break;
                    }

                default:
                    {
                        geomGO.AddComponent<BoxCollider>();
                        /*primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        primitive.transform.SetParent(geomGO.transform);
                        primitive.transform.localScale = geomGO.GetComponent<BoxCollider>().size;*/
                        break;
                    }
            }

            /*primitive.transform.localPosition = Vector3.zero;
            primitive.transform.localRotation = Quaternion.identity;*/
            Mesh oldMesh = geomGO.GetComponent<MeshFilter>().sharedMesh;
            Mesh meshCopy = new Mesh();
            meshCopy.vertices = oldMesh.vertices;
            meshCopy.triangles = oldMesh.triangles;
            meshCopy.RecalculateNormals();
            meshCopy.RecalculateTangents();
            meshCopy.name = oldMesh.name;
            DestroyImmediate(geomGO.GetComponent<MjMeshFilter>());
            DestroyImmediate(geomGO.GetComponent<MjGeom>());
            geomGO.GetComponent<MeshFilter>().mesh = meshCopy;
            //DestroyImmediate(geomGO.GetComponent<MjGeom>());
            //DestroyImmediate(primitive.GetComponent<Collider>());

            //EditorUtility.CopySerialized(geomGO.GetComponent<MeshRenderer>(), primitive.GetComponent<MeshRenderer>());
            //DestroyImmediate(geomGO.GetComponent<MeshFilter>());
            //DestroyImmediate(geomGO.GetComponent<MeshRenderer>());



        }

        private static void SwitchColliderToMjGeom(Collider coll)
        {
            MjGeom geom = coll.gameObject.AddComponent<MjGeom>();
            geom.transform.localPosition = coll.transform.localPosition;
            geom.transform.localRotation = coll.transform.localRotation;
            switch (coll)
            {
                case BoxCollider b:
                    geom.ShapeType = MjShapeComponent.ShapeTypes.Box;
                    geom.Box.Extents = b.size / 2;
                    geom.transform.localPosition += b.center;
                    break;
                case SphereCollider s:
                    geom.ShapeType = MjShapeComponent.ShapeTypes.Sphere;
                    geom.Sphere.Radius = s.radius;
                    geom.transform.localPosition += s.center;
                    break;
                case CapsuleCollider c:
                    geom.ShapeType = MjShapeComponent.ShapeTypes.Capsule;
                    geom.Capsule.Radius = c.radius;
                    geom.Capsule.HalfHeight = Mathf.Max((c.height - 2 * c.radius) / 2, 0.001f);
                    geom.transform.localPosition += c.center;
                    if (c.direction == 0) geom.transform.localRotation *= Quaternion.Euler(0f, 0f, 90f);
                    if (c.direction == 2) geom.transform.localRotation *= Quaternion.Euler(90f, 0f, 0f);
                    break;
                default:
                    throw new System.NotImplementedException($"Collider type of \"{coll.name}\" not supported");
            }
            
            coll.gameObject.AddComponent<MjMeshFilter>();
            geom.Mass = coll.attachedArticulationBody.mass;
            DestroyImmediate(coll);
        }

        private static MjActuator AddActuator(Transform actuatorParent, MjBaseJoint joint, int gearIdx = 0)
        {
            var act = new GameObject().AddComponent<MjActuator>();
            act.name = joint.name;
            act.transform.parent = actuatorParent;
            act.Joint = joint;
            act.CommonParams.Gear = Enumerable.Repeat(0f, act.CommonParams.Gear.Count).ToList();
            act.CommonParams.Gear[gearIdx] = 1;
            return act;
        }
    }
}