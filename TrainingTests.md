

## Training tests 

These are some training tests to compare 

1. two controllers: MotionMatching and Mecanim
2. two training systems: and DReCon and MarCon

2021-04-09

**DReConHumanoid-v0**



trained with scene MarathonController/3.TrainUsingMarathon

instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210409-drecon-v2\Marathon.exe" --num-envs=25 --run-id=DReConHumanoid-v0-20210409-v4 --no-graphics --env-args --spawn-env=DReConHumanoid-v0 --num-spawn-envs=20

```
The commit that corresponds to the build is: 8bf9ade6c45a8c1acb766f13998d157d45460076






