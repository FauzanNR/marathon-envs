

## Training tests 

These are some training tests to compare 

1. two controllers: MotionMatching and Mecanim
2. two training systems: and DReCon and MarCon



A.  Mecanim with DReCon (2021-04-15)



Trained with scene MarathonController/3.TrainUsingMarathon

Environment: **DReConHumanoid-v0**

instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210413-marcon-mecanim\Marathon.exe" --num-envs=25 --run-id=DReConHumanoid-v0-20210415-v2 --no-graphics --env-args --spawn-env=DReConHumanoid-v0 --num-spawn-envs=20

```

The commit that corresponds to the build is:  5250b40ebe477ea85bed85071b395370705e3f0c

**TODO** UPDATE



B.  Mecanim with MarCon (2021-04-14)

Trained with scene MarathonController/3.TrainUsingMarathon

Environment: **MarConHumanoid-v0**

Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210413-marcon-mecanim\Marathon.exe" --num-envs=25 --run-id=MarConHumanoid-v0-20210413-v2 --no-graphics --env-args --spawn-env=MarConHumanoid-v0 --num-spawn-envs=20

```

The commit that corresponds to the build is: 5250b40ebe477ea85bed85071b395370705e3f0c

The outcome of the training is at:

TrainingTests/MarConHumanoid-v0-20210413-v2/MarCon-v0.onnx



**TODO**



C.  MotionMatching with DReCon 

**TODO**



D.  MotionMatching with MarcCon (2021-04-15)

Running test with reward term focused on minimizing energy



Trained with sceneMotionMatching/mxm4marathon/Scenes/3.TrainUsingMarathon

Environment: **MotionMarCon-v0**

Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210415-marcon-motionmatch-energymin\Marathon.exe" --num-envs=25 --run-id=MotionMarCon-v0-20210415-v1 --no-graphics --env-args --spawn-env=MotionMarCon-v0 --num-spawn-envs=20

```

The commit that corresponds to the build is: 7f20a59a10b8a581e4670bc23e1a5ea77064d52f





