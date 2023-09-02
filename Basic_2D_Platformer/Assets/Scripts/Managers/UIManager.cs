using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        EventManager.Instance.Subscribe(Event.OnSystemsLoaded, (object[] args) => enabled = true);
        enabled = false;
    }

    private void OnEnable()
    {
        // Game Manager
        EventManager.Instance.Subscribe(Event.OnWelcome, ActiveWelcome);
        EventManager.Instance.Subscribe(Event.OnMainMenu, ActiveChoices);
        EventManager.Instance.Subscribe(Event.OnGameplay, (object[] args) => mainMenu.SetActive(false));
        EventManager.Instance.Subscribe(Event.OnPause, ActivePause);
        EventManager.Instance.Subscribe(Event.OnUnpause, (object[] args) => pause.SetActive(false));
        EventManager.Instance.Subscribe(Event.OnEndGameOverTrasition, ActiveGameOver);
        EventManager.Instance.Subscribe(Event.OnEndVictoryTrasition, ActiveVictory);
    }

    private void OnDisable()
    {
        // Game Manager
        EventManager.Instance.Unsubscribe(Event.OnWelcome, ActiveWelcome);
        EventManager.Instance.Unsubscribe(Event.OnMainMenu, ActiveChoices);
        EventManager.Instance.Unsubscribe(Event.OnGameplay, (object[] args) => mainMenu.SetActive(false));
        EventManager.Instance.Unsubscribe(Event.OnPause, ActivePause);
        EventManager.Instance.Unsubscribe(Event.OnUnpause, (object[] args) => pause.SetActive(false));
        EventManager.Instance.Unsubscribe(Event.OnEndGameOverTrasition, ActiveGameOver);
        EventManager.Instance.Unsubscribe(Event.OnEndVictoryTrasition, ActiveVictory);
    }

    #endregion

    #region Listener

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
