using GMDG.Basic2DPlatformer.PCG;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GMDG.Basic2DPlatformer.System
{
    public class CameraManager : MonoBehaviour
    {
        private Camera _mainCamera;
        private Transform _mainCameraTransform;
        private GameObject _player;
        private Transform _playerTransform;
        private Light2D _globalLight;

        private float _minXPosition;
        private float _maxXPosition;
        private float _minYPosition;
        private float _maxYPosition;

        private float _screenHeight;
        private float _screenWidth;

        #region UnityMessages

        private void Awake()
        {
            _globalLight = GetComponentInChildren<Light2D>();

            enabled = false;

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnEnable()
        {
            //// Game Manager
            EventManager.Instance.Subscribe(Event.OnPlayerSpawn, GLSpawnTransition);
            EventManager.Instance.Subscribe(Event.OnPlayerDeath, GLDeathTransition);
            EventManager.Instance.Subscribe(Event.OnAllLevelCompleted, GLVictoryTransition);

            // Gameplay
            EventManager.Instance.Subscribe(Event.OnLevelGenerated, LevelLoaded);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            _player = GameObject.FindGameObjectWithTag("Player");

            if (_mainCamera == null || _globalLight == null || _player == null)
            {
                Debug.LogError("CameraManager not correctly initialized");
            }

            _mainCameraTransform = _mainCamera.transform;
            _playerTransform = _player.transform;

            _screenHeight = _mainCamera.orthographicSize * 2;
            _screenWidth = _screenHeight * _mainCamera.aspect;
        }

        private void OnDisable()
        {
            // Game Manager
            EventManager.Instance.Unsubscribe(Event.OnPlayerSpawn, GLSpawnTransition);
            EventManager.Instance.Unsubscribe(Event.OnPlayerDeath, GLDeathTransition);
            EventManager.Instance.Unsubscribe(Event.OnAllLevelCompleted, GLVictoryTransition);

            // Gameplay
            EventManager.Instance.Unsubscribe(Event.OnLevelGenerated, LevelLoaded);
        }

        private void LateUpdate()
        {
            float xPosition = Mathf.Clamp(_playerTransform.position.x, _minXPosition, _maxXPosition);
            float yPosition = Mathf.Clamp(_playerTransform.position.y, _minYPosition, _maxYPosition);
            Vector2 destination = new Vector2(xPosition, yPosition);

            MoveCameraToDestination(destination);
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

        private void LevelLoaded(object[] args)
        {
            PCGData data = (PCGData)args[0];
            Vector2Int gridSize = data.GridSize;
            Vector2 cellSize = data.CellSize;

            float xExtend = gridSize.x * cellSize.x / 2;
            float yExtend = gridSize.y * cellSize.y / 2;

            _minXPosition = -xExtend + _screenWidth / 2;
            _maxXPosition = xExtend - _screenWidth / 2;
            _minYPosition = -yExtend + _screenHeight / 2;
            _maxYPosition = yExtend - _screenHeight / 2;
        }

        private void GLSpawnTransition(object[] args)
        {
            StartCoroutine(GLSpawnTransition());
        }

        private void GLDeathTransition(object[] args)
        {
            StartCoroutine(GLDeathTransition());
        }

        private void GLVictoryTransition(object[] args)
        {
            StartCoroutine(GLVictoryTransition());
        }

        #endregion

        private void MoveCameraToDestination(Vector2 destination)
        {
            _mainCameraTransform.position = new Vector3(destination.x, destination.y, _mainCameraTransform.position.z);
        }

        private IEnumerator GLSpawnTransition()
        {
            float k = 10000f;
            float x = 0f;

            while(true)
            {
                _globalLight.intensity = Mathf.Pow(k, -x + 1);
                if(_globalLight.intensity < 1)
                {
                    _globalLight.intensity = 1f;
                    break;
                }
                x += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator GLDeathTransition()
        {
            float rate = 0.4f;
            _globalLight.intensity = 1f;

            while (true)
            {
                _globalLight.intensity -= rate * Time.deltaTime;
                if (_globalLight.intensity <= 0)
                {
                    break;
                }
                yield return null;
            }

            EventManager.Instance.Publish(Event.OnEndGameOverTransition);
        }
        
        private IEnumerator GLVictoryTransition()
        {
            float k = 10000f;
            float x = 0f;

            while (true)
            {
                _globalLight.intensity = Mathf.Pow(k, x);
                if (_globalLight.intensity > 10000f)
                {
                    _globalLight.intensity = 10000f;
                    break;
                }
                x += Time.deltaTime;
                yield return null;
            }

            EventManager.Instance.Publish(Event.OnEndVictoryTransition);
        }
    }
}
