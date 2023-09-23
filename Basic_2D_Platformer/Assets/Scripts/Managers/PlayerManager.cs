using GMDG.Basic2DPlatformer.PlayerMovement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GMDG.Basic2DPlatformer.System
{
    public class PlayerManager : MonoBehaviour
    {
        private GameObject _player;
        private Transform _playerTransform;

        #region UnityMessages

        private void Awake()
        {
            enabled = false;
            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnEnable()
        {
            // GameManager
            EventManager.Instance.Subscribe(Event.OnWelcome, DespawnPlayer);
            EventManager.Instance.Subscribe(Event.OnGameOver, DespawnPlayer);
            EventManager.Instance.Subscribe(Event.OnVictory, DespawnPlayer);

            // Gameplay
            EventManager.Instance.Subscribe(Event.OnLevelGenerated, SpawnPlayer);
            EventManager.Instance.Subscribe(Event.OnEndGameOverTrasition, DespawnPlayer);
            EventManager.Instance.Subscribe(Event.OnEndVictoryTrasition, DespawnPlayer);
        }

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");

            if (_player == null)
            {
                Debug.LogError("PlayerManager not correctly initialized");
            }

            _playerTransform = _player.transform;
        }

        private void OnDisable()
        {
            // GameManager
            EventManager.Instance.Unsubscribe(Event.OnWelcome, DespawnPlayer);
            EventManager.Instance.Unsubscribe(Event.OnGameOver, DespawnPlayer);
            EventManager.Instance.Unsubscribe(Event.OnVictory, DespawnPlayer);

            // Gameplay
            EventManager.Instance.Unsubscribe(Event.OnLevelGenerated, SpawnPlayer);
            EventManager.Instance.Unsubscribe(Event.OnEndGameOverTrasition, DespawnPlayer);
            EventManager.Instance.Unsubscribe(Event.OnEndVictoryTrasition, DespawnPlayer);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(Event.OnSystemsLoaded, Activate);
        }

        #endregion

        #region Listeners

        private void Activate(object[] args)
        {
            enabled = true;
        }

        private void SpawnPlayer(object[] args)
        {
            Vector2 position = GameObject.FindGameObjectWithTag("Spawn").transform.position;

            _playerTransform.position = position;
            _player.SetActive(true);
        }

        private void DespawnPlayer(object[] args)
        {
            _player.SetActive(false);
        }

        #endregion
    }
}
