using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.System
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject welcomeMainMenu;
        [SerializeField] private GameObject choicesMainMenu;
        [SerializeField] private GameObject instructionsMainMenu;

        [SerializeField] private GameObject gameOver;

        [SerializeField] private GameObject victory;

        [SerializeField] private GameObject pause;

        #region UnityMessages

        private void Awake()
        {
            enabled = false;

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
            EventManager.Instance.Subscribe(Event.OnWelcome, ActiveWelcome);
            EventManager.Instance.Subscribe(Event.OnMainMenu, ActiveChoices);
            EventManager.Instance.Subscribe(Event.OnGameplay, DeactiveMainMenu);
            EventManager.Instance.Subscribe(Event.OnPause, ActivePause);
            EventManager.Instance.Subscribe(Event.OnUnpause, DeactivatePause);
            EventManager.Instance.Subscribe(Event.OnEndGameOverTransition, ActiveGameOver);
            EventManager.Instance.Subscribe(Event.OnEndVictoryTransition, ActiveVictory);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(Event.OnSystemsLoaded, Activate);
            EventManager.Instance.Unsubscribe(Event.OnWelcome, ActiveWelcome);
            EventManager.Instance.Unsubscribe(Event.OnMainMenu, ActiveChoices);
            EventManager.Instance.Unsubscribe(Event.OnGameplay, DeactiveMainMenu);
            EventManager.Instance.Unsubscribe(Event.OnPause, ActivePause);
            EventManager.Instance.Unsubscribe(Event.OnUnpause, DeactivatePause);
            EventManager.Instance.Unsubscribe(Event.OnEndGameOverTransition, ActiveGameOver);
            EventManager.Instance.Unsubscribe(Event.OnEndVictoryTransition, ActiveVictory);
        }

        #endregion

        #region Listener

        private void Activate(object[] args)
        {
            enabled = true;
        }

        private void ActiveWelcome(object[] args)
        {
            mainMenu.SetActive(true);
            welcomeMainMenu.SetActive(true);

            choicesMainMenu.SetActive(false);
            instructionsMainMenu.SetActive(false);
            gameOver.SetActive(false);
            victory.SetActive(false);
            pause.SetActive(false);
        }

        private void ActiveChoices(object[] args)
        {
            mainMenu.SetActive(true);
            choicesMainMenu.SetActive(true);

            welcomeMainMenu.SetActive(false);
            instructionsMainMenu.SetActive(false);
            gameOver.SetActive(false);
            victory.SetActive(false);
            pause.SetActive(false);
        }

        private void ActiveGameOver(object[] args)
        {
            gameOver.SetActive(true);

            mainMenu.SetActive(false);
            victory.SetActive(false);
            pause.SetActive(false);
        }

        private void ActiveVictory(object[] args)
        {
            victory.SetActive(true);

            mainMenu.SetActive(false);
            gameOver.SetActive(false);
            pause.SetActive(false);
        }

        private void ActivePause(object[] args)
        {
            pause.SetActive(true);

            mainMenu.SetActive(false);
            gameOver.SetActive(false);
            victory.SetActive(false);
        }

        public void ActiveInstructions()
        {
            mainMenu.SetActive(true);
            instructionsMainMenu.SetActive(true);

            welcomeMainMenu.SetActive(false);
            choicesMainMenu.SetActive(false);
            gameOver.SetActive(false);
            victory.SetActive(false);
            pause.SetActive(false);
        }

        private void DeactiveMainMenu(object[] args)
        {
            mainMenu.SetActive(false);
        }

        private void DeactivatePause(object[] args)
        {
            pause.SetActive(false);
        }

        public void DeactiveInstructions()
        {
            ActiveChoices(null);
        }

        public void StartGame()
        {
            EventManager.Instance.Publish(Event.OnStartGameClicked);

            mainMenu.SetActive(false);
        }

        public void BackToMenu()
        {
            EventManager.Instance.Publish(Event.OnBackToMenuClicked);

            pause.SetActive(false);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        #endregion
    }
}
