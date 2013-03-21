using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour
{
    public GameObject Player;
    public float distanceToPlayer;
    public float followSpeedForward;
    public float followSpeedSideways;
    public float cameraSpeed;
    public float distance = 0;

	// Update is called once per frame
	void Update () 
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        if (Player == null)
        {
        }
        else
        {
            distance = Vector3.Distance(transform.position, Player.transform.position);
            Vector3 playerFollow = Vector3.zero;

            if (distance > 25)
            {
                playerFollow = new Vector3((Player.transform.position.x - transform.position.x) * followSpeedSideways * Time.deltaTime,
                    (Player.transform.position.y - transform.position.y + 10) * Vector3.forward.y * Time.deltaTime,
                    (Player.transform.position.z - transform.position.z) * Vector3.forward.z * Time.deltaTime);
            }
            else if (distance < 20)
            {
                playerFollow = new Vector3((Player.transform.position.x - transform.position.x) * followSpeedSideways * Time.deltaTime,
                    (Player.transform.position.y - transform.position.y + 5) * followSpeedForward * Time.deltaTime,
                    -(Player.transform.position.z - transform.position.z) * Vector3.forward.z * Time.deltaTime);
            }
            else
            {
                playerFollow = new Vector3((Player.transform.position.x - transform.position.x) * followSpeedSideways * Time.deltaTime,
                   (Player.transform.position.y - transform.position.y + 5) * followSpeedForward * Time.deltaTime,
                   0);
            }

            transform.Translate(playerFollow);
        }
	}
}
