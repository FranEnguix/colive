# FIVE: Flexible Intelligent Virtual Environment designer 
FIVE is a multi-platform, distributed software created for testing Multi-Agent Systems that helps you design highly customized 3D intelligent virtual environments.

![Orange orchard IVE with 5 agents](examples/pictures/orange_orchard_field_1.png)

FIVE is composed by:
- FIVE server, coded in Unity.
- FIVE client, based on SPADE Python agents.


## Installation

### FIVE server
FIVE is available for Windows, Linux and MacOS. You will find a *vanilla* version at the release section of this GitHub repository with all the OS-built versions of the FIVE (Unity) server. 
If you prefer to build the server yourself, you can clone the [src/server](src/server/) folder and compile your own version of the FIVE server. When you open the Unity project, you will need three packages to run the simulation:

`
https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector
`

`
https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.visualizations
`

`
https://github.com/Unity-Technologies/URDF-Importer.git?path=/com.unity.robotics.urdf-importer#v0.5.2
`

To install these packages you need to open Window > Package Manager, click the + icon and select Add Package from git URL. Then, you have to copy and paste the URLs described above, one by one.

### FIVE client
The SPADE agents are required to clone the [src/client](src/client/) folder and install the SPADE package. You can install it by executing:

`
pip install spade==2.3.2
`

`
pip install pytz
`	


## Usage
We can define the agents of our simulation, the IVE features and the objects placed in it. In order to archive this purpose we have to tweak the configuration files:

> **Note**: There is a [website](https://franenguix.github.io/five/web/) under progress that will help you tweak these files in the future.

### FIVE server
You have to configure **map.txt**, **map_config.json** and **map.json** files.
- **map.txt** lets you design an environment map by writing ASCII characters.
- **map_config.json** is used to create a link between the characters of the **map.txt** and the object of the environment. For example, you may want to create a forest with a certain distribution of trees, so you can write a 'T' character in **map.txt** and then create a link with the Unity object (prefab) in **map_config.json**.
- **map.json** it is used to instantiate highly customized individual objects and light conditions. For example, you can define an empty object and name it "Agent spawner" and assign a position to it; then, you can reference this empty object by its name in **configuration.json** file and set it as the starting point of the agents. 

### FIVE client
Agents only require one configuration file, which is named **configuration.json**. This file has two sections:
- **fiveserver** is used to specify the fiveserver name and domain at the XMPP server, so agents can communicate with it.
- **agents** is a list of agents to set the agent name, avatar, starting position, etc. 

We can define the agent behaviour by creating a new Python file in the folder [src/client/behaviours](src/client/behaviours) and setting the name of the file without the *py* extension on the **behaviour** property in the file [configuration.json](src/client/configuration.json). An example of an agent behaviour is [default.py](src/client/behaviours/default.py).

### Run FIVE

To run the IVE with the inhabitant agents you have to:
1. Run the XMPP server.
2. Run the FIVE server (Unity).
3. Run the agents by executing the **launcher.py** file.

![Orange orchard IVE inside vision](examples/pictures/orange_orchard_field_2.png)

## Preconfigured example
Look at the release version of this GitHub repository to run a compiled version of FIVE.
