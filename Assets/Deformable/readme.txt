Thank you for purchasing Impact Deformable.

USAGE
-----

	Just place the S_Deformable.js in "Standard Assets" folder then attach it to any object that you want to be deformable.

SETTINGS
--------

	MeshFilter: This points to the MeshFilter that holds the mesh to be deformed. If it is not in the same object as one that S_Deformable is attached then you need to set it manually (check the Car Derby demo for an example). This can also be changed at runtime.

	Hardness: Specifies amount of distortion that this object will suffer from each impact. Higher values make harder objects that are less deformed by collisions. Minimum value considered is 0.1.

	Deform mesh collider: Makes the MeshCollider be updated along with the MeshFilter. This physically "carves" the object with the deformations. The downsides are higher CPU usage at each collision and that it requires a MeshCollider to work.

	Update Frequency: Setting a value major than 0 here makes Impact Deformable work in Delayed Mode. In this mode the mesh updates (and mesh collider updates if configured to) will be applied at UpdateFrequency times per second. This uses less CPU power if compared to immediate mode (default) where changes are made on each impact. The Metal Rain demo show this mode in use.

	Max Vertex Mov: Set maximum vertex distance (in object space) from its original position. Leave 0 (default) for no limit.

	Deformed Vertex Color: Color at deformed vertex applied proportionally with its distance from original position.

	Hardness Map: A 2D texture where alpha channel is taken as hardness map (more opaque = harder) and applied at object with mesh UVs. This allows to specify harder and softer parts on object. Leave null for uniform hardness (default). Please note that this texture must be imported with "Read/Write Enabled". Check the file T_HardnessMap used in Metal Rain demo for an example of texture and its import settings.

DEMOS
-----

	The cube and sphere demo uses a subdivided cube as deformable target with soft setting and request to deform mesh collider. Note that it also uses a custom shader that handles vertex color information. The code at S_DemoCube.js, attached to cube, handles repairing (25%) on click.

	The Metal Rain demo shows how to tweak settings to handle lot of collisions and keep performance. The delayed mode is used requesting updates at 10 times/sec. There is also some GUI controls to change hardness, hardness map settings and turn mesh collider update on/off. As in the cube demo, just click on the object to repair 25% of damage.

	The Car Derby demo shows interaction between two deformables using convex mesh colliders. Both cars can be repaired by clicking and holding the mouse above. The area repair mode is used just repairing vertex near mouse cursor position (code at S_Repair.js).


Please let me known on any problem using this script.

L-Tyrosine
Arcadium Playware
l-tyrosine@arcadiumplayware.com