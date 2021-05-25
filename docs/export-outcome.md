


## Export to an existing Unity3D project

To integrate an environment from *MarathonEnvs* in an existing project, do the following steps: 

1. Open the file Packages/manifest.json in the target project, and add to the existing package dependencies the following:

```json
    "com.joebooth.many-worlds": "https://github.com/sohojoe/many-worlds.git?path=/com.joebooth.many-worlds#v1.1.0",
	    "com.unity.ml-agents": "1.7.2-preview",
	    "com.unity.barracuda": "1.0.4",
```

2. Take the entire *marathon-envs* folder and add it to your project
3. Adjust the Layers and how they connect physically (see image below)

![Layers2add](.\img\2.1.layers2add_highlight.png)
	
![Physics Set up](.\img\2.2.physics2set-up.png)

