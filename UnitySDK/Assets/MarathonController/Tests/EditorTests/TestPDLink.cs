using System.Collections;
using System.Collections.Generic;
//using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using Unity.Mathematics;


public class TestPDLink
{

    public bool compareAtStart;

  
    public
    ArticulationBody ab;

    float KP = 500;

    [UnityTest]
    public IEnumerator ComparePDLinkImplementations()
    {
        GameObject hierarchy = (GameObject)Resources.Load("Prefabs/ArticulationBody2TestPDLink");

        GameObject.Instantiate(hierarchy, Vector3.zero, Quaternion.identity);
        ab = GameObject.Find("articulation:mixamorig:Spine").GetComponent<ArticulationBody>();


     
        CompareQi();
        CompareVels();
     
        CompareI();


        yield return null;
    }


    void InitPDLinks()
    {

        Debug.LogError("TODO: need to finish the interface IKinematic ArticulationBody inside TestPDLink");
/*        pdls = PDLinkStruct.InitPDLinkStruct(ab);
        pdls.setGain(KP, Time.fixedDeltaTime);


        List<PDLinkClass> LinksCreated = new List<PDLinkClass>();
        pdlc = PDLinkClass.CreatePDLink(ab, LinksCreated);
        pdlc.setGain(KP, Time.fixedDeltaTime);


        List<PDLinkPureClass> LinksCreated2 = new List<PDLinkPureClass>();
        pdlp = PDLinkPureClass.CreatePDLink(ab, LinksCreated2);
        pdlp.setGain(KP, Time.fixedDeltaTime);
*/

    }

    void CompareQi()
    {
        Vector3 targetRots = new Vector3(1, 2, 3);

        Debug.LogError("TODO: need to finish the interface IKinematic ArticulationBody");
        /*
        pdls.UpdatePDLinkStruct(ab);
        pdls.updateQi(targetRots);

        pdlc.updateQi(targetRots);
        pdlp.updateQi(targetRots);

        Assert.AreEqual(pdls.Qi, pdlc.Qi, "Qi are not equal");
        Assert.AreEqual(pdls.Qi, pdlp.Qi, "Qi are not equal");
        */



    }

    void CompareVels()
    {
      


    }

  


    void CompareI()
    {
      /*  pdls.calculateI_i();

        pdlc.calculateI_i();
        pdlp.calculateI_i();

        Checks4SpatialAlgebra.CheckIfEqual(pdls.I_i, pdlc.I_i);
        Checks4SpatialAlgebra.CheckIfEqual(pdls.I_i, pdlp.I_i);
      */

    }





}