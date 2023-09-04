using System;
using System.Collections.Generic;

namespace GMDG.Basic2DPlatformer.System
{
    public class EventManager
    {
        private static EventManager _instance;
        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventManager();
                }
                return _instance;
            }
        }

        private Dictionary<Event, Action<object[]>> _listenersDictionary = new Dictionary<Event, Action<object[]>>();

        private EventManager() { }

        public void Subscribe(Event eventName, Action<object[]> listener)
        {
            if (!_listenersDictionary.ContainsKey(eventName))
            {
                _listenersDictionary[eventName] = listener;
            }
            else
            {
                _listenersDictionary[eventName] += listener;
            }
        }

        public void Unsubscribe(Event eventName, Action<object[]> listener)
        {
            if (_listenersDictionary.ContainsKey(eventName))
            {
                _listenersDictionary[eventName] -= listener;

                if (_listenersDictionary[eventName] == null)
                {
                    _listenersDictionary.Remove(eventName);
                }
            }
        }

        public void Publish(Event eventName, params object[] args)
        {
            if (_listenersDictionary.ContainsKey(eventName))
            {
                _listenersDictionary[eventName]?.Invoke(args);
            }
        }

        public override string ToString()
        {
            string text = string.Empty;

            foreach (Event e in _listenersDictionary.Keys)
            {
                text += "Event registred: " + e + "\tListeners: " + _listenersDictionary[e]?.GetInvocationList().Length + Environment.NewLine;
            }

            return text;
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

        // Camera
        OnEndGameOverTrasition,
        OnEndVictoryTrasition,

        // UI
        OnStartGameClicked,
        OnBackToMenuClicked,
        OnButtonClicked,
        OnButtonOver,

        // PCG
        OnGridUpdated
    }
}