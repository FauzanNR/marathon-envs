

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





2021-04-30

Updated with improved velocity calculation.

instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210430-drecon-mecanim-with-targetedVelocity-v2\Marathon.exe" --num-envs=25 --run-id=20210430-drecon-mecanim-with-targetedVelocity-v22 --no-graphics --env-args --spawn-env=DReConHumanoid-v0 --num-spawn-envs=20

```

The commit that corresponds to this result is: 5451b2af620cbd938f609bdac722a33db1816322 (4th may)









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

The outcome of the training is at:

\TrainingTests\MotionMarCon-v0-20210415-v1MotionMarCon-v0.onnx





#### TRAIN DreConController

2021 05 26 

Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210525-DreConController2Train\Marathon.exe" --num-envs=25 --run-id=DreConController-v0-20210526-v0 --no-graphics --env-args --spawn-env=DReConHumanoid-v0 --num-spawn-envs=20

```

The commit that corresponds to the build is: 5d39ec4e41d0c38066cb6f92e9641815ff15e0cb

The outcome of the training is at:





2021 05 28









Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210528-DReConController2Train-Centered\Marathon.exe" --num-envs=25 --run-id=DreConController-centered-v0-20210528-v0 --no-graphics --env-args --spawn-env=DReConHumanoid-v0 --num-spawn-envs=20

```

The commit that corresponds to the build is: eb2e3696aa30029a20e2267478090ae478da0a6e

The outcome of the training is at:



WRONG

environment should be centered.



2021 06 11

# StablePD



Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210611-Stable\Marathon.exe" --num-envs=25 --run-id=Stable-v2 --no-graphics --env-args --spawn-env=DReConHumanoid-v0 --num-spawn-envs=20

```

The commit



```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210611-Stable2\Marathon.exe" --num-envs=25 --run-id=Stable-v4 --no-graphics   --env-args --spawn-env=DReConHumanoid-v0  --num-spawn-envs=20

```





corrected the degrees to radians, and testing with addRelativeTorque, instead of imposing the jointForces

```
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210616-Stable4\Marathon.exe" --num-envs=25 --run-id=Stable-v4v3 --no-graphics   --env-args --spawn-env=DReConHumanoid-v0  --num-spawn-envs=20


```



b2:

b3:

localTorque, 

no actionsBetwween deciions



b3:

localTorque, 

actionsBetwween deciions



b4: 

Torque, 

actionsBetwween deciions

(training b4v1 is wrong, trained on b3)

b5:

localTorque,

actionsBetweenDecisions

nno conversion degrees to radiants (forces not in radians)

 



So far, stuck, it does not reallz work



# Improve procedural generation

24.06.2021



```
mlagents-learn config\samba.yaml --env="envs\20210624-PDtrainingOnlyProcedural\Marathon.exe" --num-envs=25 --run-id=20210624-PDtrainingOnlyProcedural-v3 --no-graphics   --env-args --spawn-env=proceduralFromAnimatedDancer  --num-spawn-envs=20


```



Errors:

when I run

```
mlagents-learn config\marathon_envs_config.yaml --run-id=20210624-PD-DReCon-OnlyProcedural-EDITOR10 --num-envs=1   --env-args --spawn-env=DReConHumanoid-v0-OK
```

with the default environmetn `DReConHumanoid-v0-OK`

it runs OK



BUT

when I run 

```
mlagents-learn config\marathon_envs_config.yaml --run-id=20210624-PD-DReCon-OnlyProcedural-EDITOR8 --num-envs=1   --env-args --spawn-env=DReConHumanoid-v0
```

I get:



```
RuntimeError: There were no tensor arguments to this function (e.g., you passed an empty list of Tensors), but no fallback function is registered for schema aten::_cat.  This usually means that this function requires a non-empty list of Tensors.  Available functions are [CPU, CUDA, QuantizedCPU, BackendSelect, Named, AutogradOther, AutogradCPU, AutogradCUDA, AutogradXLA, AutogradNestedTensor, UNKNOWN_TENSOR_TYPE_ID, AutogradPrivateUse1, AutogradPrivateUse2, AutogradPrivateUse3, Tracer, Autocast, Batched, VmapMode].

CPU: registered at aten\src\ATen\RegisterCPU.cpp:5925 [kernel]
CUDA: registered at aten\src\ATen\RegisterCUDA.cpp:7100 [kernel]
QuantizedCPU: registered at aten\src\ATen\RegisterQuantizedCPU.cpp:641 [kernel]
BackendSelect: fallthrough registered at ..\aten\src\ATen\core\BackendSelectFallbackKernel.cpp:3 [backend fallback]
Named: registered at ..\aten\src\ATen\core\NamedRegistrations.cpp:7 [backend fallback]
AutogradOther: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
AutogradCPU: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
AutogradCUDA: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
AutogradXLA: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
AutogradNestedTensor: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
UNKNOWN_TENSOR_TYPE_ID: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
AutogradPrivateUse1: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
AutogradPrivateUse2: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
AutogradPrivateUse3: registered at ..\torch\csrc\autograd\generated\VariableType_2.cpp:9122 [autograd kernel]
Tracer: registered at ..\torch\csrc\autograd\generated\TraceType_2.cpp:10525 [kernel]
Autocast: registered at ..\aten\src\ATen\autocast_mode.cpp:254 [kernel]
Batched: registered at ..\aten\src\ATen\BatchingRegistrations.cpp:1016 [backend fallback]
VmapMode: fallthrough registered at ..\aten\src\ATen\VmapModeRegistrations.cpp:33 [backend fallback]
```







# Check PD with open loop

24.06.2021

Implemented loop at the style of DReCon, with fixed muscle strength.

checked in 



with the samba character:

Training Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210624-v3-PDopenloop\Marathon.exe" --num-envs=25 --run-id=20210624-v3-PDopenloop-v0 --no-graphics   --env-args --spawn-env=DReConHumanoid-v0  --num-spawn-envs=20


```

Outcome:

```shell
 DReCon-v0. Step: 128000000. Time Elapsed: 101268.503 s. Mean Reward: 376.566. Std of Reward: 743.918. Training.
[INFO] Exported results\20210624-v3-PDopenloop-v1\DReCon-v0\DReCon-v0-128001293.onnx
[INFO] Copied results\20210624-v3-PDopenloop-v1\DReCon-v0\DReCon-v0-128001293.onnx to results\20210624-v3-PDopenloop-v1\DReCon-v0.onnx
```

.



30.06.2021

With a new environment, created only with the automated procedure, and using the old animation controller:

Training Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210630-Procedural-PD\Marathon.exe" --num-envs=25 --run-id=20210630-Procedural-PD-v0 --no-graphics   --env-args --spawn-env=DReConHumanoid-v0  --num-spawn-envs=20


```

commits:

in mxmmarathon, 8197f0ec15b0e7ab720a79d38232b4beaf7a3aac

in marathon-man 9229947771a074d4af5b7c05c964e487bcd1007f (the range of motion file is not commited)



Outcome:

```shell
 [INFO] DReCon-v0. Step: 128000000. Time Elapsed: 102088.187 s. Mean Reward: 348.634. Std of Reward: 674.604. Training.
[INFO] Exported results\20210630-Procedural-PD-v0\DReCon-v0\DReCon-v0-128000824.onnx
[INFO] Copied results\20210630-Procedural-PD-v0\DReCon-v0\DReCon-v0-128000824.onnx to results\20210630-Procedural-PD-v0\DReCon-v0.onnx.
```

.

06.07.2021

the mistake was in the debug mode, that was on.

With a new environment, created only with the automated procedure, and using the samba character with the old PD controller:

Training Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210706-Procedural-PD-samba-v2\Marathon.exe" --num-envs=25 --run-id=20210706-Procedural-PD-vSamba22 --no-graphics   --env-args --spawn-env=DReConHumanoid-v0  --num-spawn-envs=20


```

this fails due to a problem with tensors not being received



06.07.2021

I try again the openloop, since it was mixed wit a debug thing.

using the non-procedural agent.

Training Instruction:

```shell

 mlagents-learn config\marathon_envs_config.yaml --env="envs\20210706-PDopenloop\Marathon.exe" --num-envs=25 --run-id=20210706-PDopenloop --no-graphics   --env-args --spawn-env=DReConHumanoid-v0  --num-spawn-envs=20
```

**this seems to work well**





06.07.2021

AN EXTRA CHECK For the procedural generated:

Environment:

20210706-Procedural-generate50actions

Contrary to the previous ones, this one does have 50 action labels, we just need to check if it works, and if it does put a warning in the generative procedure when it does not train.



Training Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210706-Procedural-generate50actions\Marathon.exe" --num-envs=25 --run-id=20210706-Samba50actions --no-graphics   --env-args --spawn-env=DReConHumanoid-v0  --num-spawn-envs=20


```

FAIL.

the DOF was not generated properly



07.07.2021



Still working on getting the procedural character right. 

20210707-PDopenloop-procedural is generated from the reference character.

Training instruction:

Training Instruction:

```shell
 mlagents-learn config\marathon_envs_config.yaml --env="envs\20210707-PDopenloop-procedural\Marathon.exe" --num-envs=25 --run-id=20210707-PDopenloop-procedural --no-graphics   --env-args --spawn-env=DReConHumanoid-v1  --num-spawn-envs=20


```

Not working very well (still training on a different machine to be completely sure)

For REFERENCE, I add:

1. the settings for the reference character that learns well
2. the  settings generated by the procedural stuff:

![ragdoll-settings1](D:\Users\JoanLlobera\Documents\repo-marathon-man-new\docs\img\ragdoll-settings1.png)

![Differences4ProceduralCase](D:\Users\JoanLlobera\Documents\repo-marathon-man-new\docs\img\Differences4ProceduralCase.png)

I change them  to match and generate:

20210707-PDopenloop-procedural-v2

Training Instruction:

```shell
 mlagents-learn config\marathon_envs_config.yaml --env="envs\20210707-PDopenloop-procedural-v2\Marathon.exe" --num-envs=25 --run-id=20210707-PDopenloop-procedural-v2 --no-graphics   --env-args --spawn-env=DReConHumanoid-v1  --num-spawn-envs=20


```

Not working very well (still training on a different machine to be completely sure)





mlagents-learn config\marathon_envs_config.yaml --env="envs\20210707-PDopenloop-procedural\Marathon.exe" --num-envs=25 --run-id=20210707-PDopenloop-procedural --no-graphics   --env-args --spawn-env=DReConHumanoid-v1  --num-spawn-envs=20

Not working very well (still training on a different machine to be completely sure)





Training Instruction:

```shell
 mlagents-learn config\marathon_envs_config.yaml --env="envs\20210707-PDopenloop-procedural-v2\Marathon.exe" --num-envs=25 --run-id=20210707-PDopenloop-procedural-v2 --no-graphics   --env-args --spawn-env=DReConHumanoid-v1  --num-spawn-envs=20


```



NOTE: the previous version may not train due to the default enviornment in World Factory being loaded, instead of the actual one (this is the case in the edit view).

To be completely sure, I set up the default environment to  `DreConHumanoid-v1`



Training Instruction:

```shell
 mlagents-learn config\marathon_envs_config.yaml --env="envs\20210707-PDopenloop-procedural-v2\Marathon.exe" --num-envs=25 --run-id=20210707-PDopenloop-procedural-v2v2 --no-graphics   --env-args --spawn-env=DReConHumanoid-v1  --num-spawn-envs=20


```

2CHECK: that the version running in Artanim PC9 gives the same result when changing the default image (this would show that ignoring the defualt environemnt works well)



MarConHhumanoidEnvProcedural2:

changes added in Muscles:

- put natural freq to 40, (was 20) OK
- put damping to 1 (was 0.9) OK

changes added in ProcRagdollAgent:



- Reproduce DRECON (true) was false
- Dont Snap Mocap to Ragdoll (true) was false

changes added in DecisionRequester:

- Take Actions Between Decisions to TRUE (was false)

Instruction to train with this new setup:

```shell
 mlagents-learn config\marathon_envs_config.yaml --env="envs\20210707-PDopenloop-procedural-v3\Marathon.exe" --num-envs=25 --run-id=20210707-PDopenloop-procedural-v3 --no-graphics   --env-args --spawn-env=DReConHumanoid-v1  --num-spawn-envs=20
```



Since it works, I set up the generative environment to work like this By DEFAULT

This is environmnet MarConHumanoidEnv8, included in build 20210707-PDopenloop-procedural-v4, with the trigger instruction:

```shell
 mlagents-learn config\marathon_envs_config.yaml --env="envs\20210707-PDopenloop-procedural-v4\Marathon.exe" --num-envs=25 --run-id=20210707-PDopenloop-procedural-v4 --no-graphics   --env-args --spawn-env=MarConAutomatic-v0  --num-spawn-envs=20
```





status

DReConHumanoid-v1, which corresponds to prefab MarConHuumanoidEnvProcedural2, trains well. This was generated procedurally, and the modifications were applied MANUALLY



MarConAutomatic-v0, with prefab MarConHumanoidEnv8 FAILS, despite the only change introduced is to attempt to automate the procedural creation inlcuding these aspects. What is wrong, here?



**this works! completely procedural:**

Instruction:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210707-PDopenloop-procedural-v5\Marathon.exe" --num-envs=25 --run-id=20210707-PDopenloop-procedural-v4testMarConHumanoidEnv11-v2 --no-graphics   --env-args --spawn-env=MarConAutomatic-v0  --num-spawn-envs=20
```

Result is stored in prefab MarConHumanoidEnv11



09.07.2021

It is still a bit shaky, so I try again with the good old PD in environment MarConHumanoidEnv12:



```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210709-PDclassic-procedural\Marathon.exe" --num-envs=25 --run-id=20210709-PDclassic-procedural-testMarConHumanoidEnv12 --no-graphics   --env-args --spawn-env=MarConAutomatic-v1  --num-spawn-envs=20
```





I fix the way the foot sensors are calculated and train again:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210709-PDclassic-withFootImproved\Marathon.exe" --num-envs=25 --run-id=20210709-PDclassic-withFootImproved-v1 --no-graphics   --env-args --spawn-env=MarConAutomatic-v1  --num-spawn-envs=20
```





I fix the way the foot sensors are calculated and train again the SAMBA character:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210709-PDclassic-footImprovedSamba\Marathon.exe" --num-envs=25 --run-id=20210709-PDclassic-footImprovedSamba-v1 --no-graphics   --env-args --spawn-env=SambaCon-v0  --num-spawn-envs=20
```





13.07.2021

I fix the HandleOverlap and several other details and train again:

```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210713-PDclassic-withExtraFixes-v2\Marathon.exe" --num-envs=25 --run-id=20210713-PDclassic-withExtraFixes-v2-run1 --no-graphics   --env-args --spawn-env=MarConAutomatic-v3  --num-spawn-envs=20
```

commit: 0c0ea78fbe4e3d4b743f01b44069da114736d1f7

in repo mxmmarathon





since it works, I also try again the samba dancer, generated procedurally, on a different computer:





```shell
mlagents-learn config\marathon_envs_config.yaml --env="envs\20210713-PDclassic-SambaCon-v2\Marathon.exe" --num-envs=25 --run-id=20210713-PDclassic-SambaCon-v2-run1 --no-graphics   --env-args --spawn-env=SambaCon-v0  --num-spawn-envs=20
```







21.07.2021

Since training works, I try to train closer to the DReCon parameters:

```shell
mlagents-learn config\marathon_envs_config_DReCon.yaml --env="envs\20210713-PDclassic-withExtraFixes-v2\Marathon.exe" --num-envs=25 --run-id=20210713-PDclassic-withExtraFixes-v2-run2 --no-graphics   --env-args --spawn-env=MarConAutomatic-v3  --num-spawn-envs=20
```

commit: 0c0ea78fbe4e3d4b743f01b44069da114736d1f7





22.07.2021

I train the dancing queen with the same method, on the other computer:

DReCon parameters:

```
mlagents-learn config\marathon_envs_config_DReCon.yaml --env="envs\20210722-PDclassic-sambacon-v2\Marathon.exe" --num-envs=25 --run-id=20210722-PDclassic-sambacon-v2-run1 --no-graphics   --env-args --spawn-env=SambaCon-v1  --num-spawn-envs=20
```



NOT WORKING!
