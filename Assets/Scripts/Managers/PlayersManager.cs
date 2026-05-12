using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using PlayerScripts;

namespace Managers
{
    public class PlayersManager : NetworkBehaviour
    {
        public static PlayersManager Instance { get; private set; }

        [SerializeField] Transform player1SpawnPoint;
        [SerializeField] Transform player2SpawnPoint;
        [SerializeField] Transform player3SpawnPoint;
        [SerializeField] Transform player4SpawnPoint;
        List<Player> m_Players;
        int m_PlayersCount;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Create 4 empty player objects in the list
                m_Players = new List<Player>() { null, null, null, null };
                m_PlayersCount = 0;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        //// Start is called once before the first execution of Update after the MonoBehaviour is created
        //void Start()
        //{

        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}

        //private Transform GetPlayerSpawnPoint(int playerID)
        //{
        //    return playerID switch
        //    {
        //        0 => player1SpawnPoint,
        //        1 => player2SpawnPoint,
        //        2 => player3SpawnPoint,
        //        3 => player4SpawnPoint,
        //        _ => throw new ArgumentException("Invalid player ID")
        //    };
        //}

        //public void JoinPlayer(Player player, int playerID)
        //{
        //    // Assign player based on playerID, if playerID is -1, assign to the first empty slot
        //    if (playerID < 0)
        //    {
        //        m_Players[m_PlayersCount] = player;
        //        player.playerID = m_PlayersCount; // Reassign playerID
        //    }
        //    else
        //    {
        //        m_Players[playerID] = player;
        //    }

        //    // Set player position to the corresponding spawn point
        //    player.transform.position = GetPlayerSpawnPoint(player.playerID).position;
        //    player.transform.rotation = GetPlayerSpawnPoint(player.playerID).rotation;
        //    player.transform.localScale = GetPlayerSpawnPoint(player.playerID).localScale;

        //    // Increment player count
        //    m_PlayersCount++;
        //    Debug.Log("Player " + player.playerID + " joined the game. Total players: " + m_PlayersCount);
        //}

        public int GetPlayerCount()
        {
            return m_PlayersCount;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandlePlayerJoined;

                // Manually add the host, since they are already connected when this runs
                //HandlePlayerJoined(NetworkManager.Singleton.LocalClientId);

                //Create 4 empty player objects in the list
                m_Players = new List<Player>() { null, null, null, null };
                m_PlayersCount = 0;
            }

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandlePlayerJoined;
            }
        }

        

        [Rpc(SendTo.Server)]
        public void JoinPlayerRpc(ulong clientId)
        {
            m_Players[m_PlayersCount] = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>();

            // Set player position to the corresponding spawn point
            Player player = m_Players[m_PlayersCount - 1];
            Transform newTransform = GetPlayerSpawnPoint(m_PlayersCount);
            if (IsServer && player != null)
            {
                player.transform.position = newTransform.position;
                player.transform.rotation = newTransform.rotation;
                player.transform.localScale = newTransform.localScale;
            }

            // Increment player count
            m_PlayersCount++;
            Debug.Log("Player " + player.playerID + " joined the game. Total players: " + m_PlayersCount);
        }

        private void HandlePlayerJoined(ulong clientId)
        {
            m_Players[m_PlayersCount] = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>();

            // Set player position to the corresponding spawn point
            Player player = m_Players[m_PlayersCount];
            Transform newTransform = GetPlayerSpawnPoint(m_PlayersCount);
            if (IsServer && player != null)
            {
                player.transform.position = newTransform.position;
                player.transform.rotation = newTransform.rotation;
                player.transform.localScale = newTransform.localScale;
            }

            // Increment player count
            m_PlayersCount++;
            Debug.Log("Player " + player.playerID + " joined the game. Total players: " + m_PlayersCount);
        }

        private Transform GetPlayerSpawnPoint(int playerIndex)
        {
            return playerIndex switch
            {
                0 => player1SpawnPoint,
                1 => player2SpawnPoint,
                2 => player3SpawnPoint,
                3 => player4SpawnPoint,
                _ => throw new ArgumentException("Invalid player ID")
            };
        }
    }
}