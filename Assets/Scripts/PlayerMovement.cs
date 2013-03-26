using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour 
{
    public float speed;
    public float jumpForce;
    public bool timing;
    private float countdown;
    private int numberOfJumps = 1;
    public bool grounded;
    public bool bounceForceApplied;

    void Start()
    {
        
        if (!networkView.isMine)
        {
            enabled = false;
        }
        
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.tag == "PlayerCollider" && !bounceForceApplied)
        {
            speed = 10;
            Debug.Log("Player is touching a tree.");
            if (transform.rigidbody.velocity.z > 0)
            {
                Debug.Log("force added");
                transform.rigidbody.AddForce(new Vector3(0, 0, 50));
                bounceForceApplied = true;
            }
            else if (transform.rigidbody.velocity.z < 0)
            {
                transform.rigidbody.AddForce(new Vector3(0, 0, -50));
                bounceForceApplied = true;
            }
            transform.rigidbody.AddForce(new Vector3(0, -400, 0));
        }
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.tag == "PlayerCollider")
        {
            bounceForceApplied = false;
            speed = 10;
        }
    }


    void StartTimer(float time)
    {
        timing = true;
        countdown = time;
    }

	// Update is called once per frame
	void Update () 
    {
        if (networkView.isMine)
        {
            //Turning right
            if (Input.GetKeyDown(KeyCode.D) && transform.localScale.x != -1)
            {
                if (!Input.GetKey(KeyCode.A))
                {
                    animation.Play("TurnRight");
                }
            }
            //Turning left
            if (Input.GetKeyDown(KeyCode.A) && transform.localScale.x != 1)
            {
                if(!Input.GetKey(KeyCode.D))
                {
                    animation.Play("TurnLeft");
                }
            }
            //Jump animation
            if (Input.GetKeyDown(KeyCode.Space) && numberOfJumps > 0)
            {
                animation.Play("jump");
            }
            //Walk Input
            if (Input.GetAxis("Horizontal") > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            //Walk animation
            if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W))
                && !animation.isPlaying && numberOfJumps == 1)
                animation.Play("walk");
            
            if (animation.IsPlaying("walk") 
                && (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.W)))
            {
                animation.Stop("walk");
            }
                
            if (Input.GetKeyDown(KeyCode.D) && !timing)
            {
                StartTimer(1);
            }

            if (timing)
            {
                countdown -= Time.deltaTime;

                if (countdown <= 0)
                {
                    //Do your stuff
                    Debug.Log("Countdown!");
                    timing = false;
                }
            }



            //Jump Start
            if (Input.GetKeyDown(KeyCode.Space) && numberOfJumps > 0)
            {
                rigidbody.AddForce(new Vector3(0, jumpForce, 0));
                grounded = false;
            }
            if (grounded)
                numberOfJumps = 1;
            else
                numberOfJumps--;

            //Jump End

            float xMovement = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            float zMovement = Input.GetAxis("Vertical") * speed * Time.deltaTime;

            transform.Translate(new Vector3(xMovement, 0, zMovement));
        }
	}

    void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.tag == "PlayerCollider")
        {
            Debug.Log("Player is touching a tree.");
            grounded = false;
        }
        else
        {
            grounded = true;
        }
    }
    [RPC]
    public void knockBack(Vector3 force)
    {
        rigidbody.AddForce(force);
    }

    void OnSerialNetworkView(BitStream stream, NetworkMessageInfo info)
    {

        if (stream.isWriting)
        {
            Debug.LogError("sent");
            Vector3 pos = transform.position;
            stream.Serialize(ref pos);
        }
        else
        {
            Vector3 posRec = Vector3.zero;
            stream.Serialize(ref posRec);
            transform.position = posRec;
        }
    }


}
