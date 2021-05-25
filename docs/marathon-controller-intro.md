# Marathon Controller

The main folder contains three scenes which can be used to create a training environment from an animated character. 

**Available controllers:** There is also a called *AnimationControllers* that contains 3 examples of those:

- In *Walk-Run-Jump* there is an example of a humanoid performing different navigation, animated with Mecanim
- In *Procedural-from-FBX* there is an example of a humanoid animated extracting the animation from a video
- In *Quadruped* there is an example of a dog running around. The animations of this dog have been created by Valérie Juillard, adapting adapted from [mocap data provided by Sebastian Starke](https://github.com/sebastianstarke/AI4Animation#siggraph-2018mode-adaptive-neural-networks-for-quadruped-motion-controlhe-zhangsebastian-starketaku-komurajun-saitoacm-trans-graph-37-4-article-145joint-first-authors). 

![animated-dog](C:\Users\joanl\Documents\ARTANIM\repo-marathon-man-github\docs\images\animated-dog.gif)



As an extra test for the procedural generation, we have managed to train  with Motion Matching as a controller but, since we are using [a proprietary implementation](https://assetstore.unity.com/packages/tools/animation/motion-matching-for-unity-145624), it is not included here. Documentation on how to integrate this repository with the motion matching project will follow soon.



**Warning:** The environment generated has everything needed to start training, but it is not adjusted well enough to immediately get good results from the training. An aspect that is well defined from data available, and will work well, is  the use of the range of motion to determine the rotation constraints at each joint and the degrees of freedom of the overall motion. However, the size of the body colliders and the weight of each is not well adjusted when created procedurally, and the resulting prefab will need tweaking. 



## Next step:

Generating a new training environment, and training.