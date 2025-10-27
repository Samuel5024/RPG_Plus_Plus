using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{

    [Header("Players")]
    public string playerPrefabPath;
    public Transform[] spawnPoints;
    public float respawnTime;
    public PlayerController[] players; // array keeps track of the players

    private int playersInGame;

    // instance
    public static GameManager instance;

    void Awake()
    {
        instance = this;    
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;

        if(playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }
    
    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);    
    }

    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabPath, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        // initialize the player
        playerObj.GetComponent<PhotonView>().RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);

    }
}
