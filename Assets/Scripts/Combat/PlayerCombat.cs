using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour 
{
    public float punchSpeed;
    public float directionMultiplyerX;
    public float directionMultiplyerZ;
    public float HP;
    public bool hit;

    private bool timing;
    private float countdown;
    private float punchTime = 0.1f;
    private Vector3 lastPosition;


    void Start()
    {
        lastPosition = transform.FindChild("PlayerFist").transform.localPosition;
        HP = 1;
        hit = false;
    }
    void StartTimer(float time)
    {
        timing = true;
        countdown = time;
    }

    void OnCollisionEnter(Collision other)
    {
        Physics.IgnoreCollision(transform.collider, transform.FindChild("PlayerFist").collider);
    }

	// Update is called once per frame
	void Update () 
    {
        Physics.IgnoreCollision(transform.collider, transform.FindChild("PlayerFist").collider);


        //Tells the fist which way the player should be punching 
        if (Input.GetAxis("Horizontal") > 0)
        {
            directionMultiplyerX = 1;
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            directionMultiplyerX = -1;
        }
        if (transform.localScale.x > 0)
        {
            directionMultiplyerX = -1;
        }
        else
        {
            directionMultiplyerX = 1;
        }

        //Player Direction is forward or backward
        if (Input.GetAxis("Vertical") > 0)
            directionMultiplyerZ = 1;
        else if (Input.GetAxis("Vertical") < 0)
            directionMultiplyerZ = -1;
        else
            directionMultiplyerZ = 0;
        //Updates Player's HP
        if (hit)
        {
            HP += 0.25f;
            hit = false;
        }


        if (networkView.isMine)
        {

            /*Start Punch!
             * When the left mouse button is pressed the fist is then turned into
             * a physical object within the game. This allows force to be applied
             * which creates a nice effect of a quick jab! The current position of
             * the fist is saved before the force is applied that way it can return 
             * to the player's side after it has moved a distance of 3 units.
             */
            
            if (Input.GetButtonDown("Fire1") && !timing)
            {
                transform.FindChild("PlayerFist").collider.isTrigger = false;
                Debug.Log("Punch!!");
                float fistPunch = directionMultiplyerX * punchSpeed;
                lastPosition = transform.FindChild("PlayerFist").transform.localPosition;
                //transform.FindChild("PlayerFist").transform.Translate(new Vector3(fistPunch,0,0));
                transform.FindChild("PlayerFist").rigidbody.isKinematic = false;
                if (Input.GetKey(KeyCode.S))
                {
                    if (directionMultiplyerX == -1)
                        animation.Play("forwardPunchLeft");
                    else if (directionMultiplyerX == 1)
                        animation.Play("forwardPunchRight");
                }
                else if (Input.GetKey(KeyCode.W))
                {
                    if (directionMultiplyerX == -1)
                        animation.Play("backwardPunchLeft");
                    else if (directionMultiplyerX == 1)
                        animation.Play("backwardPunchRight");
                }
                else
                {
                    transform.FindChild("PlayerFist").rigidbody.AddForce(new Vector3(fistPunch, 0, 0));
                }
                StartTimer(punchTime);
            }

            if (timing)
            {
                countdown -= Time.deltaTime;

                if (countdown <= 0)
                {
                    //Do your stuff
                    Debug.Log("CoolDownDone");
                    timing = false;
                }
            }
            //End Punch!

        }
       
        if (Mathf.Abs(transform.FindChild("PlayerFist").localPosition.x) > Mathf.Abs(lastPosition.x) + 2.5f || Mathf.Abs(transform.FindChild("PlayerFist").localPosition.x) < Mathf.Abs(lastPosition.x) || !timing)
        {
            transform.FindChild("PlayerFist").transform.localPosition = lastPosition;
            transform.FindChild("PlayerFist").rigidbody.isKinematic = true;
            transform.FindChild("PlayerFist").collider.isTrigger = true;
        } 
        //networkView.RPC("OnSerialNetworkView", RPCMode.All);
	}
    [RPC]
    public void Hit()
    {
        hit = true;
    }

    [RPC]
    public float SendHP()
    {
        return HP;
    }

    public void OnSerialNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        float punch = punchSpeed;
        float direction = directionMultiplyerX;
        float health = HP;
        if (stream.isWriting)
        {
           
            punch = punchSpeed;
            stream.Serialize(ref punch);

            direction = directionMultiplyerX;
            stream.Serialize(ref direction);

            health = HP;
            stream.Serialize(ref health);
        }
        else
        {
            stream.Serialize(ref punch);
            punchSpeed = punch;

            stream.Serialize(ref direction);
            directionMultiplyerX = direction;

            stream.Serialize(ref health);
            HP = health;

        }
    }
}
