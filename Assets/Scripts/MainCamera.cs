using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour
{
    public GameObject Player;
    public float distanceToPlayer;
    public float followSpeedForward;
    public float followSpeedUp;
    public float followSpeedSideways;
    public float cameraSpeed;
    public float distance = 0;
    private Vector3 cameraStart;
    private string playerName = "<NAME ME>";

    void Start()
    {
        cameraStart = transform.position;
    }
	// Update is called once per frame
	void Update () 
    {
        if (Player == null && playerName == "<NAME ME>")
        {
            transform.position = cameraStart;
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        else if (!Player.renderer.isVisible)
        {
            transform.position = cameraStart;
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
                    (Player.transform.position.y - transform.position.y + 5) * Vector3.forward.y * Time.deltaTime,
                    -(Player.transform.position.z - transform.position.z) * Vector3.forward.z * Time.deltaTime);
            }
            else
            {
                playerFollow = new Vector3((Player.transform.position.x - transform.position.x) * followSpeedSideways * Time.deltaTime,
                   (Player.transform.position.y - transform.position.y + 5) * Vector3.forward.y * Time.deltaTime,
                   0);
            }

            transform.Translate(playerFollow);
        }
	}
}
