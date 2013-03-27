using UnityEngine;
using System.Collections;

public class DeathTrigger : MonoBehaviour 
{
    public Transform respawn;
    public bool timing;
    private float countdown;
    private GameObject player;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            player = collider.gameObject;
            StartTimer(3);
        }
    }

    void StartTimer(float time)
    {
        timing = true;
        countdown = time;

    }


    void Update()
    {
        if (timing)
        {
            countdown -= Time.deltaTime;

            if (countdown <= 0)
            {
                player.transform.position = respawn.position;
                timing = false;
            }
        }
    }
}
