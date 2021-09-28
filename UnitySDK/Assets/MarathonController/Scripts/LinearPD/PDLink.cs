using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using MathNet.Numerics.LinearAlgebra;


namespace LinearPDController {

    public class PDLink
    {

        public static bool isLinearStable = true;

        ArticulationBody _ab;

        //equation is:
        //f_I = I_i a_i + p_i

        //CORIOLIS, GRAVITY AND OTHER EXTERNAL FORCES
        Vector<double> Z_i; // Vector<double>.Build.Dense(6);
                            // we use this variable to store 2 values:
                            //  1. isolated zero acceleration force  Z_i (Mirtich) or Bias Force p_i^A (Featherstone14)
                            //  2. spatial articulated zero-acceleration force  Z_i^A (Mirtich) or articulated Bias Force p_i^a (Featherstone14)
                            //When put together in a big matrix, we write C(q, q^dot) 

        Vector<double> c_i;  //Vector<double>.Build.Dense(6);
                             //Spatial Coriolis Force

        //INERTIA or generalized Mass
        Matrix<double> I_i;  //Matrix<double>.Build.Dense(6, 6);
                             //we use this variable to store 2 values:
                             //  1. spatial isolated inertia I_i (Mirtich) or rigid-body inertia I_i (Featherstone14)
                             //  2. spatial articulated inertia I_i^A (Mirtich) or rigid-body apparent  inertia I_i^a  (Featherstone14)
                             //When put together in a big matrix, we often write M(q), sometimes also H


        Vector<double> acceleration;

        PDLink dad;
      //  int index;

        //Inertia inverse, kept here  to not calculate it twice (in passes 2 and 3), In Featherstone it is D.
        Matrix<double> SISInverted;

        Vector3 Qtorque;

        [Header("Parameters for LinearPD:")]
        public static float KP = 50;
        

        public ArticulationBody articulationBody { get => _ab;  }

        PDLink()
        {
        }


        PDLink(ArticulationBody a, List<PDLink> LinksCreated)
        {

            PDLink me = LinksCreated.FirstOrDefault(x => x._ab.Equals(a));
            if (me == null)
            {
                me = new PDLink();
                me._ab = a;


                PDLink dad = null;
                ArticulationBody abdad = a.transform.parent.GetComponent<ArticulationBody>();
                if (abdad != null)
                {
                    dad = LinksCreated.FirstOrDefault(x => x._ab.Equals(abdad));

                }
                else if (!me._ab.isRoot)
                {
                    Debug.Log(a.name + " does not seem to have an articulationBody as parent");

                }

                if (dad != null)
                {
                    me.dad = dad;

                }
                else
                {
                    if (!me._ab.isRoot) {
                        Debug.Log(a.name + "'s dad does not seem to be in the list of sorted links, (this should not happen in a sorted list?)");
                        dad = new PDLink();
                        dad._ab = abdad;
                        LinksCreated.Add(dad);
                    }


                }

                me.acceleration = Vector<double>.Build.Dense(6);
                LinksCreated.Add(me);

            }
           // me.index = idx;

        }


        //public bool isRoot{ get { return _ab.isRoot; } }



    public static PDLink[] SortLinks(ArticulationBody root)
    {

        ArticulationBody[] ts = root.GetComponentsInChildren<ArticulationBody>();

       List<PDLink> LinksCreated = new List<PDLink>();


            //  recursive function that returns a sorted list from the tree of nodes
            NumberLinks(root, 1);


            
            return LinksCreated.ToArray();

            int NumberLinks(ArticulationBody node, int idx)
        {

            //Debug.Log(" linking node " + node.name + " to index " + idx);

            PDLink PDnode = new PDLink(node, LinksCreated);

            
            idx += 1;
            //notice this is recursive
            foreach (Transform child in node.transform)
            {
                ArticulationBody abchild = child.GetComponent<ArticulationBody>();
                if (abchild != null)
                    idx = NumberLinks(abchild, idx);
            }

            return idx;


        }

    }

        #region pass1

        //auxiliary function just to get the ucrrent velocity of the rigidBody
        public ArticulationReducedSpace currentVel() {

            ArticulationReducedSpace res = new ArticulationReducedSpace();
            res.dofCount = 3;

            if (_ab.jointType != ArticulationJointType.SphericalJoint)
                return res;
           
            if(_ab.dofCount > 0) { 
                if (_ab.twistLock == ArticulationDofLock.LimitedMotion)
                    res[0] = _ab.jointVelocity[0];
                else
                    res[0] = 0.0f;

                if (_ab.swingYLock== ArticulationDofLock.LimitedMotion)
                    res[1] = _ab.jointVelocity[1];
                else
                    res[1] = 0.0f;

                if (_ab.swingZLock == ArticulationDofLock.LimitedMotion)
                    res[2] = _ab.jointVelocity[2];
                else
                    res[2] = 0.0f;
            }
            return res;

        
        }


        //Mirtich pge 116
        //public void updateVelocities(Vector3 qVel) {
        public void updateVelocities(ArticulationReducedSpace qVel)
        {

        ArticulationBody ab = this._ab;
            if (ab.isRoot)
                return;

        Quaternion Rot = ab.transform.localRotation;

        ArticulationBody dad = ab.transform.parent.GetComponent<ArticulationBody>();
        Vector3 Rad = ab.centerOfMass - dad.centerOfMass;


        //w_i = Rot * w_h
        ab.angularVelocity = Rot * dad.angularVelocity;

        //v_i = Rot * w_i x r
        ab.velocity = Rot * dad.velocity + Vector3.Cross(ab.angularVelocity, Rad);


        Vector3 d_i = get_d(ab);

        Vector3 u_i = get_u(ab);



        ab.angularVelocity += Utils.Scale(u_i, qVel);


         

        Vector3 temp2 = Vector3.Cross(u_i, d_i);
        ab.velocity += Utils.Scale(temp2, qVel);

        }

    public Vector<double> calculateZ_i() { 
    //A. we calculate Z_i
        Vector3 tmp = _ab.inertiaTensor;
        tmp.Scale(_ab.angularVelocity);
        tmp = Vector3.Cross(_ab.angularVelocity, tmp);
           
       //TODO: sort out if here we can put more than gravity
       double[] a = { 0.0, -1.0 * _ab.mass * 9.81, 0.0, tmp[0], tmp[1], tmp[2] };

        Z_i = Vector<double>.Build.Dense(a);
        return Z_i;
    }

    //B. We calculate I_i
    public Matrix<double> calculateI_i() { 
        Matrix<double> I_i = Matrix<double>.Build.Dense(6, 6);
        
        //this could be done only once
        I_i[0, 3] = _ab.mass;
        I_i[1, 4] = _ab.mass;
        I_i[2, 5] = _ab.mass;

        //this has to be done in each frame:
        I_i[3, 0] = _ab.inertiaTensor.x;
        I_i[4, 1] = _ab.inertiaTensor.y;
        I_i[5, 2] = _ab.inertiaTensor.z;

        this.I_i = I_i;

        return this.I_i;

    }

    //C. We calculate c_i
    public Vector<double> updateCoriolis() {




            if (_ab.isRoot)
            {
                double[] noC = {  0, 0, 0, 0, 0, 0  };
                this.c_i = Vector<double>.Build.Dense(noC);
                return this.c_i;

            }

        Vector3 vel_i = Utils.Scale( get_u(_ab), _ab.jointVelocity);
        ArticulationBody dad = _ab.transform.parent.GetComponent<ArticulationBody>();

        Vector3 d_i = get_d(_ab);
        

        Vector3 tmp = Vector3.Cross(dad.angularVelocity, vel_i);
        Vector3 tmp2 = Vector3.Cross(dad.angularVelocity, vel_i)
                        + 2 * Vector3.Cross(dad.angularVelocity, Vector3.Cross(vel_i, d_i))
                        + Vector3.Cross(vel_i, Vector3.Cross(vel_i, d_i));

        double[] c = { tmp[0], tmp[1], tmp[2], tmp2[0], tmp2[1], tmp[2] };
        this.c_i = Vector<double>.Build.Dense(c);
        return this.c_i;
    }



        #endregion




        #region pass2

        public void updateQtorque(Vector3 targetCoord, float actionTimeDelta)
        {




            if (isLinearStable) { 

            float KD = KP * actionTimeDelta * 1.5f;

            Qtorque = -KP * (Utils.GetArticulationReducedSpaceInVector3(_ab.jointPosition) + actionTimeDelta * Utils.GetArticulationReducedSpaceInVector3(_ab.jointVelocity) - (targetCoord))
                       - KD * Utils.GetArticulationReducedSpaceInVector3(_ab.jointVelocity);
        }else{

                Debug.LogError("only implemented linear stable case");
        
        }

        }

    //treeFwdDynamics Mirtich page 121


    public void updateIZ_dad() {
           updateIZ_dad(Vector3.zero, Qtorque);

    }



    void updateIZ_dad(Vector3 forces, Vector3 torques)
    {


         //A. we calculate I_h^A
        ArticulationBody ab = this._ab;
        if (ab.isRoot)
            return;
        
        ArticulationBody abdad = this.dad._ab;

        Vector3 u = get_u(ab);
        Vector3 uxd = Vector3.Cross(u, get_d(ab));
        double[] a = { u[0], u[1], u[2], uxd[0], uxd[1], uxd[2] };
        Vector<double> S_i = Vector<double>.Build.Dense(a);
        Matrix<double> S_iT = spatialTranspose(S_i);//it is a row matrix
        Matrix<double> dadXab = get_bXa(ab, abdad);
        Matrix<double> abXdad = get_bXa(abdad, ab);

        //update I_dad
        Matrix<double> tmp = I_i * S_i.ToColumnMatrix() * S_iT * I_i;

        SISInverted = (S_iT * I_i * S_i.ToColumnMatrix()).Inverse(); //this is a scalar??? TO FIX
            dad.I_i += dadXab * (I_i - SISInverted[0,0] * tmp) * abXdad;


        //B. we calculate Z_h^A
       //update Z_dad
        double[] q = { forces.x, forces.y, forces.z, torques.x, torques.y, torques.z };
        Vector<double> Q = Vector<double>.Build.Dense(q);


        var test1 = I_i * S_i.ToColumnMatrix();
        var test2 = I_i * c_i;
        var test3 = S_iT * (Z_i + test2);//AAAAAAAAAARG JL 

            var resTest3 = test3[0];
            var tmp4 = (Q - resTest3);
        Vector<double> tmp3 = test1 * tmp4;
        dad.Z_i += dadXab * (Z_i + I_i * c_i + SISInverted * tmp3);


    }

        #endregion

        #region pass3

    public Vector<double> updateAcceleration() {
            return updateAcceleration(Vector3.zero, Qtorque);

        }

    Vector<double> updateAcceleration(Vector3 forces, Vector3 torques) 
    {

            double[] q = { forces.x, forces.y, forces.z, torques.x, torques.y, torques.z };
            Vector<double> Q = Vector<double>.Build.Dense(q);


            Vector3 u = get_u(_ab);
            Vector3 uxd = Vector3.Cross(u, get_d(_ab));
            double[] a = { u[0], u[1], u[2], uxd[0], uxd[1], uxd[2] };
            Vector<double> S_i = Vector<double>.Build.Dense(a);
            Matrix<double> S_iT = spatialTranspose(S_i);//it is a row matrix
                                                        //Matrix<double> dadXab = get_bXa(_ab, abdad);

            ArticulationBody abdad = this.dad._ab;
            Matrix<double> abXdad = get_bXa(abdad, this._ab);

            //TODO: check how we deal with a(0) = 0;

            Vector<double> qAccel = SISInverted * (Q - S_iT * abXdad * dad.acceleration - S_iT * (Z_i + I_i * c_i));

            acceleration = abXdad * dad.acceleration + c_i + qAccel * S_i;

            return qAccel;
        }


    #endregion


        #region utils
        static Matrix<double> spatialTranspose(Vector<double> input)
    {
        double[] oarray = { input[3], input[4], input[5], input[0], input[1], input[2] };
        Vector<double> output = Vector<double>.Build.Dense(oarray);
        return output.ToRowMatrix();
    }


    //WRONG THIS WOULD ONLY WORK FOR 1 DOF, not for 3.

    static Vector3 get_u(ArticulationBody ab)
    {
        Vector3 u_i = new Vector3();
        ab.transform.rotation.ToAngleAxis(out _, out u_i);
        return u_i;

    }


    static Vector3 get_d(ArticulationBody ab)
    {
        return (ab.centerOfMass - ab.anchorPosition);

    }





    static Vector3 get_r(ArticulationBody ab, ArticulationBody dad)
    {
        return (ab.centerOfMass - dad.centerOfMass);


    }



    //see Mirtich appendix A.2, page 230
    static Matrix<double> get_rtilde(Vector3 r)
    {


        //we are describing column by column:
        double[] rtarray = {    0  , r.z,-r.y ,
                               -r.z, 0  , r.x ,
                                r.y,-r.x, 0    };

        Matrix<double> rt = Matrix<double>.Build.Dense(3, 3, rtarray);
        return rt;
    }

    //return a 3x3 matrix rotation from a quaternion
    //see https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation
    static Matrix<double> get_Rot3x3(Quaternion q)
    {

        // in this list each row represents a column in the resulting matrix:
        // we list describing column by column, assuming the norm of the quaternion is 1
        double[] Rarray = { 1 - 2 * (q.y * q.y + q.z * q.z),  2* (q.x * q.y + q.z * q.w )   , 2* (q.x * q.z - q.y * q.w)      ,
                            2* (q.x * q.y - q.z * q.w )    , 1 - 2 * (q.x * q.x + q.z * q.z), 2* (q.y * q.z + q.x * q.w )     ,
                            2* (q.x * q.z + q.y * q.w)     , 2* (q.y * q.z - q.x * q.w )    , 1 - 2 * (q.x * q.x + q.y * q.y) };

        Matrix<double> R = Matrix<double>.Build.Dense(3, 3, Rarray);
        return R;

    }

    //Returns the spatial transform matrix that goes from frame of reference a to frame of reference b
    static Matrix<double> get_bXa(ArticulationBody a, ArticulationBody b)
    {


        //the spatial transformation matrix, see Mirtich page 102

        Matrix<double> rtild = get_rtilde(get_r(a, b));

        //Matrix<double> R = get_R(ab.transform.localRotation);
        Quaternion localRot = a.transform.rotation * Quaternion.Inverse(b.transform.rotation);//todo check  that when a is son of b, it is equivalent to a.transform.localRotation;

        Matrix<double> R = get_Rot3x3(localRot);


        //this builds it by rows?
        Matrix<double>[,] x = { { R, Matrix<double>.Build.Dense(3, 3)},
                                    { -rtild * R, R                      } };

        Matrix<double> bXa = Matrix<double>.Build.DenseOfMatrixArray(x);
        return bXa;

    }

    #endregion

}



}//namespace


