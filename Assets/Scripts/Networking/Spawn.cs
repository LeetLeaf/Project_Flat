using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public Transform player;
    Object myPlayer;

    void OnServerInitialized()
    {
        SpawnPlayer();
    }

    void OnConnectedToServer()
    {
        SpawnPlayer();
    }
    public void SpawnPlayer()
    {
        myPlayer = Network.Instantiate(player, transform.position, transform.rotation, 0);
        myPlayer.name = GameObject.Find("Main Camera").GetComponent<MPBase>().playerName;
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Network.RemoveRPCs(Network.player);
        Network.DestroyPlayerObjects(Network.player);
        Application.LoadLevel(Application.loadedLevel);
    }
    
}
