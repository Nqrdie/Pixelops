using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    public class GameManager : NetworkBehaviour
    {


        /// <summary>
        /// Made by Jesper Heese
        /// I only edited this to make it work in my game
        /// </summary>
        /// 

        public static GameManager Instance;
        private int _playerAmount;
        private int _playersLoaded;
        private List<Player> _players = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void LoadInPlayers()
        {
            // should start in the main game scene
            if (IsServer)
            {
                GetAllPlayers();
                Debug.Log("Loading in players");
                NetworkManager.SceneManager.OnLoadComplete += SceneManagerOnOnLoadComplete;
            }
        }

        private void SceneManagerOnOnLoadComplete(
            ulong clientid,
            string scenename,
            LoadSceneMode loadscenemode
        )
        {
            if (scenename != "Main")
                return;

            var playerId = LobbyManager.Instance.ConvertedIds[clientid];
            var playerName = LobbyUtil.GetNameRpc(playerId);
            Debug.Log($"Player {playerName} joined.");
            _playersLoaded++;
            if (_playersLoaded == _playerAmount)
                SpawnInPlayerObjectsRpc();
        }

        private void GetAllPlayers()
        {
            foreach (var player in NetworkManager.Singleton.ConnectedClients)
            {
                _players.Add(player.Value.PlayerObject.GetComponent<Player>());
            }
            _playerAmount = _players.Count;
        }

        [Rpc(SendTo.Everyone)]
        private void SpawnInPlayerObjectsRpc()
        {
            foreach (var player in _players)
            {
                player.SpawnInThisPlayer();
            }
        }
    }
}
