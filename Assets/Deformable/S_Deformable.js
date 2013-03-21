#pragma strict

// Mesh filter with mesh to deform
var meshFilter: MeshFilter;
// Impact resistance to calculate amount of deformation applied to the mesh
var Hardness: float = 0.5;
// Configure if the mesh at collider must also be deformed (only works if a MeshCollider is in use)
// Warning: High CPU usage - in low cpu environments disable it or combine with fewer mesh updates per second (below)
var DeformMeshCollider: boolean = true;
// Configure the mesh (and mesh collider) update frequency in times per second. 0 for real time (high CPU usage)
var UpdateFrequency: float = 0;
// Maximum movement allowed for a vertex from its original position (0 means no limit)
var MaxVertexMov: float = 0;
// Color to be applied at deformed vertices (only works for shaders that handle vertices colors)
var DeformedVertexColor: Color32 = Color.gray;
// Texture to modulate maximum movement allowed (uses alpha channel)
var HardnessMap: Texture2D;

// Mesh to deform
private var mesh: Mesh;
// Cached access
private var meshCollider: MeshCollider;
// Backup of original mesh form
private var baseVertices: Vector3[];
// Backup of original mesh vertices colors
private var baseColors: Color32[];
// Size factor of mesh
private var sizeFactor: float;
// Vertex and color array that will suffer changes
private var vertices: Vector3[];
private var colors: Color32[];
private var map: float[];
// Flag requesting mesh update
private var meshUpdate: boolean = false;
// Mesh update timer
private var lastUpdate: float;
// Applied hardness map
private var appliedMap: Texture2D;

function Awake ()
{
	// Capture mesh collider
	meshCollider = GetComponent(MeshCollider);
	
	// Capture local mesh filter if not assigned
	if (!meshFilter)
		meshFilter = GetComponent(MeshFilter);
	
	// Load mesh data
	if (meshFilter)
		LoadMesh();
	else
		// Warning if no mesh filter assigned at start
		Debug.LogWarning('Deformable component warning: No mesh filter assigned for object ' + gameObject.ToString());
}

private function LoadMesh ()
{
	// Capture mesh
	if (meshFilter)
		mesh = meshFilter.mesh;
	else
		mesh = null;
		
	// Check mesh assigned
	if (!mesh)
	{
		// Warning if no mesh assigned
		Debug.LogWarning('Deformable component warning: Mesh at mesh filter is null ' + gameObject.ToString());		
		return;
	}
	
	// Capture mesh arrays
	vertices = mesh.vertices;
	colors = mesh.colors32;
	
	// Save original mesh form and colors
	baseVertices = mesh.vertices;
	baseColors = mesh.colors32;
	
	// Calc size factor
	var s: Vector3 = mesh.bounds.size;
	sizeFactor = Mathf.Min(s.x, s.y, s.z);
}

private function LoadMap ()
{
	appliedMap = HardnessMap;

	// Load hardness map
	if (HardnessMap)
	{
		var uvs: Vector2[] = mesh.uv;
		map = new float[uvs.length];
		var c: int = 0;
		for (var uv: Vector2 in uvs)
		{
			try
			{
				map[c] = HardnessMap.GetPixelBilinear(uv.x, uv. y).a;
			}
			catch (error)
			{
				Debug.LogWarning('Deformable component warning: Texture at HardnessMap must be readable (check Read/Write Enabled at import settings). Hardness map not applied.');		
				map = null;
				return;
			}
			c++;
		}
	}
	else
		map = null;
}

private function Deform (collision : Collision)
{
	// Check mesh assigned
	if ((!mesh) || (!meshFilter))
		return;
				
	// Calc collision force	
	var colForce: float = Mathf.Min(1, collision.impactForceSum.sqrMagnitude / 1000);
	
	// Ignore weak collisions
	if (colForce < 0.01)
		return;
		
	// distFactor is the amount of deforming in local coordinates 
	var distFactor: float = colForce * (sizeFactor * (0.1 / Mathf.Max(0.1, Hardness)));
	
	// Deform process
	for (var contact: ContactPoint in collision.contacts)
	{
		var i: int = 0;
		for (var vertex: Vector3 in vertices)
		{
			// Apply deformation only on vertex inside "blast area"
			var p: Vector3 = meshFilter.transform.InverseTransformPoint(contact.point);
			var d: float = (vertex - p).sqrMagnitude;
			if (d <= distFactor)
			{
				// Deformation is the collision normal with local deforming ratio
				// Vertices near the impact point must also be more deformed
				var n: Vector3 = meshFilter.transform.InverseTransformDirection(contact.normal * (1 - (d / distFactor)) * distFactor);
				
				// Apply hardness map if any
				if (map)
					n *= 1 - map[i];
					
				// Deform vertex
				vertex += n;
				
				// Apply max vertex movement if configured
				// Here the deformed vertex position is just scaled down to keep the best deformation while respecting limits
				if (MaxVertexMov > 0)
				{
					var max: float = MaxVertexMov;
					n = vertex - baseVertices[i];
					d = n.magnitude;
					if (d > max)
						vertex = baseVertices[i] + (n * (max / d));
						
					// Apply vertex color deformation
					// Deform color is applied in proportional amount with vertex distance and max deform
					if (colors.Length > 0)
					{
						d = (d / MaxVertexMov);
						colors[i] = Color.Lerp(baseColors[i], DeformedVertexColor, d);
					}
				}
				else
				{
					if (colors.Length > 0)
					{
						// Apply vertex color deformation
						// Deform color is applied in proportional amount with vertex distance and mesh distance factor
						colors[i] = Color.Lerp(baseColors[i], DeformedVertexColor, (vertex - baseVertices[i]).magnitude / (distFactor * 10));				
					}
				}
			}			
			i++;
		}
	}
	
	// Request mesh update
	RequestUpdateMesh();
}

function Repair (repair: float)
{
	Repair(repair, Vector3.zero, 0);
}

function Repair (repair: float, point: Vector3, radius: float)
{
	// Check mesh assigned
	if ((!mesh) || (!meshFilter))
		return;
		
	// Transform world point to mesh local
	point = meshFilter.transform.InverseTransformPoint(point);

	// Repairing is done returning vertices position and color to original positions by requested amount
	var i: int = 0;
	for (var vertex: Vector3 in vertices)
	{
		try
		{
			// Check for repair inside radius
			if (radius > 0)
			{
				if ((point - vertex).magnitude >= radius)
					continue;
			}
			
			// Repair
			var n: Vector3 = vertex - baseVertices[i];
			vertex = baseVertices[i] + (n * (1 - repair));
			if (colors.Length > 0)
				colors[i] = Color.Lerp(colors[i], baseColors[i], repair);
		}
		finally
			i++;
	}
	
	// Request mesh update
	RequestUpdateMesh();
}

private function RequestUpdateMesh ()
{	
	// Check immediate update mode
	if (UpdateFrequency == 0)
		UpdateMesh();
	else
		// Delayed mode
		meshUpdate = true;
}

private function UpdateMesh ()
{
	// Update vertices and colors
	mesh.vertices = vertices;	
	if (colors.Length > 0)
		mesh.colors32 = colors;
		
	// Recalc normals and bounds
	mesh.RecalculateNormals();
	mesh.RecalculateBounds();
	
	// Update mesh collider if requested
	if ((meshCollider) && (DeformMeshCollider))
	{	
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;
	}
	
	// Done
	lastUpdate = Time.time;
	meshUpdate = false;
}

function OnCollisionEnter (collision : Collision) 
{
	Deform(collision);
}

function OnCollisionStay (collision : Collision)
{
	Deform(collision);
}

function FixedUpdate ()
{
	// Check mesh change
	if (((meshFilter) && (mesh != meshFilter.mesh)) || ((!meshFilter) && (mesh)))
		LoadMesh();
		
	// Check hardness map change
	if (HardnessMap != appliedMap)
		LoadMap();

	// Delayed mesh update
	if ((meshUpdate) && (Time.time - lastUpdate >= (1 / UpdateFrequency)))
		UpdateMesh();
}
