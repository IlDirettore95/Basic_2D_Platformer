using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private State currentState;

    #region UnityMessages

    private void Awake()
    {
        EventManager.Instance.Subscribe(Event.OnSystemsLoaded, (object[] args) => enabled = true);
        enabled = false;
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe(Event.OnStartGameClicked, (object[] args) => 
        {
            ChangeState(State.Gameplay);
            EventManager.Instance.Publish(Event.OnGameplay);
        });
        EventManager.Instance.Subscribe(Event.OnBackToMenuClicked, (object[] args) => 
        {
            Time.timeScale = 1f;
            ChangeState(State.Welcome);
            EventManager.Instance.Publish(Event.OnWelcome);
        });
        EventManager.Instance.Subscribe(Event.OnEndGameOverTrasition, (object[] args) => 
        {
            ChangeState(State.GameOver);
            EventManager.Instance.Publish(Event.OnGameOver);
        });
        EventManager.Instance.Subscribe(Event.OnEndVictoryTrasition, (object[] args) => 
        {
            ChangeState(State.Victory);
            EventManager.Instance.Publish(Event.OnVictory);
        });
    }

    private void Start()
    {
        currentState = State.None;
    }

    private void Update()
    {
        switch(currentState)
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

    private void None()
    {
        ChangeState(State.Welcome);
        EventManager.Instance.Publish(Event.OnWelcome);
    }

    private void Welcome()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ChangeState(State.MainMenu);
            EventManager.Instance.Publish(Event.OnMainMenu);
        }
    }

    private void MainMenu() { }

    private void Gameplay()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
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
