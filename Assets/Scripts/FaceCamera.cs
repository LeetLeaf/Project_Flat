using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour 
{
    public GameObject mainCamera;

    public float rotXOffset;
    public float rotYOffset;
    public float rotZOffset;
    public bool enabled = true;

	// Update is called once per frame
	void Update () 
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (enabled)
        {
            transform.rotation = mainCamera.transform.rotation;
            transform.Rotate(rotXOffset, rotYOffset, rotZOffset, Space.Self);
        }
    }
}
