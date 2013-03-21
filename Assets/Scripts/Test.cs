using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

    public int one = 1;
	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (one > 1)
        {
            Debug.LogError("OMGH");
        }
    }
    void OnSerialNetworkView(BitStream stream, NetworkMessageInfo info)
    {

        if (stream.isWriting)
        {

            int blah = one;
            stream.Serialize(ref one);
        }
        else
        {
            int blahRec = one;
            stream.Serialize(ref blahRec);
            one = blahRec;
        }
    }
}
