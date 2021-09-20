using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class LinearPD : MonoBehaviour
{
   
    ArticulationBody[] Links;


    [SerializeField]
    ArticulationBody theRoot;


    //equation is:
    //f_I = I_i a_i + p_i


    //CORIOLIS AND OTHER FORCES
    Vector<double>[] Z_i;  //array of vectors  Vector<double>.Build.Dense(6);
                           //we use this variable to store 2 values:
                           //  1. isolated zero acceleration force  Z_i (Mirtich) or Bias Force p_i^A (Featherstone14)
                           //  2. spatial articulated zero-acceleration force  Z_i^A (Mirtich) or articulated Bias Force p_i^a (Featherstone14)
                           //TO CONFIRM IT IS EQUIVALENT: when put together in a big matrix, we write C(q, q^dot) 


    //Spatial Coriolis Force:
    Vector<double>[] c_i;  //array of vectors  Vector<double>.Build.Dense(6);



    //INERTIA or generalized Mass
    Matrix<double>[] I_i;       //array of matrices  Matrix<double>.Build.Dense(6, 6);
                                //we use this variable to store 2 values:
                                //  1. spatial isolated inertia I_i (Mirtich) or rigid-body inertia I_i (Featherstone14)
                                //  2. spatial articulated inertia I_i^A (Mirtich) or rigid-body apparent  inertia I_i^a  (Featherstone14)
                                //TO CONFIRM IT IS EQUIVALENT: when put together in a big matrix, we often write M(q)



    Vector3 get_u(ArticulationBody ab)
    {
        Vector3 u_i = new Vector3();
        ab.transform.rotation.ToAngleAxis(out _, out u_i);
        return u_i;

    }


    Vector3 get_d(ArticulationBody ab)
    {
        return (ab.centerOfMass - ab.anchorPosition);

    }

    Vector3 get_r(ArticulationBody ab, ArticulationBody dad) 
    {
        return (ab.centerOfMass - dad.centerOfMass );


    }


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


    }


    //Sort transforms in order that the parent always has an index lower than the son, for whatever tree structure
    //following Mirtich pge 114
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



    void ABAalgo() {

        //PASS 1 in Featherstone
        ComputeTreeLinkVelocities();
        InitTreeLinks();

        //PASS 2 and 3 in Featherstone
        TreeFwdDynamics();



    }




    //Mirtich pge 116
    void ComputeTreeLinkVelocities() {


        //input is qVel for each articulated body
        float qVel = 0;

    

        foreach (ArticulationBody ab in Links) {

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



        for (int j = 0; j < Links.Length; j++) {
            ArticulationBody ab = Links[j];

            //we put the isolated zero acceleration force  Z_i (Mirtich) or Bias Force p_i^A (Featherstone14)
            double[] f_iX = {0.0, -1.0 * ab.mass * 9.81, 0.0 };

           //A. we calculate Z_i
            Vector3 tmp = ab.inertiaTensor;
            tmp.Scale(ab.angularVelocity);
            tmp = Vector3.Cross(ab.angularVelocity, tmp);
                
            double[] a = { 0.0, -1.0 * ab.mass * 9.81, 0.0 , tmp[0], tmp[1], tmp[2]};

            Z_i[j] = Vector<double>.Build.Dense(a);

            //B. We calculate I_i
            //this could be done only once
            I_i[j][0, 3] = ab.mass;
            I_i[j][1, 4] = ab.mass;
            I_i[j][2, 5] = ab.mass;

            //this has to be done in each frame:
            I_i[j][4, 0] = ab.inertiaTensor.x;
            I_i[j][5, 1] = ab.inertiaTensor.y;
            I_i[j][6, 2] = ab.inertiaTensor.z;

            //We calculate c_i
            Vector3 vel_i =qVel* get_u(ab);
            ArticulationBody dad = ab.transform.parent.GetComponent<ArticulationBody>();

            Vector3 d_i = get_d(ab);
            Vector3 r_i = get_r(ab, dad);

            tmp = Vector3.Cross(dad.angularVelocity, vel_i);
            Vector3 tmp2 = Vector3.Cross(dad.angularVelocity, vel_i) 
                            + 2 * Vector3.Cross(dad.angularVelocity, Vector3.Cross(vel_i, d_i) )
                            + Vector3.Cross(vel_i, Vector3.Cross(vel_i, d_i) );

            double[] c = { tmp[0], tmp[1], tmp[2], tmp2[0], tmp2[1], tmp[2] };
            c_i[j] = Vector<double>.Build.Dense(c);

        }


    }

    void TreeFwdDynamics() 
    { 
    
        // TODO 
        //pass 2
        
        
        
        //pass 3
    }

    
    
   

}
