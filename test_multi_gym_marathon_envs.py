import gym
import numpy as np

# from marathon_envs.envs import MarathonEnvs
from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymWrapper
from mlagents_envs.side_channel.engine_configuration_channel import (
    EngineConfigurationChannel,
)
from sys import platform

from timeit import default_timer as timer
from datetime import timedelta
import os


def MarathonEnvs(
        environment_name: str,
        num_spawn_envs: int = 1,
        worker_id: int = 0,
        marathon_envs_path: str = None,
        no_graphics: bool = False,
        use_editor: bool = False,
        inference: bool = False,
    ):
    """
    Environment initialization
    :param environment_name: The Marathon Environment 
    :param num_spawn_envs: The number of environments to spawn per instance
    :param worker_id: Worker number for environment.
    :param marathon_envs_path: alternative path for environment
    :param no_graphics: Whether to run the Unity simulator in no-graphics mode
    :param use_editor: If True, assume Unity Editor is the envionment (use for debugging)
    :param inference: If True, run in inference mode (normal framerate)
    """
    use_visual: bool = False
    uint8_visual: bool = False
    multiagent: bool = True # force multiagent
    flatten_branched: bool = False
    allow_multiple_visual_obs: bool = False

    base_port = 5005
    # use if we want to work with Unity Editoe
    if use_editor:
        base_port = DEFAULT_EDITOR_PORT
        marathon_envs_path = None
    elif marathon_envs_path is None:
        marathon_envs_path = os.path.join('envs', 'MarathonEnvs')
        if platform == "win32":
            marathon_envs_path = os.path.join(marathon_envs_path, 'Marathon Environments.exe')
    args = ['--spawn-env='+environment_name]
    args.append('--num-spawn-envs='+str(num_spawn_envs))

    engine_configuration_channel = EngineConfigurationChannel()
    channels = [engine_configuration_channel]

    unity_env = UnityEnvironment(
        marathon_envs_path,
        worker_id = worker_id,
        base_port=base_port,
        side_channels=channels,
        no_graphics=no_graphics,
        additional_args = args,
    )
    if not inference:
        engine_configuration_channel.set_configuration_parameters(
            width=160, height=160, quality_level=0, 
            time_scale=20., target_frame_rate=-1)
    # env = UnityToGymWrapper(unity_env, allow_multiple_obs=True)
    env = UnityToGymWrapper(unity_env)
    return env

env_names = [
    'Hopper-v0', 
    'Walker2d-v0', 
    'Ant-v0', 
    'MarathonMan-v0', 
    'MarathonManSparse-v0'
    ]
for env_name in env_names:
    print ('-------', env_name, '-------')
    env = MarathonEnvs(env_name, 3)
    
    obs = env.reset()
    total_score = 0.
    total_steps = 0
    episodes = 0
    while episodes < 15:
        action = [env.action_space.sample() for _ in range(env.number_agents)]
        obs, rewards, dones, info = env.step(action)
        total_score += rewards.sum()
        total_steps += 1
        num_done = dones.sum()
        if num_done > 0:
            episodes += num_done
            # obs = env.reset()
    print ('total_score', total_score, 'total_steps', total_steps)
    env.close()    