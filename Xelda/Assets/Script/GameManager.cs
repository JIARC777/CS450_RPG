using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class GameManager : MonoBehaviourPun
{
    [Header("Player")]
    public string playerPrefabPath;
    public Transform[] spawnPoints;
    public float respawnTime;
    private int playersInGame;

    public static GameManager instance;
    public PlayerController[] players;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log(players[i]);
        }
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            Debug.Log("Player Spawned");
            //Debug.Log(PhotonNetwork.PlayerList.Length);
            SpawnPlayer();
        }
            
            
    }
    // Update is called once per frame
    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabPath, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        //TO DO: Initialize Player;
        playerObj.GetComponent<PhotonView>().RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
}
