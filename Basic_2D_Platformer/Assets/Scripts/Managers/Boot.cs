using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GMDG.Basic2DPlatformer.System
{
    public class Boot : MonoBehaviour
    {
        #region UnityMessages

        private void Awake()
        {
            LoadSystems();
        }

        private void Start()
        {
            Destroy(GameObject.Find("[Debug Updater]"));
            SceneManager.UnloadSceneAsync("Boot");

            EventManager.Instance.Publish(Event.OnSystemsLoaded);
        }

        #endregion

        private void LoadSystems()
        {
            SceneManager.LoadScene("Player", LoadSceneMode.Additive);
            SceneManager.LoadScene("Managers", LoadSceneMode.Additive);
            SceneManager.LoadScene("Debug", LoadSceneMode.Additive);
        }
    }
}