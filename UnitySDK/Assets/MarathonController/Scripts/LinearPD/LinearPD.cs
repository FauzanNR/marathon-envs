using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using LinearPDController;

public class LinearPD : MonoBehaviour 
//TODO: we only define it as a MonoBehavior to show debug values, it could easily be a pure class
{


    PDLink[] PDLinks;


    /*
    public enum Mode { 
        spherical,
        revolute
    }
    */



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
        for (int i = 0; i < PDLinks.Length; i++)
        {


            PDLinks[i].updateQtorque(targetRots[i], actionTimeDelta);


        }


    }



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
          


            //Debug.Log(" linking node "   + " to index " + i);
            

            PDLinks[i].updateIZ_dad();


        }

        //pass 3
        for (int i = 0; i < PDLinks.Length; i++) 
        {
            PDLinks[i].updateAcceleration();
        
        }

    }



}
