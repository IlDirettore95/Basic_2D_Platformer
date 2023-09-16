using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GMDG.Basic2DPlatformer.System
{
    public class CameraManager : MonoBehaviour
    {
        private Camera mainCamera;
        private Transform mainCameraTransform;
        private Light2D globalLight;

        private Vector3 initialCameraPosition;
        private float cameraMovingTime = 0.2f;
        private float cameraMovingSpeed = 150;

        private IEnumerator transitionCamera;

        #region UnityMessages

        private void Awake()
        {
            globalLight = GetComponentInChildren<Light2D>();

            enabled = false;

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnEnable()
        {
            //// Game Manager
            //EventManager.Instance.Subscribe(Event.OnWelcome, (object[] args) => mainCameraTransform.position = initialCameraPosition);
            //EventManager.Instance.Subscribe(Event.OnGameplay, (object[] args) => StartCoroutine(GLSpawnTransition()));
            //EventManager.Instance.Subscribe(Event.OnPlayerDeath, (object[] args) => StartCoroutine(GLDeathTransition()));
            //EventManager.Instance.Subscribe(Event.OnDungeonCleared, (object[] args) => StartCoroutine(GLVictoryTransition()));

            //// Gameplay
            //EventManager.Instance.Subscribe(Event.OnRoomChanged, RoomChanghed);
        }

        private void Start()
        {
            mainCamera = Camera.main;

            if (mainCamera == null || globalLight == null)
            {
                Debug.LogError("CameraManager not correctly initialized");
            }

            mainCameraTransform = Camera.main.transform;
            initialCameraPosition = mainCameraTransform.position;
        }

        private void OnDisable()
        {
            // Game Manager
            //EventManager.Instance.Unsubscribe(Event.OnWelcome, (object[] args) => mainCameraTransform.position = initialCameraPosition);
            //EventManager.Instance.Unsubscribe(Event.OnGameplay, (object[] args) => StartCoroutine(GLSpawnTransition()));
            //EventManager.Instance.Unsubscribe(Event.OnGameOver, (object[] args) => StartCoroutine(GLDeathTransition()));
            //EventManager.Instance.Unsubscribe(Event.OnVictory, (object[] args) => StartCoroutine(GLVictoryTransition()));

            //// Gameplay
            //EventManager.Instance.Unsubscribe(Event.OnRoomChanged, RoomChanghed);
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

        //private void RoomChanghed(object[] args)
        //{
        //    if(transitionCamera != null)
        //    {
        //        StopCoroutine(transitionCamera);
        //    }
        //    transitionCamera = TransitionCameraToDestination((Vector2)mainCameraTransform.position + (Vector2Int)args[0] * Room.Size);
        //    StartCoroutine(transitionCamera);
        //}

        #endregion

        private IEnumerator TransitionCameraToDestination(Vector2 destination)
        {
            float distance = Vector2.Distance((Vector2)mainCameraTransform.position, destination);
            while ((Vector2)mainCameraTransform.position != destination)
            {
                Vector2 movement = Vector2.MoveTowards(mainCameraTransform.position, destination, distance/cameraMovingTime * Time.deltaTime);
                MoveCameraToDestination(movement);
                yield return null;
            }
            transitionCamera = null;
        }

        private void MoveCameraToDestination(Vector2 destination)
        {
            mainCameraTransform.position = new Vector3(destination.x, destination.y, mainCameraTransform.position.z);
        }

        private IEnumerator GLSpawnTransition()
        {
            float k = 10000f;
            float x = 0f;

            while(true)
            {
                globalLight.intensity = Mathf.Pow(k, -x + 1);
                if(globalLight.intensity < 1)
                {
                    globalLight.intensity = 1f;
                    break;
                }
                x += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator GLVictoryTransition()
        {
            float k = 10000f;
            float x = 0f;

            while (true)
            {
                globalLight.intensity = Mathf.Pow(k, x);
                if (globalLight.intensity > 10000f)
                {
                    globalLight.intensity = 10000f;
                    break;
                }
                x += Time.deltaTime;
                yield return null;
            }

            EventManager.Instance.Publish(Event.OnEndVictoryTrasition);
        }

        private IEnumerator GLDeathTransition()
        {
            float rate = 0.4f;
            globalLight.intensity = 1f;

            while (true)
            {
                globalLight.intensity -= rate * Time.deltaTime;
                if (globalLight.intensity <= 0)
                {
                    break;
                }
                yield return null;
            }

            EventManager.Instance.Publish(Event.OnEndGameOverTrasition);
        }
    }
}
