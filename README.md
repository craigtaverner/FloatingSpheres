# FloatingSpheres

A simple network modeling app for VR. Using VR controllers to interactively build simple and perhaps even
complex network or graph models.

## Building

This application uses two plugins from the Unity Asset Store:

* The NewtonVR plugin - for creating, grabbing and manipulating the network/graph model
* The SteamVR plugin - for HTC Vive VR controller support

It was built and tested with HTC Vive, although the NewtonVR plugin also works with Oculus Rift. It should
be possible to get it to work with Oculus if the teleportation feature is changed or disabled.

To build and run in the Unity Editor:

* Checkout FloatingSpheres from github
* Launch unity on the scene file at Assets/Scenes/FloatingSpheres.unity
* Go to the assets store view and download and install NewtonVR and SteamVR
* In the scene heirarchy go to the NVRPlayer and check the settings in the inspector
  * Make sure that it has detected the SteamVR install or click the install button if not
  * Make sure that `Enable SteamVR` is selected

Click the run button to try it out.

## Running

When starting the app the player should find themselves on an empty surface with a stony texture. There is
a menu for more advanced features, but you can get started by simply clicking the trigger buttons on either
controller to create spheres, called 'nodes' in the air about head height above the controller used.
Use the grip buttons to grab the spheres and move them around.
They can be thrown and bounced but have no mass and will not fall to
the ground. They have drag so they will not fly too far away if you throw them. Grabbing two at the same time
will cause them to be connected by a 'reltionship' or 'edge'.
The relationship will attempt to grow or shrink to 1m in length at the default
scale. This allows you to build complex 3D frameworks that will hold their shape. If you pull on one node 
the relationship will stretch and start to pull the other nodes too.

Nodes can have labels which define their colors. The initial starting settings come with two labels `Person` is
colored RED and `Post` is colored BLUE. You can change this by opening the menu with the menu button on either
controllers (the button above the touch pad). Select which label you want to use to create nodes, or add and remove
labels to get a wider color palette.

Change to `delete` mode to delete nodes by grabbing them and while holding them pull the trigger. All relationships
to a node are deleted when the node is deleted.

Clicking the `Screenshot` menu item will take two screenshots, one from the perspective of the player and one from
a perspective above the playing field to the side to give a broader view.
Both are saved into the `FloatingSpheres/Screenshots` folder.

You can also load and save models using the menu. There are a few pre-build models available in the 
`FloatingSpheres/Models` folder to try out.

## Developing

This project was developed primarily within the unity editor itself. C# coding was done in both Visual Studio 2017 
and 'Visual Studio Code'.

The project is licensed under the GPL in the hopes that others find it useful.
