using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using LinearPDController;

public class LinearPD : MonoBehaviour
{





   // ArticulationBody[] Links;


  //  [SerializeField]
  //  ArticulationBody theRoot;


    PDLink[] PDLinks;

   



    void Start()
    {
       // Init(); 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
      //  ABAalgo();

    }


    public void Init(ArticulationBody theRoot)
    {
        if (theRoot == null | !theRoot.isRoot)
        {
            Debug.LogError("LinearPD does not know who is the root of the articulationBody Hierarchy");
        }



        PDLinks = PDLink.SortLinks(theRoot);


    }



    //NOT USED HERE
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
            pdl.updateVelocities(qVel);
            
        }
    
    
    }


    //Mirtich page 120
    void InitTreeLinks() 
    {

        


        for (int i = 0; i < PDLinks.Length; i++)
        {
            //TODO: input is qVel for each articulated body. Introduce this value.
            float qVel = 0;

            //we put the isolated zero acceleration force  Z_i (Mirtich) or Bias Force p_i^A (Featherstone14)

            //A. we calculate Z_i
            PDLinks[i].calculateZ_i();


            //B. We calculate I_i
            PDLinks[i].calculateI_i();

            //C. We calculate c_i
            PDLinks[i].updateCoriolis(qVel);


        }


    }




   

    //Mirtich page 121
    void TreeFwdDynamics()
    {
        //pass 2
        for (int i = PDLinks.Length; i > 1; i--)
        {
            //TODO: update and find the target forces to apply            
            Vector3 Qforces = Vector3.zero;
            Vector3 Qtorques = Vector3.zero;

            PDLinks[i].updateIZ_dad(Qforces,Qtorques);


        }

        //pass 3
        for (int i = 0; i < PDLinks.Length; i++) 
        {

            Vector3 Qforces = Vector3.zero;
            Vector3 Qtorques = Vector3.zero;
            PDLinks[i].updateAcceleration(Qforces, Qtorques);
        
        
        }

    }



}
