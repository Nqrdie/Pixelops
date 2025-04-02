using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Player
{
    public class Player : NetworkBehaviour
    {
        private void Awake() { }

        private void Update()
        {
            if (GetComponent<PlayerMovement>().enabled == false)
            {
                CheckScene();
            }
        }
        private void Start()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
            transform.position = new Vector3(0, -10000, 0);
            ParentThisRpc();
            GameObject.DontDestroyOnLoad(transform.parent.gameObject);
        }

        [Rpc(SendTo.Server)]
        private void ParentThisRpc()
        {
            transform.parent = GameObject.Find("PlayerParent").transform;
            LobbyManager.Instance.CheckForPlayersRpc();
        }

        public void OnLeaving()
        {
            Destroy(gameObject);
        }

        private void CheckScene()
        {
            if (SceneManager.GetActiveScene().name == "Main")
            {
                GetComponent<PlayerMovement>().enabled = true;
                GetComponentInChildren<Weapons>().enabled = true;
                transform.position = new Vector3(0, 1, 0);
            }
        }
    }
}
