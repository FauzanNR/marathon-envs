

## Training tests 

These are some training tests to compare 

1. two controllers: MotionMatching and Mecanim
2. two training systems: and DReCon and MarCon



A.  Mecanim with DReCon (2021-04-09)



Trained with scene MarathonController/3.TrainUsingMarathon

Environment: **DReConHumanoid-v0**

instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210409-drecon-v2\Marathon.exe" --num-envs=25 --run-id=DReConHumanoid-v0-20210409-v4 --no-graphics --env-args --spawn-env=DReConHumanoid-v0 --num-spawn-envs=20

```
The commit that corresponds to the build is: 8bf9ade6c45a8c1acb766f13998d157d45460076

The outcome of the training is at: UnitySDK\Assets\MarathonController\TrainingTests\DReConHumanoid-v0-20210409-v4\DReCon-v0.onnx



B.  Mecanim with MarCon (2021-04-13)

Trained with scene MarathonController/3.TrainUsingMarathon

Environment: **MarConHumanoid-v0**

Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210413-marcon-mecanim\Marathon.exe" --num-envs=25 --run-id=MarConHumanoid-v0-20210413-v1 --no-graphics --env-args --spawn-env=MarConHumanoid-v0 --num-spawn-envs=20

```

The commit that corresponds to the build is: 5250b40ebe477ea85bed85071b395370705e3f0c

The outcome of the training is at: