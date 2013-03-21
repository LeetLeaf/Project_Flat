using UnityEngine;
using System.Collections;

public class PlayerHp : MonoBehaviour 
{
    public float HP;	
	// Update is called once per frame
	void Start ()
    {
        HP = 1;
	}
    void OnSerialNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            float health = HP;
            stream.Serialize(ref health);
        }
        else
        {
            float healthRec = HP;
            stream.Serialize(ref healthRec);
            HP = healthRec;
        }
    }
}
