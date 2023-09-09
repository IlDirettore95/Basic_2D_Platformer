using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
        public class Grid2D<T>
        {
            public Vector2[,] CellsPositions { get; }
            public Vector2Int GridSize { get; }
            public Vector2 CellSize { get; }
            public Vector2 GridPosition { get; }

            private Cell2D<T>[,] cells;

            public Grid2D(Vector2Int gridSize, Vector2 cellSize, Vector2 gridPosition)
            {
                GridSize = gridSize;
                CellSize = cellSize;
                GridPosition = gridPosition;

                cells = new Cell2D<T>[gridSize.y, gridSize.x];
                CellsPositions = new Vector2[gridSize.y, gridSize.x];

                float yTranslation = (gridSize.y - 1) * cellSize.y / 2;
                float xTranslation = (gridSize.x - 1) * cellSize.x / 2;

                for (int i = 0; i < gridSize.y; i++)
                {
                    for (int j = 0; j < gridSize.x; j++)
                    {
                        Vector2 cellPosition = new Vector2(j * cellSize.x - xTranslation, i * cellSize.y - yTranslation) + gridPosition;
                        cells[i, j] = new Cell2D<T>(cellSize, cellPosition, new Vector2Int(i, j));
                        cells[i, j].Content = default(T);
                        CellsPositions[i, j] = cellPosition;
                    }
                }
            }

            public Vector2 GetPosition(int i, int j)
            {
                if (i < 0 || j < 0 || i > GridSize.y - 1 || j > GridSize.x - 1)
                {
                    throw new ArgumentException();
                }

                return CellsPositions[i, j];
            }

            public Vector2Int GetIndicies(Vector2 position)
            {
                for (int i = 0; i < CellsPositions.GetLength(0); i++)
                {
                    for (int j = 0; j < CellsPositions.GetLength(1); j++)
                    {
                        if (CellsPositions[i, j] == position) return new Vector2Int(i, j);
                    }
                }

                return new Vector2Int(-1, -1);
            }

            public T GetElement(int i, int j)
            {
                if (i < 0 || j < 0 || i > GridSize.y - 1 || j > GridSize.x - 1)
                {
                    throw new ArgumentException();
                }

                return cells[i, j].Content;
            }

            public void PlaceElement(int i, int j, T content)
            {
                if (i < 0 || j < 0 || i > GridSize.y - 1 || j > GridSize.x - 1)
                {
                    throw new ArgumentException();
                }

                cells[i, j].Content = content;
            }

            public void PlaceElement(Vector2Int indicies, T content)
            {
                if (indicies.y < 0 || indicies.x < 0 || indicies.y > GridSize.y - 1 || indicies.x > GridSize.x - 1)
                {
                    throw new ArgumentException();
                }

                cells[indicies.y, indicies.x].Content = content;
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

            public void DrawContent(GameObject parent, Func<T,Color> colorHeuristic)
            {
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    GameObject.Destroy(parent.transform.GetChild(i).gameObject);
                }

                for (int i = 0; i < GridSize.y; i++)
                {
                    for (int j = 0; j < GridSize.x; j++)
                    {
                        cells[i, j].DrawContent(parent, colorHeuristic);
                    }
                }
            }

            private class Cell2D<S>
            {
                public Vector2 Size;
                public Vector2 PositionInWorld;
                public Vector2 PositionInGrid;
                public S Content;

                public Cell2D(Vector2 positionInWorld, Vector2Int positionInGrid)
                {
                    Size = new Vector2(1, 1);
                    PositionInWorld = positionInWorld;
                    PositionInGrid = positionInGrid;
                }

                public Cell2D(Vector2 size, Vector2 positionInWorld, Vector2Int positionInGrid)
                {
                    Size = size;
                    PositionInWorld = positionInWorld;
                    PositionInGrid = positionInGrid;
                }

                public void Draw()
                {
                    Debug.DrawLine(PositionInWorld + new Vector2(-Size.x / 2, Size.y / 2), PositionInWorld + new Vector2(Size.x / 2, Size.y / 2), Color.black);
                    Debug.DrawLine(PositionInWorld + new Vector2(Size.x / 2, Size.y / 2), PositionInWorld + new Vector2(Size.x / 2, -Size.y / 2), Color.black);
                    Debug.DrawLine(PositionInWorld + new Vector2(Size.x / 2, -Size.y / 2), PositionInWorld + new Vector2(-Size.x / 2, -Size.y / 2), Color.black);
                    Debug.DrawLine(PositionInWorld + new Vector2(-Size.x / 2, -Size.y / 2), PositionInWorld + new Vector2(-Size.x / 2, Size.y / 2), Color.black);
                }

                public void DrawContent(GameObject parent, Func<S, Color> colorHeuristic)
                {
                    Color color = colorHeuristic.Invoke(Content);
                    TextUtility.CreateWorldText(Content.ToString(), 6, PositionInWorld, color, parent.transform);
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

        public static List<Direction2D> Directions2D = new List<Direction2D>()
        { 
            Direction2D.NORTH,
            Direction2D.EAST,
            Direction2D.SOUTH,
            Direction2D.WEST
        };

        public static Dictionary<Direction2D, Direction2D> OppositeDirections = new Dictionary<Direction2D, Direction2D>()
        {
            { Direction2D.NORTH, Direction2D.SOUTH },
            { Direction2D.EAST, Direction2D.WEST },
            { Direction2D.SOUTH, Direction2D.NORTH },
            { Direction2D.WEST, Direction2D.EAST }
        };

        public static Dictionary<string, Direction2D> Directions2DString = new Dictionary<string, Direction2D>()
        {
            { "NORTH", Direction2D.NORTH },
            { "EAST", Direction2D.EAST },
            { "SOUTH", Direction2D.SOUTH },
            { "WEST", Direction2D.WEST },
        };

        public static Dictionary<Direction2D, Vector2Int> VectorsDirections2D = new Dictionary<Direction2D, Vector2Int>()
        {
            { Direction2D.NORTH, Vector2Int.up },
            { Direction2D.EAST, Vector2Int.right },
            { Direction2D.SOUTH, Vector2Int.down },
            { Direction2D.WEST, Vector2Int.left }
        };

        public static Dictionary<Direction2D, Vector2Int> GridDirections2D = new Dictionary<Direction2D, Vector2Int>()
        {
            { Direction2D.NORTH, new Vector2Int(1, 0) },
            { Direction2D.EAST, new Vector2Int(0, 1) },
            { Direction2D.SOUTH, new Vector2Int(-1, 0) },
            { Direction2D.WEST, new Vector2Int(0, -1) }
        };
    }

    #region Text

    public class TextUtility
    {
        public static GameObject CreateWorldText(string text, int fontSize, Vector3 position, Color color, Transform parent)
        {
            GameObject go = new GameObject("WorldText", typeof(TextMeshPro));
            go.transform.position = position;
            go.transform.SetParent(parent);
            TextMeshPro textComponent = go.GetComponent<TextMeshPro>();
            textComponent.text = text;
            textComponent.color = color;
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAlignmentOptions.Center;

            return go;
        }
    }

    #endregion

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