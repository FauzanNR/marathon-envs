using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;

public class AdversarialTerrainAgent : Agent {

    Terrain terrain;
	int lastSteps;
	Agent _agent;


	public int posXInTerrain;
	public int posYInTerrain;
	float[,] _heights;
	float[,] _rowHeight;

	public int heightIndex;
	public float curHeight;
	public float actionReward;

	internal const float _maxHeight = 10f;
	const float _midHeight = 5f;
	float _mapScaleY;

	public override void AgentReset()
	{
		// get start position
		if (this.terrain == null)
		{
			var parent = gameObject.transform.parent;
			terrain = parent.GetComponentInChildren<Terrain>();
			var sharedTerrainData = terrain.terrainData;
			terrain.terrainData = new TerrainData();
			terrain.terrainData.heightmapResolution = sharedTerrainData.heightmapResolution;
			terrain.terrainData.baseMapResolution = sharedTerrainData.baseMapResolution;
			terrain.terrainData.SetDetailResolution(sharedTerrainData.detailResolution, sharedTerrainData.detailResolutionPerPatch);
			terrain.terrainData.size = sharedTerrainData.size;
			terrain.terrainData.thickness = sharedTerrainData.thickness;
			terrain.terrainData.splatPrototypes = sharedTerrainData.splatPrototypes;
			terrain.terrainData.terrainLayers = sharedTerrainData.terrainLayers;
			var collider = terrain.GetComponent<TerrainCollider>();
			collider.terrainData = terrain.terrainData;
			_rowHeight = new float[terrain.terrainData.heightmapResolution,1];
		}
		if (this._agent == null)
			_agent = GetComponent<Agent>();
        //print($"HeightMap {this.terrain.terrainData.heightmapWidth}, {this.terrain.terrainData.heightmapHeight}. Scale {this.terrain.terrainData.heightmapScale}. Resolution {this.terrain.terrainData.heightmapResolution}");
        _mapScaleY = this.terrain.terrainData.heightmapScale.y;
		// get the normalized position of this game object relative to the terrain
        Vector3 tempCoord = (transform.position - terrain.gameObject.transform.position);
        Vector3 coord;
		tempCoord.z = Mathf.Clamp(tempCoord.z,0f, terrain.terrainData.size.z-0.000001f);
        coord.x = (tempCoord.x-1) / terrain.terrainData.size.x;
        coord.y = tempCoord.y / terrain.terrainData.size.y;
        coord.z = tempCoord.z / terrain.terrainData.size.z;
        // get the position of the terrain heightmap where this game object is
        posXInTerrain = (int) (coord.x * terrain.terrainData.heightmapWidth); 
        posYInTerrain = (int) (coord.z * terrain.terrainData.heightmapHeight);
        // we set an offset so that all the raising terrain is under this game object
        int offset = 0 / 2;
        // get the heights of the terrain under this game object
        _heights = terrain.terrainData.GetHeights(posXInTerrain-offset,posYInTerrain-offset, 100,1);
		curHeight = _midHeight;
		heightIndex = posXInTerrain;
		actionReward = 0f;

        
        // set the new height
        // terrain.terrainData.SetHeights(_posXInTerrain-offset,_posYInTerrain-offset,_heights);
        //_heights[0,0] = 0.1f/600f;
        //this.terrain.terrainData.SetHeights(_posXInTerrain, _posYInTerrain, _heights);
		ResetHeights();
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);

		lastSteps = 0;
		//RequestDecision();
	}

	void ResetHeights()
	{
		float height = curHeight / _mapScaleY;
		for (int i = 0; i < terrain.terrainData.heightmapHeight; i++)
			_rowHeight[i,0] = height;
		for (int i = 0; i < terrain.terrainData.heightmapHeight; i++)
			this.terrain.terrainData.SetHeights(i, 0, _rowHeight);
	}

	void SetNextHeight(int action)
	{
		float actionSize = 0f;
		bool actionPos = (action-1) % 2 == 0;
		if (action != 0)
		{
			actionSize = ((float)((action+1)/2)) * 0.1f;
			curHeight += actionPos ? actionSize : -actionSize;
			if (curHeight < 0) {
				curHeight = 0f;
				actionSize = 0;
				AddReward(-1f);
			}
			if (curHeight > _maxHeight)
			{
				curHeight = _maxHeight;
				actionSize = 0;
				AddReward(-1f);				
			}
		}

		float height = curHeight / _mapScaleY;
		for (int i = 0; i < terrain.terrainData.heightmapHeight; i++)
			_rowHeight[i,0] = height;
		this.terrain.terrainData.SetHeights(heightIndex, 0, _rowHeight);

		heightIndex++;
		actionReward = actionSize;
	}
	internal void OnNextMeter()
	{
		// AddReward(1);
		// AddReward(_actionReward);
		actionReward = 0f;
		RequestDecision();
	}
	internal void Terminate(float cumulativeReward)
	{
		if (this.IsDone())
			return;
		var maxReward = 1000f;
		var agentReward = cumulativeReward;
		agentReward = Mathf.Clamp(agentReward, 0f, maxReward);
		var adverseralReward = maxReward - agentReward;
		AddReward(adverseralReward);
		Done();
		// RequestDecision();
	}

	public override void CollectObservations()
	{
		var height = curHeight / _maxHeight;
		// add last agent distance
		
		int curSteps = 0;
		if (_agent != null)
			curSteps = _agent.GetStepCount();
		var numberSinceLast = curSteps - lastSteps;
		numberSinceLast = 1 - (numberSinceLast/1000);
		lastSteps = curSteps;
        AddVectorObs(numberSinceLast);
        AddVectorObs(height);
        AddVectorObs(actionReward);
	}
	public override void AgentAction(float[] vectorAction, string textAction)
	{
		// each action is a descreate for height change
		int action = (int)vectorAction[0];
		SetNextHeight(action);
	}

	public (List<float>, float) GetDistances2d(Vector3 pos, bool showDebug)
	{
		int layerMask = ~(1 << 14);
        var xpos = pos.x;
        xpos -= 2f;
        float fraction = (xpos - (Mathf.Floor(xpos*5)/5)) * 5;
        float ypos = pos.y;
        List<Ray> rays = Enumerable.Range(0, 5*5).Select(x => new Ray(new Vector3(xpos+(x*.2f), AdversarialTerrainAgent._maxHeight, 0f), Vector3.down)).ToList();
        List<float> distances = rays.Select
            ( x=>
                ypos - (AdversarialTerrainAgent._maxHeight - 
                Physics.RaycastAll(x,_maxHeight,layerMask)
                .OrderBy(y=>y.distance)
                .FirstOrDefault()
                .distance)
            ).ToList();
        if (Application.isEditor && showDebug)
        {
            var view = distances.Skip(10).Take(20).Select(x=>x).ToList();
            Monitor.Log("distances", view.ToArray());
            var time = Time.deltaTime;
            time *= agentParameters.numberOfActionsBetweenDecisions;
            for (int i = 0; i < rays.Count; i++)
            {
                var distance = distances[i];
                var origin = new Vector3(rays[i].origin.x, ypos,0f);
                var direction = distance > 0 ? Vector3.down : Vector3.up;
                var color = distance > 0 ? Color.yellow : Color.red;
                Debug.DrawRay(origin, direction*Mathf.Abs(distance), color, time, false);
            }
        }
		List<float> normalizedDistances = distances
			.Select(x => Mathf.Clamp(x, -10f, 10f))
			.Select(x => x/10f)
			.ToList();
		;
		return (normalizedDistances, fraction); 
	}
}
