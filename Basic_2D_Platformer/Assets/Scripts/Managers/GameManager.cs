using UnityEngine;

namespace GMDG.Basic2DPlatformer.System
{
    public class GameManager : MonoBehaviour
    {
        private State currentState;

        #region UnityMessages

        private void Awake()
        {
            enabled = false;

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
            EventManager.Instance.Subscribe(Event.OnStartGameClicked, StartGameplay);
            EventManager.Instance.Subscribe(Event.OnBackToMenuClicked, BackToMenu);
            EventManager.Instance.Subscribe(Event.OnEndGameOverTrasition, StartGameOver);
            EventManager.Instance.Subscribe(Event.OnEndVictoryTrasition, StartVictory);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(Event.OnSystemsLoaded, Activate);
            EventManager.Instance.Unsubscribe(Event.OnStartGameClicked, StartGameplay);
            EventManager.Instance.Unsubscribe(Event.OnBackToMenuClicked, BackToMenu);
            EventManager.Instance.Unsubscribe(Event.OnEndGameOverTrasition, StartGameOver);
            EventManager.Instance.Unsubscribe(Event.OnEndVictoryTrasition, StartVictory);
        }

        private void Start()
        {
            currentState = State.None;
        }

        private void Update()
        {
            switch (currentState)
            {
                case State.None:
                    None();
                    break;

                case State.Welcome:
                    Welcome();
                    break;

                case State.MainMenu:
                    MainMenu();
                    break;

                case State.Gameplay:
                    Gameplay();
                    break;

                case State.Pause:
                    Pause();
                    break;

                case State.GameOver:
                    GameOver();
                    break;

                case State.Victory:
                    Victory();
                    break;
            }
        }

        #endregion

        #region Listeners

        private void Activate(object[] args)
        {
            enabled = true;
        }

        private void StartGameplay(object[] args)
        {
            ChangeState(State.Gameplay);
            EventManager.Instance.Publish(Event.OnGameplay);
        }

        private void BackToMenu(object[] args)
        {
            Time.timeScale = 1f;
            ChangeState(State.Welcome);
            EventManager.Instance.Publish(Event.OnWelcome);
        }

        private void StartGameOver(object[] args)
        {
            ChangeState(State.GameOver);
            EventManager.Instance.Publish(Event.OnGameOver);
        }

        private void StartVictory(object[] args)
        {
            ChangeState(State.Victory);
            EventManager.Instance.Publish(Event.OnVictory);
        }

        #endregion

        private void None()
        {
            ChangeState(State.Welcome);
            EventManager.Instance.Publish(Event.OnWelcome);
        }

        private void Welcome()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(State.MainMenu);
                EventManager.Instance.Publish(Event.OnMainMenu);
            }
        }

        private void MainMenu() { }

        private void Gameplay()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 0f;

                ChangeState(State.Pause);
                EventManager.Instance.Publish(Event.OnPause);
            }
        }

        private void Pause()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 1f;

                ChangeState(State.Gameplay);
                EventManager.Instance.Publish(Event.OnUnpause);
            }
        }

        private void GameOver()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(State.Welcome);
                EventManager.Instance.Publish(Event.OnWelcome);
            }
        }

        private void Victory()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(State.Welcome);
                EventManager.Instance.Publish(Event.OnWelcome);
            }
        }

        private void ChangeState(State newState)
        {
            currentState = newState;
        }

        private enum State
        {
            None,
            Welcome,
            MainMenu,
            Gameplay,
            Pause,
            GameOver,
            Victory
        }
    }
}
