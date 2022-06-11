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
        // Start is called before the first frame update
        public static void ConvertMjToArticulationBody()
        {
            foreach(var mjBody in Selection.activeGameObject.GetComponentsInChildren<MjBody>())
            {
                SwitchToArticulationBody(mjBody);
            }
        }

        [MenuItem("GameObject/Convert Chain/Convert ArticulationBody Chain to MuJoCo")]
        // Start is called before the first frame update
        public static void ConvertArticulationBodyToMj()
        {

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
            

            foreach(var geom in mjBody.GetComponentsInDirectChildren<MjGeom>())
            {
                SwitchMjGeomToPrimitive(geom);
            }

            foreach (var joint in mjBody.GetComponentsInDirectChildren<MjBaseJoint>())
            {
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
            joint.Settings.Spring.Stiffness = drive.stiffness/Mathf.Deg2Rad;
            joint.Settings.Spring.Damping = drive.damping/Mathf.Deg2Rad;
            joint.RangeUpper = -drive.lowerLimit;
            joint.RangeLower = -drive.upperLimit;
        }

        private static Quaternion AnchorRotationFromJoints(IEnumerable<MjBaseJoint> joints)
        {
            if (joints.Count() == 1) return joints.First().transform.localRotation;

            var hingeList = joints.Cast<MjHingeJoint>().ToList();

            return  Quaternion.LookRotation(hingeList[0].transform.localRotation * Vector3.right, hingeList[1].transform.localRotation * Vector3.right) * Quaternion.Euler(0f, -90f, 0f);
        }

        private static void SwitchMjGeomToPrimitive(MjGeom geom)
        {
            var geomGO = geom.gameObject;
            GameObject primitive;
            switch(geom.ShapeType)
            {
                case MjShapeComponent.ShapeTypes.Box:
                {
                    geomGO.AddComponent<BoxCollider>();
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    primitive.transform.SetParent(geomGO.transform);
                    primitive.transform.localScale = geomGO.GetComponent<BoxCollider>().size;
                    break;
                }

                case MjShapeComponent.ShapeTypes.Capsule:
                {
                    var capsule = geomGO.AddComponent<CapsuleCollider>();
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    primitive.transform.SetParent(geomGO.transform);
                    primitive.transform.localScale = new Vector3(capsule.radius*2, capsule.height/2, capsule.radius*2);
                    break;
                }

                case MjShapeComponent.ShapeTypes.Sphere:
                {
                    var sphere = geomGO.AddComponent<SphereCollider>();
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    primitive.transform.SetParent(geomGO.transform);
                    primitive.transform.localScale = new Vector3(sphere.radius*2, sphere.radius * 2, sphere.radius * 2);
                    break;
                }

                case MjShapeComponent.ShapeTypes.Ellipsoid:
                {
                    geomGO.AddComponent<BoxCollider>();
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    primitive.transform.SetParent(geomGO.transform);
                    primitive.transform.localScale = geomGO.GetComponent<BoxCollider>().size;
                    break;
                }

                default:
                {
                    geomGO.AddComponent<BoxCollider>();
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    primitive.transform.SetParent(geomGO.transform);
                    primitive.transform.localScale = geomGO.GetComponent<BoxCollider>().size;
                    break;
                }
            }

            primitive.transform.localPosition = Vector3.zero;
            primitive.transform.localRotation = Quaternion.identity;
            DestroyImmediate(geomGO.GetComponent<MjMeshFilter>());
            DestroyImmediate(geomGO.GetComponent<MjGeom>());
            DestroyImmediate(geomGO.GetComponent<MjGeom>());
            DestroyImmediate(primitive.GetComponent<Collider>());

            EditorUtility.CopySerialized(geomGO.GetComponent<MeshRenderer>(), primitive.GetComponent<MeshRenderer>());
            DestroyImmediate(geomGO.GetComponent<MeshFilter>());
            DestroyImmediate(geomGO.GetComponent<MeshRenderer>());
            

            
        }

    }
}