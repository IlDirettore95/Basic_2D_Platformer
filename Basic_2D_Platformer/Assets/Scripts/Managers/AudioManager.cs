using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Music

    [SerializeField] AudioClip mainMenuMusic;
    [SerializeField] AudioClip gameMusic;

    #endregion

    #region SFX

    #region UI

    [SerializeField] AudioClip buttonClick;
    [SerializeField] AudioClip buttonOver;

    #endregion

    #region Gameplay

    [SerializeField] List<AudioClip> visitorsSpawns;
    [SerializeField] List<AudioClip> playerHits;
    [SerializeField] List<AudioClip> visitorHits;
    [SerializeField] List<AudioClip> laserShots;

    [SerializeField] AudioClip victory;

    #endregion

    #endregion

    private AudioSource musicSource1;
    private AudioSource musicSource2;
    private AudioSource activeMusicSource;
    private AudioSource secondaryMusicSource;

    private List<AudioSource> sfxAudioSources;

    #region UnityMessages

    private void Awake()
    {
        musicSource1 = gameObject.AddComponent<AudioSource>();
        musicSource1.loop = true;
        musicSource1.volume = 0f;
        musicSource2 = gameObject.AddComponent<AudioSource>();
        musicSource2.loop = true;
        musicSource2.volume = 0f;

        activeMusicSource = musicSource1;
        secondaryMusicSource = musicSource2;

        sfxAudioSources = new List<AudioSource>();
        for(int i = 0; i < 10; i++)
        {
            sfxAudioSources.Add(gameObject.AddComponent<AudioSource>());
            sfxAudioSources[i].loop = false;
        }

        EventManager.Instance.Subscribe(Event.OnSystemsLoaded, (object[] args) => enabled = true);
        enabled = false;
    }

    private void OnEnable()
    {
        // Game Manager
        EventManager.Instance.Subscribe(Event.OnWelcome, (object[] args) => PlayMusicCrossfade(mainMenuMusic, 0.8f, 2.5f, 0.5f));
        EventManager.Instance.Subscribe(Event.OnGameplay, (object[] args) => PlayMusicCrossfade(gameMusic, 0.7f, 1f, 1f));

        // UI
        EventManager.Instance.Subscribe(Event.OnButtonClicked, (object[] args) => PlaySFX(buttonClick, 0.6f));
        EventManager.Instance.Subscribe(Event.OnButtonOver, (object[] args) => PlaySFX(buttonOver, 0.8f));
    }

    private void OnDisable()
    {
        // Game Manager
        EventManager.Instance.Unsubscribe(Event.OnWelcome, (object[] args) => PlayMusicCrossfade(mainMenuMusic, 0.8f, 2.5f, 0.5f));
        EventManager.Instance.Unsubscribe(Event.OnGameplay, (object[] args) => PlayMusicCrossfade(gameMusic, 0.7f, 1f, 1f));

        // UI
        EventManager.Instance.Unsubscribe(Event.OnButtonClicked, (object[] args) => PlaySFX(buttonClick, 0.6f));
        EventManager.Instance.Unsubscribe(Event.OnButtonOver, (object[] args) => PlaySFX(buttonOver, 0.8f));
    }

    #endregion

    private void PlayMusic(AudioClip music, AudioSource audioSource, float volume)
    {
        audioSource.clip = music;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private void PlayMusicCrossfade(AudioClip music, float volume, float fadeInDuration, float fadeOutDuration)
    {
        if (activeMusicSource.clip == music) return;

        StopAllCoroutines();

        if (activeMusicSource.clip == null)
        {
            StartCoroutine(PlayMusicFadeIn(music, activeMusicSource, volume, fadeInDuration));
            return;
        }

        StartCoroutine(PlayMusicFadeIn(music, secondaryMusicSource, volume, fadeInDuration));
        StartCoroutine(PlayMusicFadeOut(activeMusicSource, fadeOutDuration));

        AudioSource tempAudioSource = activeMusicSource;
        activeMusicSource = secondaryMusicSource;
        secondaryMusicSource = tempAudioSource;
    }

    private IEnumerator PlayMusicFadeIn(AudioClip music, AudioSource audioSource, float volume, float duration)
    {
        float currentTime = 0f;
        float start = audioSource.volume;


        PlayMusic(music, audioSource, 0f);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, volume, currentTime / duration);
            yield return null;
        }
    }

    private IEnumerator PlayMusicFadeOut(AudioSource audioSource, float duration)
    {
        float currentTime = 0f;
        float start = activeMusicSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, 0f, currentTime / duration);
            yield return null;
        }

        audioSource.clip = null;
    }

    private void PlaySFX(AudioClip sound, float volume)
    {
        for (int i = 0; i < sfxAudioSources.Count; i++)
        {
            if (!sfxAudioSources[i].isPlaying)
            {
                sfxAudioSources[i].clip = sound;
                sfxAudioSources[i].volume = volume;
                sfxAudioSources[i].Play();
                return;
            }
        }

        Debug.LogError("Couldn't play SFX!");
    }

    private void PlaySFX(AudioClip sound, float volume, float delay)
    {
        for(int i = 0; i < sfxAudioSources.Count; i++)
        {
            if(!sfxAudioSources[i].isPlaying)
            {
                sfxAudioSources[i].clip = sound;
                sfxAudioSources[i].volume = volume;
                sfxAudioSources[i].PlayDelayed(delay);
                return;
            }
        }

        Debug.LogError("Couldn't play SFX!");
    }

    private void PlayRandomSFX(List<AudioClip> listOfSfx, float volume)
    {
        int randomIndex = Random.Range(0, listOfSfx.Count);
        float randomDelay = Random.Range(0, 0.05f);
        PlaySFX(listOfSfx[randomIndex], volume, randomDelay);
    }
}
