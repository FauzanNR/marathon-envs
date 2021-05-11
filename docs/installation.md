## Installation

### 1. Installation with conda environment file

1. Make sure you have the latest graphics drivers updated. Restart the computer.
2. Install anaconda. If it is already installed, make sure you have the latest version by running, with admin privileges, `conda update anaconda`. Restart the computer.
3. Make sure you have the latest version of pip by running, with admin privileges, `python -m pip install --upgrade pip`
4. In the conda command prompt, go to the home of this repo
5. Run `conda env create -f environment.yml `



If you update from a previous ml-agents version, you can use `conda env update -f environment.yml `. Remember to restart the computer after that.





### 2. Common mistakes

On windows,  missing packages. If you have an error similar to:



`Error loading site-packages\torch\lib\caffe2_detectron_ops_gpu.dll or one of its dependencies.`

Then the fix seems to be:

https://github.com/pytorch/pytorch/issues/35803

If the error is:

`Error loading "c:\users\joanl\.conda\envs\marathon-envs\lib\site-packages\torch\lib\cudnn_adv_infer64_8.dll" or one of its dependencies.`

Then, you may install the latest C++ redistributable



https://support.microsoft.com/en-ca/help/2977003/the-latest-supported-visual-c-downloads 



If you still have trouble using the previous, you may have memory limits. Try setting up the training with less environments.