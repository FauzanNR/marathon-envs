using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using LinearPDController;

public class LinearPD : MonoBehaviour
{


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


    public List<ArticulationBody> Init(ArticulationBody theRoot)
    {
        if (theRoot == null | !theRoot.isRoot)
        {
            Debug.LogError("LinearPD does not know who is the root of the articulationBody Hierarchy");
        }



        PDLinks = PDLink.SortLinks(theRoot);


        //to make sure the list of motors is in the same order than the list of links we do:
        List<ArticulationBody> _motors = new List<PDLink>(PDLinks).Select(x => x.articulationBody).ToList();
        return _motors;


    }


    public void updateTargets(Vector3[] targetRots, float actionTimeDelta)
    { 
                

    
    
    }



    //NOT USED HERE
    public void ABAalgo() {

        //PASS 1 in Featherstone
        ComputeTreeLinkVelocities();

       
        InitTreeLinks();

        //PASS 2 and 3 in Featherstone
        TreeFwdDynamics();



    }




    //Mirtich pge 116
    void ComputeTreeLinkVelocities() {

     

        //note: this is probably already done by the algo itself
        foreach (PDLink pdl in PDLinks)
        {           
           

                   pdl.updateVelocities(pdl.currentVel());

            
        }
    
    
    }


    //Mirtich page 120
    void InitTreeLinks() 
    {

        


        for (int i = 0; i < PDLinks.Length; i++)
        {
          

            //we put the isolated zero acceleration force  Z_i (Mirtich) or Bias Force p_i^A (Featherstone14)

            //A. we calculate Z_i
            PDLinks[i].calculateZ_i();


            //B. We calculate I_i
            PDLinks[i].calculateI_i();

            //C. We calculate c_i
            PDLinks[i].updateCoriolis();


        }


    }




   

    //Mirtich page 121
    void TreeFwdDynamics()
    {
        //pass 2
        for (int i = PDLinks.Length-1; i > 0; i--)
        {
            //TODO: update and find the target forces to apply            
            Vector3 Qforces = Vector3.zero;
            Vector3 Qtorques = Vector3.zero;

            Debug.Log(" linking node "   + " to index " + i);

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
