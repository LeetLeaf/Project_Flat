using UnityEngine;
using System.Collections;

public class DeformMesh : MonoBehaviour 
{
    public MeshFilter meshFilter;
    public float Hardness = 0.5f;
    public bool DeformMeshCollider = true;
    public float UpdateFrequency = 0;
    public float MaxVertexMov = 0;
    public Color32 DeformedVertexColor = Color.gray;
    public Texture2D HardnessMap;

    private Mesh mesh;
    private MeshCollider meshCollider;
    private Vector3[] baseVertices;
    private Color32[] baseColors;
    private float sizeFactor;
    private Vector3[] vertices;
    private Color32[] colors;
    private float[] map;
    private bool meshUpdate = false;
    private float lastUpdate;
    private Texture2D appliedMap;

    void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();

        if (!meshFilter)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (meshFilter)
        {
            LoadMesh();
        }
        else
        { 
            Debug.LogWarning("Deformable component warning: No mesh filter assigned for object " + gameObject.ToString());
        }
    }

    private void LoadMesh()
    {
        if (meshFilter)
        {
            mesh = meshFilter.mesh;
        }
        else
        {
            mesh = null;
        }

        if (!mesh)
        { 
            Debug.LogWarning("Deformable component warning: Mesh at mesh filter is null "+ gameObject.ToString());
            return;
        }

        vertices = mesh.vertices;
        colors = mesh.colors32;

        baseVertices = mesh.vertices;
        baseColors = mesh.colors32;

        Vector3 s = mesh.bounds.size;
        sizeFactor = Mathf.Min(s.x,s.y,s.z);
    }

    private void LoadMap()
    {
        appliedMap = HardnessMap;

        if (HardnessMap)
        {
            Vector2[] uvs = mesh.uv;
            map = new float[uvs.Length];
            int c = 0;
            foreach (Vector2 uv in uvs)
            {
                try
                {
                    map[c] = HardnessMap.GetPixelBilinear(uv.x, uv.y).a;
                }
                catch
                {
                    Debug.LogWarning("Deformable component warning: Texture at HardnessMap must be readable (check Read/Write Enabled at import settings). Hardness map not applied.");
                    map = null;
                    return;
                }
                c++;
            }
        }
        else
            map = null;
    }
    private void Deform(Collision collision)
    {
        if (!mesh || !meshCollider)
            return;

        float colForce = Mathf.Min(1, collision.impactForceSum.sqrMagnitude / 1000);

        if (colForce < 0.01f)
            return;

        float distFactor = colForce * (sizeFactor * (0.1f / Mathf.Max(0.1f, Hardness)));

        foreach (ContactPoint contact in collision.contacts)
        {
            for (int i = 0; i < vertices.Length;i++)
            {
                Vector3 p = meshFilter.transform.InverseTransformPoint(contact.point);
                float d = (vertices[i] - p).sqrMagnitude;
                if (d <= distFactor)
                {
                    Vector3 n = meshFilter.transform.InverseTransformDirection(contact.normal * (1 - (d / distFactor)) * distFactor);

                    if (map != null)
                        n *= 1 - map[i];

                    vertices[i] += n;

                    if (MaxVertexMov > 0)
                    {
                        float max = MaxVertexMov;
                        n = vertices[i] - baseVertices[i];
                        d = n.magnitude;
                        if (d > max)
                            vertices[i] = baseVertices[i] + (n * (max / d));

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
                            colors[i] = Color.Lerp(baseColors[i], DeformedVertexColor, (vertices[i] - baseVertices[i]).magnitude / (distFactor * 10));
                        }
                    }
                }
            }
        }

        RequestUpdateMesh();
    }

    void Repair(float repair)
    {
        Repair(repair, Vector3.zero, 0);
    }

    void Repair(float repair, Vector3 point, float radius)
    {
        if (!mesh || !meshFilter)
            return;

        point = meshFilter.transform.InverseTransformDirection(point);

        for (int i = 0; i < vertices.Length; i=i)
        {
            try
            {
                if (radius > 0)
                {
                    if ((point - vertices[i]).magnitude >= radius)
                        continue;
                }
            }
            finally
            {
                i++;
            }
        }

        RequestUpdateMesh();
    }
    private void RequestUpdateMesh()
    {
        if (UpdateFrequency == 0)
            UpdateMesh();
        else
            meshUpdate = true;
    }
    private void UpdateMesh()
    {
        mesh.vertices = vertices;
        if (colors.Length > 0)
            mesh.colors32 = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if ((meshCollider) && (DeformMeshCollider))
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        lastUpdate = Time.time;
        meshUpdate = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (Mathf.Abs(collision.gameObject.rigidbody.velocity.x) > 11 ||
                Mathf.Abs(collision.gameObject.rigidbody.velocity.y) > 11 ||
                Mathf.Abs(collision.gameObject.rigidbody.velocity.z) > 11)
            {
                Debug.LogError("Player Velocity: " + collision.gameObject.rigidbody.velocity);
                Deform(collision);
            }
            return;
        }
        if (collision.gameObject.name == "PlayerFist")
        {
            Debug.Log("PunchTree");
            if (collision.gameObject.rigidbody.isKinematic)
                return;
            Deform(collision);
        }
        Deform(collision);
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (Mathf.Abs(collision.gameObject.rigidbody.velocity.x) > 11 ||
                Mathf.Abs(collision.gameObject.rigidbody.velocity.y) > 11 ||
                Mathf.Abs(collision.gameObject.rigidbody.velocity.z) > 11)
            {
                Debug.LogError("Player Velocity: " + collision.gameObject.rigidbody.velocity);
                Deform(collision);
            }
            return;
        }
        if (collision.gameObject.name == "PlayerFist")
        {
            Debug.Log("PunchTree");
            if (collision.gameObject.rigidbody.isKinematic)
                return;
            Deform(collision);
        }
        Deform(collision);
    }
    void FixedUpdate()
    {

        if (((meshFilter) && (mesh != meshFilter.mesh)) || ((!meshFilter) && (mesh)))
            LoadMesh();

        if (HardnessMap != appliedMap)
            LoadMap();

        if ((meshUpdate) && (Time.time - lastUpdate >= (1 / UpdateFrequency)))
            UpdateMesh();
    }


    void OnSerialNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            foreach (Vector3 vertex in vertices)
            { 
                Vector3 vertexPos = vertex;
                stream.Serialize(ref vertexPos);
            }
            
        }
        else
        {

            for (int i = 0; i < vertices.Length;i++)
            {
                Vector3 vertexRec = vertices[i];
                stream.Serialize(ref vertexRec);
                vertices[i] = vertexRec;
            }
        }
    }
}
