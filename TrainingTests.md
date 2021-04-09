

## Training tests 

These are some training tests to compare 

1. two controllers: MotionMatching and Mecanim
2. two training systems: and DReCon and MarCon

2021-04-09

**DReConHumanoid-v0**

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210409-drecon\Marathon.exe" --num-envs=25 --run-id=DReConHumanoid-v0-20210409 --no-graphics --env-args --spawn-env=MarConHumanoid-v0 --num-spawn-envs=20

```
The commit that corresponds to the build is: f9e52ec446b1986083e4d6411ca209ffdc870326








