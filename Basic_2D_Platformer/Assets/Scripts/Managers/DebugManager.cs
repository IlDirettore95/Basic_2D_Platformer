using GMDG.Basic2DPlatformer.PCG;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.System
{
    public class DebugManager : MonoBehaviour
    {
        [SerializeField] private GameObject _fpsCounter;

        // Grid Debug
        PCGData _data;
        private GameObject _pcgDebugGo;

        #region UnityMessages

        private void Awake()
        {
            enabled = false;
            _fpsCounter.SetActive(false);
            _pcgDebugGo = GameObject.Find("PCG Debug");
            _pcgDebugGo.SetActive(false);

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
            EventManager.Instance.Subscribe(Event.OnPCGUpdated, UpdatePCG);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(Event.OnSystemsLoaded, Activate);
            EventManager.Instance.Unsubscribe(Event.OnPCGUpdated, UpdatePCG);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _fpsCounter.SetActive(!_fpsCounter.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                _pcgDebugGo.SetActive(!_pcgDebugGo.activeSelf);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_pcgDebugGo == null)
            {
                _pcgDebugGo = GameObject.Find("PCG Debug");
            }
            if (_pcgDebugGo.activeSelf && _data != null)
            {
                _data.Grid?.Draw();

                Gizmos.color = Color.green;
                List<Vector2Int> feasiblePath = _data.FeasiblePath;

                if (feasiblePath == null) return;

                foreach(Vector2Int position in feasiblePath)
                {
                    Vector2 worldPosition = _data.Grid.GetPosition(position);
                    Gizmos.DrawSphere(new Vector3(worldPosition.x, worldPosition.y + _data.CellSize.y / 4, -5), 0.5f);
                }
            }
        }
#endif

        #endregion

        #region Listeners

        private void Activate(object[] args)
        {
            enabled = true;
        }

        private void UpdatePCG(object[] args)
        {
            if (!_pcgDebugGo.activeSelf) return;
            _data = (PCGData)args[0];
#if UNITY_EDITOR
            _data?.Grid?.DrawContent(_pcgDebugGo, 20, ColorHeuristic, StringHeuristic);
#endif
        }

        private Color ColorHeuristic(HashSet<int> superPositions)
        {
            if (superPositions.Contains(PCGData.START_CELL) && superPositions.Count == 1)
            {
                return Color.red;
            }
            else if (superPositions.Contains(PCGData.END_CELL) && superPositions.Count == 1) 
            {
                return Color.green;
            }
            else if (superPositions.Contains(PCGData.PASSAGE_CELL) && superPositions.Count == 1)
            {
                return Color.blue;
            }
            else
            {
                return Color.white;
            }
        }

        private string StringHeuristic(HashSet<int> superPositions)
        {
            string text = string.Empty;

            foreach (int pos in superPositions) 
            { 
                text += pos.ToString() + "-";
            }
            if (text.Length > 0) text = text.Remove(text.Length - 1);

            return text;
        }

        #endregion
    }
}
