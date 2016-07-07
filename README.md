# IvyGenerator
An Ivy Generator for Unity5.x

How To Use
-------
After Import unitypackage or copy all files under Assets folder, you can use Unity main Menu, "Window"->"Ivy Generation System"->"Ivy Manager"
to create the Ivy generator.

Import your obj model into scene, press "start plant" and key 'p' to place the ivy root on the surface of the object!

press "grow", and enjoy watching the ivy growing! 

Press "birth" to generate the full geometry after setting  three kinds of plant textures in editor Inspector Pannel.

Press "export obj" to save your ivy geometry as obj+mtl for usage in your 3d world after.

Please triangulate your obj model before loading it, and keep your planted object simple, since the growing process slows down in complex scenes heavily. You can customize your ivy using the provided settings - tool-tips will help you to understand the settings.


>The ivy grows from one single root following different forces: a primary growth direction (the weighted average of previous growth directions), a random influence, an adhesion force towards other objects, an up-vector imitating the phototropy of plants, and finally gravity. This simple scheme reveals that the goal was not to provide a biological simulation of growing ivy but rather a simple approach to producing complex and convincing vegetation that adapts to an existing scene. The ivy generator imports and exports obj+mtl files.

*---- Thomas Luft from University of Konstanz*

You can find other Win32, MacOS, and Linux version including the source code [here](http://http://graphics.uni-konstanz.de/~luft/ivy_generator/).

Requires Unity5.x  or higher.

Developed By
-------
The original version comes from
[Thomas Luft](http://graphics.uni-konstanz.de/mitarbeiter/luft.php?language=english) in 
[University of Konstanz](http://graphics.uni-konstanz.de/)
### [Click here visit An Ivy Generator Project](http://http://graphics.uni-konstanz.de/~luft/ivy_generator/)

The file *Assets/Ivy/Scripts/Editor/EditorCoroutine.cs* come from *EditorCoroutines for Unity3D* project, Please visit [https://github.com/FelixEngl/EditorCoroutines](https://github.com/FelixEngl/EditorCoroutines) for more information.
Thanks to  Felix Engl - <felix.engl@hotmail.com>


ported by phoenixzz (Weng xiao yi) - <phoenixzz@sina.com>


License
-------
This program is free software, Use for free.