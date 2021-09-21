using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;

public class LinearPD : MonoBehaviour
{


    class PDLink {

       

        ArticulationBody _ab;

        //CORIOLIS, GRAVITY AND OTHER EXTERNAL FORCES
        public Vector<double>  Z_i; // Vector<double>.Build.Dense(6);
                             // we use this variable to store 2 values:
                             //  1. isolated zero acceleration force  Z_i (Mirtich) or Bias Force p_i^A (Featherstone14)
                             //  2. spatial articulated zero-acceleration force  Z_i^A (Mirtich) or articulated Bias Force p_i^a (Featherstone14)
                             //When put together in a big matrix, we write C(q, q^dot) 

        public Vector<double>  c_i;  //Vector<double>.Build.Dense(6);
                              //Spatial Coriolis Force

        //INERTIA or generalized Mass
        public Matrix<double> I_i;  //Matrix<double>.Build.Dense(6, 6);
                             //we use this variable to store 2 values:
                             //  1. spatial isolated inertia I_i (Mirtich) or rigid-body inertia I_i (Featherstone14)
                             //  2. spatial articulated inertia I_i^A (Mirtich) or rigid-body apparent  inertia I_i^a  (Featherstone14)
                             //When put together in a big matrix, we often write M(q), sometimes also H



        PDLink dad;


        PDLink() { 
        }
        

        PDLink(ArticulationBody a, List<PDLink> SortedPDLinks) {

            PDLink me = SortedPDLinks.FirstOrDefault(x => x._ab.Equals(a));
            if (me == null) {
                PDLink pdl = new PDLink();
                pdl._ab = a;        


                PDLink dad = null;
                ArticulationBody abdad = a.transform.parent.GetComponent<ArticulationBody>();
                if (abdad != null)
                {
                    dad = SortedPDLinks.FirstOrDefault(x => x._ab.Equals(abdad));

                }
                else {
                    Debug.Log(a.name + " does not seem to have an articulationBody as parent");
            
                }

                if (dad != null)
                {
                    pdl.dad = dad;

                }
                else {
                    Debug.LogError(a.name + "'s dad does not seem to be in the list of sorted links, this should not happen in a sorted list");


                }

            }

        }

        public ArticulationBody ArticulationBody { get => _ab;}

        public static PDLink[] SortLinks(ArticulationBody root)
        {

            ArticulationBody[] ts = root.GetComponentsInChildren<ArticulationBody>();



            PDLink[] sortedLinks = new PDLink[ts.Length];




            NumberLinks(root, 1);


            return sortedLinks;


            int NumberLinks(ArticulationBody node, int idx)
            {

                PDLink PDnode = new PDLink(node, sortedLinks.ToList<PDLink>());

                sortedLinks[idx - 1] = PDnode;
                idx += 1;
                //notice this is recursive
                foreach (Transform child in node.transform) {
                    ArticulationBody abchild= child.GetComponent<ArticulationBody>();
                    if(abchild != null)
                        idx = NumberLinks(abchild, idx);
                }

                return idx;


            }

        }

        public Matrix<double> updateI_i() {
            ArticulationBody ab = this._ab;
            ArticulationBody abdad = this.dad.ArticulationBody;

            Vector3 u = get_u(ab);
            Vector3 uxd = Vector3.Cross(u, get_d(ab));

            double[] a = { u[0], u[1], u[2], uxd[0], uxd[1], uxd[2] };
            Vector<double> S_i = Vector<double>.Build.Dense(a);
            
            Matrix<double> S_iT = spatialTranspose(S_i);//it is a row matrix
            Matrix<double> dadXab = get_bXa(ab, abdad);
            Matrix<double> abXdad = get_bXa(abdad, ab);

            Matrix<double> tmp = I_i * S_i.ToColumnMatrix() * S_iT * I_i;

            Matrix<double> tmp2 = (S_iT * I_i * S_i.ToColumnMatrix()).Inverse();

            dad.I_i += dadXab * (I_i - tmp2 * tmp) * abXdad;
            


        }
    }





    PDLink[] PDLinks;

   // ArticulationBody[] Links;


    [SerializeField]
    ArticulationBody theRoot;


    //equation is:
    //f_I = I_i a_i + p_i

    /*
    //CORIOLIS, GRAVITY AND OTHER EXTERNAL FORCES
    Vector<double>[] Z_i;  //array of vectors  Vector<double>.Build.Dense(6);
                           //we use this variable to store 2 values:
                           //  1. isolated zero acceleration force  Z_i (Mirtich) or Bias Force p_i^A (Featherstone14)
                           //  2. spatial articulated zero-acceleration force  Z_i^A (Mirtich) or articulated Bias Force p_i^a (Featherstone14)
                           //When put together in a big matrix, we write C(q, q^dot) 


    //Spatial Coriolis Force:
    Vector<double>[] c_i;  //array of vectors  Vector<double>.Build.Dense(6);



    //INERTIA or generalized Mass
    Matrix<double>[] I_i;       //array of matrices  Matrix<double>.Build.Dense(6, 6);
                                //we use this variable to store 2 values:
                                //  1. spatial isolated inertia I_i (Mirtich) or rigid-body inertia I_i (Featherstone14)
                                //  2. spatial articulated inertia I_i^A (Mirtich) or rigid-body apparent  inertia I_i^a  (Featherstone14)
                                // when put together in a big matrix, we often write M(q)
    */

    #region utils
    static Matrix<double> spatialTranspose(Vector<double> input) 
    {
        double[] oarray = { input[3], input[4], input[5], input[0], input[1], input[2] };
        Vector<double> output = Vector<double>.Build.Dense(oarray);
        return  output.ToRowMatrix();    
    }

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
        return (ab.centerOfMass - dad.centerOfMass );


    }



    //see Mirtich appendix A.2, page 230
    static Matrix<double> get_rtilde(Vector3 r) {


        //we are describing column by column:
        double[] rtarray = {    0  , r.z,-r.y ,
                               -r.z, 0  , r.x ,
                                r.y,-r.x, 0    };

        Matrix<double> rt = Matrix<double>.Build.Dense(3,3,rtarray);
        return rt;
    }

    //return a 3x3 matrix rotation from a quaternion
    //see https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation
    static Matrix<double> get_Rot3x3(Quaternion q) {

        // in this list each row represents a column in the resulting matrix:
        // we list describing column by column, assuming the norm of the quaternion is 1
        double[] Rarray = { 1 - 2 * (q.y * q.y + q.z * q.z),  2* (q.x * q.y + q.z * q.w )   , 2* (q.x * q.z - q.y * q.w)      ,
                            2* (q.x * q.y - q.z * q.w )    , 1 - 2 * (q.x * q.x + q.z * q.z), 2* (q.y * q.z + q.x * q.w )     ,
                            2* (q.x * q.z + q.y * q.w)     , 2* (q.y * q.z - q.x * q.w )    , 1 - 2 * (q.x * q.x + q.y * q.y) };

        Matrix<double> R = Matrix<double>.Build.Dense(3, 3, Rarray);
        return R;

    }

    //Returns the spatial transform matrix that goes from frame of reference a to frame of reference b
    static Matrix<double> get_bXa(ArticulationBody a, ArticulationBody b) {


        //the spatial transformation matrix, see Mirtich page 102

        Matrix<double> rtild = get_rtilde(get_r(a, b));

        //Matrix<double> R = get_R(ab.transform.localRotation);
        Quaternion localRot = a.transform.rotation * Quaternion.Inverse(b.transform.rotation);//todo that when a is son of b, it is equivalent to a.transform.localRotation;

        Matrix<double> R = get_Rot3x3(localRot);


        //this builds it by rows?
        Matrix<double>[,] x = { { R, Matrix<double>.Build.Dense(3, 3)},
                                    { -rtild * R, R                      } };

        Matrix<double> bXa = Matrix<double>.Build.DenseOfMatrixArray(x);
        return bXa;

    }

    #endregion






    // Start is called before the first frame update
    void Start()
    {
        Init(); 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       

    }


    void Init()
    {
        if (theRoot == null | !theRoot.isRoot)
        {
            Debug.LogError("LinearPD does not know who is the root of the articulationBody Hierarchy");
        }



        /*
        Transform[]  SortedTransforms = SortTransforms(theRoot.transform);

        Links = new ArticulationBody[SortedTransforms.Length];

        for (int i=0;  i < SortedTransforms.Length; i++) {

            Links[i] = SortedTransforms[i].GetComponent<ArticulationBody>();
        
        }



        Vector<double>[] Z_i = new Vector<double>[Links.Length];
        //for (int j = 0; j < Links.Length; j++)
        //    Z_i[j] = Vector<double>.Build.Dense(6);


        Matrix<double>[] I_i = new Matrix<double>[Links.Length];
        //for (int j = 0; j < Links.Length; j++)
        //    I_i[j] = Matrix<double>.Build.Dense(6, 6);


        Vector<double>[] c_i = new Vector<double>[Links.Length];
        */


        PDLinks = PDLink.SortLinks(theRoot);


    }


    //Sort transforms in order that the parent always has an index lower than the son, for whatever tree structure
    //following Mirtich pge 114

    /*
    Transform[] SortTransforms(Transform root) {

        //NOTE: this will be a problem if there are transforms that are not articulationBody. I should rewrite this
        Transform[] ts = root.GetComponentsInChildren<Transform>();

        Transform[] sortedLinks = new Transform[ts.Length];


        NumberLinks(root, 1);


        return sortedLinks;

       
        int NumberLinks(Transform node, int idx)
        {

            sortedLinks[idx - 1] = node;
            idx += 1;
            //notice this is recursive
            foreach (Transform child in node)
                idx = NumberLinks(child, idx);

            return idx;


        }

    }

    */



    void ABAalgo() {

        //PASS 1 in Featherstone
        ComputeTreeLinkVelocities();
        InitTreeLinks();

        //PASS 2 and 3 in Featherstone
        TreeFwdDynamics();



    }




    //Mirtich pge 116
    void ComputeTreeLinkVelocities() {

        //TODO:
        //input is qVel for each articulated body
        float qVel = 0;



        //foreach (ArticulationBody ab in Links) {
        foreach (PDLink pdl in PDLinks)
        {
            ArticulationBody ab = pdl.ArticulationBody;
            Quaternion Rot = ab.transform.localRotation;

            ArticulationBody dad = ab.transform.parent.GetComponent<ArticulationBody>();
            Vector3 Rad = ab.centerOfMass - dad.centerOfMass;


            //w_i = Rot * w_h
            ab.angularVelocity = Rot * dad.angularVelocity;

            //v_i = Rot * w_i x r
            ab.velocity = Rot * dad.velocity + Vector3.Cross(ab.angularVelocity, Rad);


            Vector3 d_i = get_d(ab);
            Vector3 u_i = get_u(ab);

            ab.angularVelocity += qVel *u_i;
            ab.velocity += qVel * Vector3.Cross(u_i, d_i);
            

            

            
        }
    
    
    }


    //Mirtich page 120
    void InitTreeLinks() 
    {

        //input is qVel for each articulated body
        float qVel = 0;



        // for (int i = 0; i < Links.Length; i++) {
        for (int i = 0; i < PDLinks.Length; i++)
        {

            ArticulationBody ab = PDLinks[i].ArticulationBody;

            //we put the isolated zero acceleration force  Z_i (Mirtich) or Bias Force p_i^A (Featherstone14)
            double[] f_iX = {0.0, -1.0 * ab.mass * 9.81, 0.0 };

           //A. we calculate Z_i
            Vector3 tmp = ab.inertiaTensor;
            tmp.Scale(ab.angularVelocity);
            tmp = Vector3.Cross(ab.angularVelocity, tmp);
                
            double[] a = { 0.0, -1.0 * ab.mass * 9.81, 0.0 , tmp[0], tmp[1], tmp[2]};

            PDLinks[i].Z_i = Vector<double>.Build.Dense(a);

            //  Z_i[i] = Vector<double>.Build.Dense(a);

            //B. We calculate I_i
            Matrix<double> I_i = Matrix<double>.Build.Dense(6, 6);
            //this could be done only once
            I_i[0, 3] = ab.mass;
            I_i[1, 4] = ab.mass;
            I_i[2, 5] = ab.mass;

            //this has to be done in each frame:
            I_i[4, 0] = ab.inertiaTensor.x;
            I_i[5, 1] = ab.inertiaTensor.y;
            I_i[6, 2] = ab.inertiaTensor.z;

            PDLinks[i].I_i = I_i;

            //C. We calculate c_i
            Vector3 vel_i =qVel* get_u(ab);
            ArticulationBody dad = ab.transform.parent.GetComponent<ArticulationBody>();

            Vector3 d_i = get_d(ab);
            //Vector3 r_i = get_r(ab, dad);

            tmp = Vector3.Cross(dad.angularVelocity, vel_i);
            Vector3 tmp2 = Vector3.Cross(dad.angularVelocity, vel_i) 
                            + 2 * Vector3.Cross(dad.angularVelocity, Vector3.Cross(vel_i, d_i) )
                            + Vector3.Cross(vel_i, Vector3.Cross(vel_i, d_i) );

            double[] c = { tmp[0], tmp[1], tmp[2], tmp2[0], tmp2[1], tmp[2] };
            PDLinks[i].c_i = Vector<double>.Build.Dense(c);

        }


    }





    //Mirtich page 121
    void TreeFwdDynamics()
    {

        //pass 2
        for (int i = PDLinks.Length; i > 1; i--)
        {
            //A. we calculate I_i^A


            ArticulationBody ab = PDLinks[i].ab;
            ArticulationBody dad = ab.transform.parent.GetComponent<ArticulationBody>();

            Vector3 u=get_u(ab);
            Vector3 uxd =Vector3.Cross(u, get_d(ab));

            double[] a = { u[0], u[1], u[2], uxd[0], uxd[1], uxd[2] };
            Vector<double>  S_i = Vector<double>.Build.Dense(a);
            Matrix<double> S_iT = spatialTranspose(S_i);//it is a row matrix
            Matrix<double> dadXab = get_bXa( ab, dad);
            Matrix<double> abXdad = get_bXa(dad, ab);

            //to store it in the parent's I_h^A:




            //B. we calculate Z_i^A
            //Quaternion Rot = ab.transform.localRotation;





        }

        //pass 3


    }



}
