using UnityEngine;
using System.Collections;

public class CollisionDetect : MonoBehaviour
{
    public bool isCollided;
    public float otherPlayerHp;

    void Start()
    {
        Physics.IgnoreCollision(gameObject.collider, gameObject.transform.parent.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Destruct") && !rigidbody.isKinematic)
        {
            isCollided = true;
            if (other.gameObject.transform.parent.name == "TreeBase1")
            {
                GameObject Tree = other.gameObject.transform.parent.gameObject;
                Tree.GetComponent<PropHP>().HP -= Mathf.Abs(gameObject.rigidbody.velocity.x);
                Debug.LogWarning("HIT");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Destruct"))
        {
            isCollided = false;
        }
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        applyForce(collisionInfo.gameObject);
        //networkView.RPC("knockBack", RPCMode.All,collisionInfo.gameObject);
    }

    [RPC]
    public void applyForce(GameObject player)
    {
        if (player.gameObject.tag == "PlayerCollider" && gameObject.name == "PlayerFist" && gameObject.transform.parent.GetComponent<PlayerMovement>().timing)
        {
            Physics.IgnoreCollision(collider, player.collider);
            Debug.Log("Ignored");
        }
        if (player.gameObject.tag == "Player")
        {
            GameObject playerFist = gameObject.transform.parent.gameObject;
            GameObject otherPlayer = player.gameObject;
            Vector3 knockForce = Vector3.zero;
            otherPlayerHp = otherPlayer.GetComponent<PlayerCombat>().HP;

            float punchVelocity = playerFist.GetComponent<PlayerCombat>().punchSpeed;
            float direction = playerFist.GetComponent<PlayerCombat>().directionMultiplyer;
            //Up Modifer
            knockForce.y = (punchVelocity / 2) * otherPlayerHp;


            //KnockBackForward
            if (direction > 0)
            {
                knockForce.x = direction * punchVelocity * otherPlayerHp;
            }
            else if (direction < 0)
            {
                knockForce.x = direction * punchVelocity * otherPlayerHp;

            }
            Debug.LogWarning("hit: " + direction + " : : " + punchVelocity);
            otherPlayer.transform.networkView.RPC("knockBack", RPCMode.All, knockForce);
            otherPlayer.transform.networkView.RPC("Hit", RPCMode.All);
            otherPlayer.transform.networkView.RPC("SendHP", RPCMode.All);

        }
    }

    void OnSerialNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            float Hp = otherPlayerHp;
            stream.Serialize(ref Hp);
        }
        else
        {
            float HpRec = otherPlayerHp;
            stream.Serialize(ref HpRec);
            otherPlayerHp = HpRec;
        }
    }
}
