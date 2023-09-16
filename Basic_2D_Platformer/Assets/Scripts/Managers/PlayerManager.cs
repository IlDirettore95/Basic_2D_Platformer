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
        private Movement _playerMovement;
        private SpriteRenderer _playerRenderer;

        #region UnityMessages

        private void Awake()
        {
            enabled = false;
            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnEnable()
        {
            // Gameplay
            EventManager.Instance.Subscribe(Event.OnLevelGenerated, SpawnPlayer);
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
            _playerMovement = _player.GetComponent<Movement>();
            _playerRenderer = _player.GetComponent<SpriteRenderer>();

            DespawnPlayer(null);
        }

        private void OnDisable()
        {
            // Gameplay
            EventManager.Instance.Unsubscribe(Event.OnLevelGenerated, SpawnPlayer);
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
            _playerMovement.enabled = true;
            _playerRenderer.enabled = true;
        }

        private void DespawnPlayer(object[] args)
        {
            _playerMovement.enabled = false;
            _playerRenderer.enabled = false;
        }

        #endregion
    }
}
