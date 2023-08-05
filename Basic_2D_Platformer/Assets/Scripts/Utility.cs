using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GMDG.NoProduct.Utility
{
    #region DesignPatterns

    #region ObjectPooling

    public class ObjectPool
    {
        private GameObject[] pool;
        private GameObject father;

        public ObjectPool(GameObject prefab, GameObject parent, int capacity)
        {
            pool = new GameObject[capacity];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = GameObject.Instantiate(prefab, parent.transform);
                pool[i].SetActive(false);
            }
        }

        public GameObject Take()
        {
            for (int i = 0; i < pool.Length; i++)
            {
                if (!pool[i].activeInHierarchy)
                {
                    return pool[i];
                }
            }
            return null;
        }

        public void Return(GameObject gameObject)
        {
            if (!IsObjectInPool(gameObject))
            {
                Debug.LogError("Game Object in wrong ObjectPool");
            }

            gameObject.SetActive(false);
        }

        public bool IsObjectInPool(GameObject gameObject)
        {
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i] == gameObject) return true;
            }

            return false;
        }
    }

    #endregion

    #region Singleton+Monobehaviour

    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        protected void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Instance of this singleton " + (T)this + " already exists, deleting!");
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                instance = (T)this;
            }
        }
    }

    #endregion

    #region Observer-Pub/Sub

    public class EventManager
    {
        private static EventManager instance;
        public static EventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventManager();
                }
                return instance;
            }
        }

        private Dictionary<Event, Action<object[]>> listenersDictionary = new Dictionary<Event, Action<object[]>>();

        private EventManager() { }

        public void Subscribe(Event eventName, Action<object[]> listener)
        {
            if (!listenersDictionary.ContainsKey(eventName))
            {
                listenersDictionary[eventName] = listener;
            }
            else
            {
                listenersDictionary[eventName] += listener;
            }
        }

        public void Unsubscribe(Event eventName, Action<object[]> listener)
        {
            if (listenersDictionary.ContainsKey(eventName))
            {
                listenersDictionary[eventName] -= listener;
            }
        }

        public void Publish(Event eventName, params object[] args)
        {
            if (listenersDictionary.ContainsKey(eventName))
            {
                listenersDictionary[eventName]?.Invoke(args);
            }
        }
    }

    public enum Event
    {
        OnSystemsLoaded,
        OnMenu,
        OnGameplay,
        OnPause,
        OnUnpause,
        OnGameOver,
        OnVictory
    }

    #endregion

    #region State

    public class StateMachine
    {
        private IState _currentState;

        private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
        private List<Transition> _currentTransitions = new List<Transition>();
        private List<Transition> _anyTransitions = new List<Transition>();

        private static List<Transition> EmptyTransitions = new List<Transition>(0);

        public void Tick()
        {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);

            _currentState?.Tick();
        }

        public IState GetState()
        {
            return _currentState;
        }

        public void SetState(IState state)
        {
            if (state == _currentState)
                return;

            _currentState?.OnExit();
            _currentState = state;

            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null)
                _currentTransitions = EmptyTransitions;

            _currentState.OnEnter();
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (_transitions.TryGetValue(from.GetType(), out var transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;
            }

            transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState state, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(state, predicate));
        }

        private class Transition
        {
            public Func<bool> Condition { get; }
            public IState To { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private Transition GetTransition()
        {
            foreach (var transition in _anyTransitions)
                if (transition.Condition())
                    return transition;

            foreach (var transition in _currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;
        }
    }

    public interface IState
    {
        void Tick();
        void OnEnter();
        void OnExit();
    }

    #endregion

    #endregion

    #region 3D

    #region Movement 2.5D

    /// <summary>
    /// Class <c>Static</c> models static character data (position and orientation).
    /// </summary>
    public class Static
    {
        private Transform transform;
        public Transform Transform
        {
            get { return transform; }
        }
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        public float Orientation
        {
            get { return transform.rotation.eulerAngles.y; }
            set { transform.rotation = Quaternion.Euler(0f, value, 0f); }
        }

        public Static(Transform transform)
        {
            this.transform = transform;
        }
    }

    /// <summary>
    /// Class <c>Kinematic</c> models dynamic character data (velocity and angular velocity).
    /// </summary>
    public class Kinematic
    {
        private Static staticInfo;
        private Vector3 velocity;
        private float angularVelocity;
        public Transform Transform
        {
            get { return staticInfo.Transform; }
        }
        public Vector3 Position
        {
            get { return staticInfo.Position; }
            set { staticInfo.Position = value; }
        }
        public float Orientation
        {
            get { return staticInfo.Orientation; }
            set { staticInfo.Orientation = value; }
        }
        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        public float AngularVelocity
        {
            get { return angularVelocity; }
            set { angularVelocity = value; }
        }

        public Static Static
        {
            get { return staticInfo; }
        }

        public Kinematic(Transform transform)
        {
            staticInfo = new Static(transform);
            velocity = Vector3.zero;
            angularVelocity = 0f;
        }

        public static void Update(Kinematic kinematic, KinematicSteeringOutput steering)
        {
            kinematic.Position += steering.velocity * Time.deltaTime;
            kinematic.Orientation = steering.rotation;
            kinematic.Velocity = steering.velocity;
        }

        public static void Update(Kinematic kinematic, SteeringOutput steering)
        {
            // Update the position and orientation.
            float halfTimeSqrt = 0.5f * Time.deltaTime * Time.deltaTime;
            kinematic.Position += kinematic.Velocity * Time.deltaTime + steering.linear * halfTimeSqrt;
            kinematic.Orientation += kinematic.AngularVelocity * Time.deltaTime + steering.angular * halfTimeSqrt;

            //  and the velocity and rotation.
            kinematic.Velocity += steering.linear * Time.deltaTime;
            kinematic.AngularVelocity += steering.angular * Time.deltaTime;
        }

        public static float newOrientation(float currentOrientation, Vector3 velocity)
        {
            //  Make sure we have a velocity.
            if (velocity.magnitude > 0)
            {
                // Calculate orientation from the velocity.
                return Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
            }
            else
            {
                // Otherwise use the current orientation.
                return currentOrientation;
            }
        }

        public static float newOrientationLerp(float currentOrientation, Vector3 velocity, float delta)
        {
            //  Make sure we have a velocity.
            if (velocity.magnitude > 0)
            {
                float angle = Mathf.DeltaAngle(currentOrientation, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg);

                if (Mathf.Abs(angle) < delta * Time.deltaTime)
                {
                    return Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
                }

                // Calculate orientation from the velocity.
                return currentOrientation + Mathf.Sign(angle) * delta * Time.deltaTime;
            }
            else
            {
                // Otherwise use the current orientation.
                return currentOrientation;
            }
        }

        public static KinematicSteeringOutput KinematicSeek(Static characher, Static target, float maxSpeed)
        {
            Vector3 velocity;
            float rotation;

            // Get the direction to the target.
            velocity = target.Position - characher.Position;

            // The velocity is along this direction, at full speed.
            velocity = velocity.normalized * maxSpeed;

            // Face in the direction we want to move
            characher.Orientation = newOrientation(characher.Orientation, velocity);

            rotation = 0f;

            return new KinematicSteeringOutput(velocity, rotation);
        }
    }

    #region Kinematic Movement

    /// <summary>
    /// Struct <c>KinematicSteeringOutput</c> holds new volocity and angular velocity for a kinematic movement algorithm.
    /// </summary>
    public struct KinematicSteeringOutput
    {
        public readonly Vector3 velocity;
        public readonly float rotation;

        public KinematicSteeringOutput(Vector3 velocity, float rotation)
        {
            this.velocity = velocity;
            this.rotation = rotation;
        }
    }

    #endregion

    #region Steering(Dynamic) Movement

    /// <summary>
    /// Struct <c>SteeringOutput</c> holds new linear acceleration and angular acceleration for a dynamic movement algorithm.
    /// </summary>
    public struct SteeringOutput
    {
        public readonly Vector3 linear;
        public readonly float angular;

        public SteeringOutput(Vector3 linear, float angular)
        {
            this.linear = linear;
            this.angular = angular;
        }
    }

    #endregion

    #endregion

    #endregion

    #region 2D

    #region Movement 2D

    /// <summary>
    /// Class <c>Static</c> models static character data (position and orientation).
    /// </summary>
    public class Static2D
    {
        private Transform transform;

        public Transform Transform
        {
            get { return transform; }
        }

        public Vector2 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        public float Orientation
        {
            get { return transform.rotation.eulerAngles.y; }
            set { transform.rotation = Quaternion.Euler(0f, value, 0f); }
        }

        public Static2D(Transform transform)
        {
            this.transform = transform;
        }
    }

    /// <summary>
    /// Class <c>Kinematic</c> models dynamic character data (velocity and angular velocity).
    /// </summary>
    public class Kinematic2D
    {
        private Static2D staticInfo;
        private Vector2 velocity;
        private float angularVelocity;

        public Transform Transform
        {
            get { return staticInfo.Transform; }
        }
        public Vector2 Position
        {
            get { return staticInfo.Position; }
            set { staticInfo.Position = value; }
        }
        public float Orientation
        {
            get { return staticInfo.Orientation; }
            set { staticInfo.Orientation = value; }
        }
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        public float AngularVelocity
        {
            get { return angularVelocity; }
            set { angularVelocity = value; }
        }

        public Static2D Static
        {
            get { return staticInfo; }
        }

        public Kinematic2D(Transform transform)
        {
            staticInfo = new Static2D(transform);
            velocity = Vector3.zero;
            angularVelocity = 0f;
        }

        public static void Update(Kinematic2D kinematic, KinematicSteeringOutput2D steering)
        {
            kinematic.Position += steering.velocity * Time.deltaTime;
            kinematic.Orientation = steering.rotation;
            kinematic.Velocity = steering.velocity;
        }
    }

    #region Kinematic Movement

    /// <summary>
    /// Struct <c>KinematicSteeringOutput</c> holds new volocity and angular velocity for a kinematic movement algorithm.
    /// </summary>
    public struct KinematicSteeringOutput2D
    {
        public readonly Vector2 velocity;
        public readonly float rotation;

        public KinematicSteeringOutput2D(Vector2 velocity, float rotation)
        {
            this.velocity = velocity;
            this.rotation = rotation;
        }
    }

    #endregion

    #endregion

    public class Utility2D
    {
        public class Grid2D
        {
            public Vector2[,] CellsPositions { get; private set; }
            public int YLength { get { return CellsPositions.GetLength(0); } }
            public int XLength { get { return CellsPositions.GetLength(1); } }

            private Cell2D[,] cells;
            private Vector2 gridPosition;

            public Grid2D(int y, int x, Vector2 cellSize, Vector2 gridPosition)
            {
                cells = new Cell2D[y, x];
                CellsPositions = new Vector2[y, x];
                this.gridPosition = gridPosition;

                float yTranslation = (y - 1) * cellSize.y / 2;
                float xTranslation = (x - 1) * cellSize.x / 2;

                for (int i = 0; i < y; i++)
                {
                    for (int j = 0; j < x; j++)
                    {
                        Vector2 cellPosition = new Vector2(j * cellSize.x - xTranslation, i * cellSize.y - yTranslation) + gridPosition;
                        cells[i, j] = new Cell2D(cellSize, cellPosition, new Vector2Int(i, j));
                        CellsPositions[i, j] = cellPosition;
                    }
                }
            }

            public void Draw()
            {
                for (int i = 0; i < cells.GetLength(0); i++)
                {
                    for (int j = 0; j < cells.GetLength(1); j++)
                    {
                        cells[i, j].Draw();
                    }
                }
            }

            private class Cell2D
            {
                public Vector2 size;
                public Vector2 positionInWorld;
                public Vector2 positionInGrid;

                public Cell2D(Vector2 positionInWorld, Vector2Int positionInGrid)
                {
                    size = new Vector2(1, 1);
                    this.positionInWorld = positionInWorld;
                    this.positionInGrid = positionInGrid;
                }

                public Cell2D(Vector2 size, Vector2 positionInWorld, Vector2Int positionInGrid)
                {
                    this.size = size;
                    this.positionInWorld = positionInWorld;
                    this.positionInGrid = positionInGrid;
                }

                public void Draw()
                {
                    Debug.DrawLine(positionInWorld + new Vector2(-size.x / 2, size.y / 2), positionInWorld + new Vector2(size.x / 2, size.y / 2), Color.black, float.PositiveInfinity);
                    Debug.DrawLine(positionInWorld + new Vector2(size.x / 2, size.y / 2), positionInWorld + new Vector2(size.x / 2, -size.y / 2), Color.black, float.PositiveInfinity);
                    Debug.DrawLine(positionInWorld + new Vector2(size.x / 2, -size.y / 2), positionInWorld + new Vector2(-size.x / 2, -size.y / 2), Color.black, float.PositiveInfinity);
                    Debug.DrawLine(positionInWorld + new Vector2(-size.x / 2, -size.y / 2), positionInWorld + new Vector2(-size.x / 2, size.y / 2), Color.black, float.PositiveInfinity);
                }
            }
        }

        public enum Direction2D
        {
            NORTH,
            EAST,
            SOUTH,
            WEST
        }

        public static Dictionary<Direction2D, Vector2Int> VectorsDirections2D = new Dictionary<Direction2D, Vector2Int>()
    {
        { Direction2D.NORTH, Vector2Int.up },
        { Direction2D.EAST, Vector2Int.right },
        { Direction2D.SOUTH, Vector2Int.down },
        { Direction2D.WEST, Vector2Int.left }
    };

        public static Dictionary<Direction2D, Vector2Int> GridDirections2D = new Dictionary<Direction2D, Vector2Int>()
    {
        { Direction2D.NORTH, new Vector2Int(-1, 0) },
        { Direction2D.EAST, new Vector2Int(0, 1) },
        { Direction2D.SOUTH, new Vector2Int(1, 0) },
        { Direction2D.WEST, new Vector2Int(0, -1) }
    };
    }

    #region SpriteAnimation

    public class SpriteUtility
    {
        public static IEnumerator FlickeringAnimation(SpriteRenderer spriteRenderer, float duration, float deltaDuration)
        {
            Color initial = spriteRenderer.color;
            float timeCount = 0f;
            float deltaTimeCount = 0f;

            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);

            while (timeCount < duration)
            {
                if (deltaTimeCount >= deltaDuration)
                {
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1 - spriteRenderer.color.a);
                    deltaTimeCount = deltaTimeCount - deltaDuration;
                }
                deltaTimeCount += Time.deltaTime;
                timeCount += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }
    }

    #endregion

    #endregion

    #region Gizmos

    public static class ArrowGizmo
    {
        public static void ForGizmo(Vector3 pos, Vector3 direction, Color? color = null, bool doubled = false, float arrowHeadLength = 0.2f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color ?? Color.white;

            //arrow shaft
            Gizmos.DrawRay(pos, direction);

            if (direction != Vector3.zero)
            {
                //arrow head
                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
                Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
            }
        }
    }

    #endregion

    #region General

    public class GameObjectUtility
    {
        public static IEnumerator FlickeringBehaviour(Behaviour behaviour, float duration, float deltaDuration)
        {
            float timeCount = 0f;
            float deltaTimeCount = 0f;

            behaviour.enabled = false;

            while (timeCount < duration)
            {
                if (deltaTimeCount >= deltaDuration)
                {
                    behaviour.enabled = !behaviour.enabled;
                    deltaTimeCount = deltaTimeCount - deltaDuration;
                }
                deltaTimeCount += Time.deltaTime;
                timeCount += Time.deltaTime;
                yield return null;
            }

            behaviour.enabled = true;
        }
    }

    #endregion

    #region Math

    public class MathUtility
    {
        public static bool IsValueBetween(float value, float bound1, float bound2)
        {
            if (bound1 > bound2) throw new ArgumentException();

            return value >= bound1 && value <= bound2;
        }
    }

    #endregion
}
