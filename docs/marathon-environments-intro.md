# Marathon Environments

**Preview MarathonEnvs using the [Web Demo](http://marathonenvs.joebooth.com)**



---
## Environments

| **Controller (DReCon)** - Preview |  |
|:----------|:----------|
| ![Controller](images/Controller.gif) | A controller based agent, inspired by the DReCon paper (link below). The agent learns to follow a simple traditional controller agent and exhibits emergent behavior.<br /> **In Preview** ControllerMarathonMan-v0 |
| **Style Transfer** (DeepMimic) |  |
| Learning from motion capture examples, inspired by the DeepMimic paper (link below). The agent learns the motion capture sequence using a phase value. MarathonManWalking-v0<br />MarathonManRunning-v0 <br />MarathonManJazzDancing-v0<br />MarathonManMMAKick-v0<br />MarathonManPunchingBag-v0<br />MarathonManBackflip-v0 | ![StyleTransfer](images/StyleTransfer.gif) |
| **Procedural Environments** |  |
| ![Terrain](images/Terrain.gif) | <p>Procedurally-generated terrains aimed at addressing overfitting in Reinforcement Learning and generalizable skills.<br />TerrainHopper-v0<br />TerrainWalker2d-v0<br />TerrainAnt-v0<br />TerrainMarathonMan-v0 |
|                                                              |                                                              |
| Classical implementations of Ant, Hopper, Walker-2d, Humanoid<br />Hopper-v0<<br />Walker2d-v0<br />Ant-v0<br />MarathonMan-v0 | ![Classical](images/Classical.gif) |
| **Sparse - Experimental** |  |
| Sparse reward version of a humanoid learning to walk. The agent receives a single reward at the end of the episode.<br />MarathonManSparse-v0 | |



---
## Releases

**The latest version is v4.0.0** 

The following table lists releases, the required unity version, and links to release note, source code, and binaries:

**TODO:** add information on the latest release.



 **Version** | **Unity** | **Updated Environments** | **Source** | **MacOS** | **Windows** | **Linux** | **Web** | **Paper** |
|:-------:|:------------:|:-------------------:|:-------:|:--------:|:---------:|:---------:|:---------:|:---------:|
| **master (unstable)** | 2020.1 | ControllerMarathonMan-v0 | -- | -- | -- | -- | -- | -- | -- |
| [**v3.0.0**](https://github.com/Unity-Technologies/marathon-envs/releases/tag/v3.0.0) | 2020.1 beta.12 | ControllerMarathonMan-v0 | [Source](https://github.com/Unity-Technologies/marathon-envs/tree/v3.0.0) | [MacOS](https://github.com/Unity-Technologies/marathon-envs/releases/download/v3.0.0/MarathonEnvsMacOS.zip) | -- | [Linux](https://github.com/Unity-Technologies/marathon-envs/releases/download/v3.0.0/MarathonEnvsLinux.zip) | [Web](http://marathonenvs.joebooth.com) | [arXiv](https://arxiv.org/abs/2006.07508) | 
| [**v2.0.0**](https://github.com/Unity-Technologies/marathon-envs/releases/tag/v2.0.0) | 2018.4 LTS | MarathonManWalking-v0 MarathonManRunning-v0 MarathonManJazzDancing-v0 MarathonManMMAKick-v0 MarathonManPunchingBag-v0  | [Source](https://github.com/Unity-Technologies/marathon-envs/tree/v2.0.0) | [MacOS](https://github.com/Unity-Technologies/marathon-envs/releases/download/v2.0.0/MarathonEnvsMacOS.zip) | -- | [Linux](https://github.com/Unity-Technologies/marathon-envs/releases/download/v2.0.0/MarathonEnvsLinux.zip) | -- | -- |
| [**v2.0.0-alpha.2**](https://github.com/Unity-Technologies/marathon-envs/releases/tag/v2.0.0-alpha.2) | 2018.4 LTS | -- | [Source](https://github.com/Unity-Technologies/marathon-envs/tree/v2.0.0-alpha.2) | [MacOS](https://github.com/Unity-Technologies/marathon-envs/releases/download/v2.0.0-alpha.2/MarathonEnvsMacOS.zip) | [Windows](https://github.com/Unity-Technologies/marathon-envs/releases/download/v2.0.0-alpha.2/MarathonEnvsWindows.zip) | [Linux](https://github.com/Unity-Technologies/marathon-envs/releases/download/v2.0.0-alpha.2/MarathonEnvsLinux.zip) | -- | [AAAI 2019](https://arxiv.org/abs/1902.09097) |
| [**v2.0.0-alpha.1**](https://github.com/Unity-Technologies/marathon-envs/releases/tag/v2.0.0-alpha.1) | 2018.4 LTS | MarathonManBackflip-v0 MarathonMan-v0 ManathonManSparse-v0 TerrainHopperEnv-v0, TerrainWalker2dEnv-v0, TerrainAntEnv-v0, TerrainMarathonManEnv-v0 | [Source](https://github.com/Unity-Technologies/marathon-envs/tree/v2.0.0-alpha.1) | -- | -- | -- | -- | -- |
| [**v0.5.0a**](https://github.com/Unity-Technologies/marathon-envs/releases/tag/0.5.0a) | 2018.2 | Hopper-v0, Walker2d-v0, Ant-v0, Humanoid-v0 | [Source](https://github.com/Unity-Technologies/marathon-envs/tree/0.5.0a) | -- | -- | -- | -- | [Blog](https://towardsdatascience.com/gettingstartedwithmarathonenvs-v0-5-0a-c1054a0b540c) |



### Next

* See [Training](marathon-environments-training.md) to train the different Marathon Environments


