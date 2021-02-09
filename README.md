# README #

## 1. Introduction

I have used this Repository to integrate the use of physics-based characters, as found in the project *Marathon Environments*, with usual character animation techniques, based on  skinned characters. This project is a fork of *Marathon Environments*, and the Readme of the initial project can be found [here](README-marathon-environments.md). Joe Booth is the main developer of   *Marathon Environment*, and has also helped substantially in this project. Valérie Juillard, a colleague from the [Artanim Foundation](www.artanim.ch) has provided some of the animations.

**Problem:** The main problem is to aovid getting results that look like  if they came with this *department of silly walks* effect, which obviously does not appear in the demos of the papers. It is annoying, and we absolutely need to solve it if we want to have something that can be used in practice... 

![Weird Walks](img/marathon-man-silly-walks.gif)



This document has the following sections:

1. **Introduction**
2. **Install and troubleshooting**
3. **Export to an existing animation project**



Document last updated: 09.02.2021



### 1.1 Project Organization

The Unity project has two parts. Both can be find in *UnitySDK > Assets*: 

- In folder *MarathonEnvs* there are several benchmarks of physics-based animation, implemented on the basis of different papers in the field that have appeared in the last 5-10 years.
- In folder *ProceduralGeneration* there are 3 scenes that can be used to generate a training environment from an existing interactive controller. There is also a sub-folder called *AnimationControllers* that contains 3 examples of those:
  - In *Walk-Run-Jump* there is an example of a humanoid performing different navigation, animated with Mecanim
  - In *Procedural-from-FBX* there is an example of a humanoid animated extracting the animation from a video
  - In *Quadruped* there is an example of a dog running around. The animations of this dog have been created by Valérie Juillard, adapting adapted from [mocap data provided by Sebastian Starke](https://github.com/sebastianstarke/AI4Animation#siggraph-2018mode-adaptive-neural-networks-for-quadruped-motion-controlhe-zhangsebastian-starketaku-komurajun-saitoacm-trans-graph-37-4-article-145joint-first-authors). As stated in their [README](https://github.com/sebastianstarke/AI4Animation), *it is not available for commercial use or redistribution. The intellectual  property for different scientific contributions belongs to the  University of Edinburgh, Adobe Systems and Electronic Arts. Licensing is possible if you want to use the code for commercial use. For scientific use, please reference this repository together with the relevant  publications below. The motion capture data is available only under the terms of the [Attribution-NonCommercial 4.0 International](https://creativecommons.org/licenses/by-nc/4.0/legalcode) (CC BY-NC 4.0) license.*

![animated-dog](images/animated-dog.gif)



As an extra test for the procedural generation, we are playing with Motion Matching as a controller but, since we are using [a proprietary implementation](https://assetstore.unity.com/packages/tools/animation/motion-matching-for-unity-145624), it is not included here. Documentation on how to integrate this repository with the motion matching project will follow soon.



### 1.2 Licensing

The contributions of Dr Joan Llobera are not available for commercial use or redistribution. The intellectual  property for different scientific contributions belongs to the [Artanim Foundation](www.artanim.ch). Licensing is possible if you want to use the code for commercial use. The motion capture data is available only under the terms of the  [Attribution-NonCommercial 4.0 International](https://creativecommons.org/licenses/by-nc/4.0/legalcode) (CC BY-NC 4.0) license.





### 1.3 Current Status

#### 1 Behaviour synthesized

The main goal has *almost*  been achieved: skinned characters are matched with ragdolls.


![Example-current-status](img/trained-with-automatic-constraint-parsing.gif)


### 1.4 Active branches

- *master* contains a stable version
- *develop* contains the main current development




#### 2 Pending tasks:

**Constraints**

- Clarify why in the articulation constraints:
  - The first animation is not acceptable. Unclear why would be the case (T-pose switches abruptly?) 

**Rewards**

- Change reward system to make position matching more important

**Simplify use and make animation pipeline more robust**


**Training**

- Training of the procedural must be validated with all the controllers

#### 3 Current limitations (out from the scope of this repo)

- Running several environments together does not work. Indeed, if I try to run it with the previous version, in parallel, it complains of socket conflicts.





## 2. Installation

### 2.1 Installation with conda environment file

1. Make sure you have the latest graphics drivers updated. Restart the computer.
2. Install anaconda. If it is already installed, make sure you have the latest version by running, with admin privileges, `conda update anaconda`. Restart the computer.
3. Make sure you have the latest version of pip by running, with admin privileges, `python -m pip install --upgrade pip`
4. In the conda command prompt, go to the home of this repo
5. Run `conda env create -f environment.yml `



If you update from a previous ml-agents version, you can use `conda env update -f environment.yml `. Remember to restart the computer after that.





### 2.2 Common mistakes

On windows,  missing packages. If you have an error similar to:



`Error loading site-packages\torch\lib\caffe2_detectron_ops_gpu.dll or one of its dependencies.`

Then the fix seems to be:

https://github.com/pytorch/pytorch/issues/35803

If the error is:

`Error loading "c:\users\joanl\.conda\envs\marathon-envs\lib\site-packages\torch\lib\cudnn_adv_infer64_8.dll" or one of its dependencies.`

Then, you may install the latest C++ redistributable



https://support.microsoft.com/en-ca/help/2977003/the-latest-supported-visual-c-downloads 



If you still have trouble using the previous, you may have memory limits. Try setting up the training with less environments.



## 3. Training




### 3.1 How to train an existing environment

You can either make a binary containing only the *MarathonEnvs.unity* scene and run the instructions below, or do a quick training test in editor.

macOS format:

```
mlagents-learn config/marathon_envs_config.yaml --env="envs/MarathonEnvs" --num-envs=10 --run-id=Hopper-001 --no-graphics --env-args --spawn-env=Hopper-v0 --num-spawn-envs=50
```

Windows format:

```
mlagents-learn config\marathon_envs_config.yaml --env="envs\MarathonEnvs\Marathon Environments.exe" --num-envs=25 --run-id=newMlAgents-007 --no-graphics --env-args --spawn-env=ControllerMarathonMan-v1 --num-spawn-envs=35
```

Example to test quickly in editor:

```
mlagents-learn config\marathon_envs_config.yaml --run-id=test-2020-01-06-v2 --env-args --spawn-env=ControllerMarathonMan-v1 
```




### 3.2 How to generate a new training environment

To simplify the creation of a training environment, you can do it directly from a character with an animation controller associated to it.

To do so:

1. Open the scene found in `Assets > ProceduralGeneration > 2.GenerateTrainingEnvFromAnimatedCharacter.unity`

2. Add the animated character that you prefer

   It needs to be a character that moves, either interactively, either as a long sequence. For results to work reasonably, there needs to be some variety of movements, otherwise the range of motion will be too narrow, and the trianing will give poor results.

3. Select the gameObject *CHECKME-generate-training-env*. That gameObject has  the component *Training Environment Generator*. You should drag in the first 3 fields  the character that you want to use, as well as it's head, and it's root. Below you can find an example with a character consisting of a very long animation:

![proced-config](img/proced-config.png)

4. At the bottom of the *Training Environment Generator* you can find the 3 steps that you need to go through:

![procedural-generation-buttons](img/procedural-generation-buttons.png)

Once you have gone through these 3 steps, you will have an environment generated. It will be in *Assets > MarathonEnvs > Environments* and, if it the first one, will be called *TrainingEnvironment*.

![Captura de Pantalla 2021-01-26 a les 16.26.16](img/training-environment-generated.png)

If one already exists, it will store it with a name like *TrainingEnvironment 1*, and add indices (2,3,...).

5. Open the scene *Assets > MarathonEnvs > Scenes > MarathonEnvs*. When you select the gameObject *WorldFactory*, you will see a list of environments. Click on Add New, give it a name (in the example below I called it *ControllerProcedural-v0*), and add the Procedural Environment that was stored. If you do so, it should look like below

![adjust-environment-in-list](img/adjust-environment-in-list.png)

6. You can now train using the usual procedure, like below:

In **editor** (useful to check everything goes well):

```
mlagents-learn config/marathon_envs_config.yaml --run-id=Procedural-v1 --env-args --spawn-env=ControllerProcedural-v0
```

With a **binary** file:

- Compile the project only with the scene MarathonEnvs

![Captura de Pantalla 2021-01-26 a les 16.35.59](img/build-settings.png)

Once compiled, simply launch the *marathonenvs* training environment, and launch the training as explained above, making sure that the `--spawn-env`variable is targeting the name that you associated with your procedural environment.






## 3. Export to an existing animation project

### 3.1 How to integrated  marathon envs in an existing project

1. Open the file Packages/manifest.json in the target project, and add to the existing package dependencies the following:

```json
    "com.joebooth.many-worlds": "https://github.com/sohojoe/many-worlds.git?path=/com.joebooth.many-worlds",
	"com.unity.ml-agents": "1.0.6",
	"com.unity.barracuda": "0.6.1-preview",
```



2. Take the entire *marathon-envs* folder and add it to your project
3. Adjust the Layers and how they connect physically (see image below)

![Layers2add](img/2.1.layers2add_highlight.png)
	
![Physics Set up](img/2.2.physics2set-up.png)



4. Adjust the Physics collisions

![Physics](img/3.physics-settings-fix-highlight.png)



**IN PROGRESS, UNFINISHED**



### 3.2 How to integrated the marathon man scene in an existing project

A package called *export-controller-man-v2* has been generated by exporting elements from the marathon repo. This is old, but the same principle would appy with the latest project.

There is a zipped folder called Project Settings.

Then, the recipe is:

1. Copy folder com.unity.mlagents 

2. Create empty project (call it import-marathon) folder 

3. In Packages > manifest.json do add the line:

   "com.unity.ml-agents": "file:../../com.unity.ml-agents",

   If you do this it will work, but you will get something similar to:
   ![Marathon-crash](img/marathon-man-crashing.gif)

   

4. Adjust the Layers and how they connect physically (see image below)

![Layers2add](img/2.1.layers2add_highlight.png)
	
![Physics Set up](img/2.2.physics2set-up.png)



5. Adjust the Physics collisions

![Physics](img/3.physics-settings-fix-highlight.png)


6. Open the project and Import Package *export-controller-man-v2*

The folder *import-marathon-with-settings* contains an example of how to do this


7. You will need to:

   1.  initialize the bounds of SpawnableEnv
   2. Change the way the floor is detected (in case you want to use a normal floor), 
      the main thing to do is to change the collisions to detect a layer, instead of detect a component. 
      See branch  *feature/add-physics-based-char* inside artanim_npcai

### 3.3 How to get controllerMarathon to work with an existing Character animation library

**Note:** this section is now (01.12.2020) slightly outdated, we are using **MarathonMan004** as a basis.

To understand how the training works in the Marathon Environment, the main components of the Marathon Man setup that you need to understand are the following:

If you open the MarathonMan main 

 1. *Ragdoll002* This class is set up with minimal elements in the prefab **MarathonMan003Target**.  
	
 2. other prefabs like **MarathonManTarget** are built on top of it. This last prefab also includes:

  - Animator
  - Character Controller
  - Mocap Animator Controller
  - Mocap Controller
  - Track Body States in Word Space


 3. A strategy that has worked: to build from an existing character animation (Mecanim or other), by adding on top of it the scripts from MarathonManTarget as listed:

  - Animator. *NO*, I do not need it, it already has one
  - Character Controller. *No*, this sums up to have 2 different controllers on the character  
  - Mocap Animator Controller. *No*, this deals with the animator, we are already doing this 
  - Mocap Controller. *YES*, this maps the movements to the rigidbodies
  - Track Body States in Word Space. *OPTIONAL* the movement seems to work fine, it only seems to generate some stats and data


  The script MocapController that it is on AgentMoveToMM seems to find the right movements to mimic in this setup.

  

### 3.4 How to get  pathfinding to work with training

The training system instantiates the environment dynamically in order to be able to train in parallel. However, the path finding needs to be a unique game object.  To circumvent this, this solution has worked for me:

 - I create an environment that already contains the floor. 
 - I deactivate the ground in the  Environment prefab, and find it through the name of the ground (a reference to the gameobject cannot work since the floor already exists, and the environment is generated dynamically afterwards).
 - In addition, to prevent problems related to path finding, I activate the agents *only* when the environment is instantiated.













