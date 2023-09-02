using System;
using System.Collections;
using System.Collections.Generic;

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
    // System
    OnSystemsLoaded,

    // Game Manager
    OnWelcome,
    OnMainMenu,
    OnGameplay,
    OnPause,
    OnUnpause,
    OnGameOver,
    OnVictory,

    //Camera
    OnEndGameOverTrasition,
    OnEndVictoryTrasition,

    //UI
    OnStartGameClicked,
    OnBackToMenuClicked,
    OnButtonClicked,
    OnButtonOver
}
