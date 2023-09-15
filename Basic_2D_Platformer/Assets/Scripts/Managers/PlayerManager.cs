using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.System
{
    public class PlayerManager : MonoBehaviour
    {
        private GameObject player;
        private Transform playerTransform;

        #region UnityMessages

        private void Awake()
        {
            enabled = false;
            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnEnable()
        {
            // System
            EventManager.Instance.Subscribe(Event.OnLevelGenerated, SpawnPlayer);
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError("PlayerManager not correctly initialized");
            }

            playerTransform = player.transform;
        }

        private void OnDisable()
        {
            // System
            EventManager.Instance.Unsubscribe(Event.OnLevelGenerated, SpawnPlayer);
        }

        #endregion

        #region Listeners

        private void Activate(object[] args)
        {
            enabled = true;
        }

        private void SpawnPlayer(object[] args)
        {
            Vector2 position = (Vector3)args[0];

            player.SetActive(true);
            playerTransform.position = position;

            EventManager.Instance.Publish(Event.OnPlayerSpawn);
        }

        private void RoomChanged(object[] args)
        {
            playerTransform.position = (Vector2)playerTransform.position + ((Vector2)(Vector2Int)args[0]) * 2.5f;
        }

        #endregion
    }
}
