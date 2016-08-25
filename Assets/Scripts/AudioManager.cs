using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
    public int numberOfSFXBanks = 6;
    public float bgmTransitionTime = 1f;
    public float maxBackgroundVolume = 1f;

    Dictionary<string, AudioSource> m_banks;
    AudioSource m_background1;
    AudioSource m_background2;
    AudioSource m_currentBackground;

    static string SFXPrefix = "_sfx";

    void Awake() {
        Initialize ();
    }

    /// <summary>
    /// Plays a clip in a named audio bank.
    /// </summary>
    /// <param name="bankName"></param>
    /// <param name="clip"></param>
    /// <param name="loop"></param>
    public void PlayClipInBank(string bankName, AudioClip clip, bool loop) {
        // Create the new bank if necessary.
        if (!m_banks.ContainsKey (bankName)) {
            m_banks [bankName] = CreateAudioSource ();
        }

        // Set and play the new clip.
        m_banks[bankName].loop = loop;
        m_banks [bankName].clip = clip;
        if (m_banks[bankName].clip != null) {
            m_banks [bankName].Play ();
        }
    }

    /// <summary>
    /// Stops playing an audio bank.
    /// </summary>
    /// <param name="bankName"></param>
    public void StopBank(string bankName) {
        if (m_banks.ContainsKey(bankName)) {
            m_banks[bankName].Stop();
        }
    }

    /// <summary>
    /// Play a one-shot sound effect.
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySFX(AudioClip clip) {
        // Find an idle AudioSource to use.
        for (int i = 0; i < numberOfSFXBanks; ++i) {
            if (!m_banks [AudioManager.SFXPrefix + i].isPlaying) {
                m_banks [AudioManager.SFXPrefix + i].PlayOneShot (clip);
                return;
            }
        }

        // If all the audio soruces are playing clips, overwrite the first one.
        Debug.LogError ("SFX channel overflow");
        m_banks [AudioManager.SFXPrefix + "0"].PlayOneShot (clip);
    }

    /// <summary>
    /// Sets the background music, fading between clips if they're different.
    /// </summary>
    /// <param name="clip"></param>
    public void SetBGM(AudioClip clip) {
        if (clip == m_currentBackground.clip) {
            return;
        }

        AudioSource oldBackground = m_currentBackground;
        if (m_currentBackground == m_background1) {
            m_currentBackground = m_background2;
        }
        else {
            m_currentBackground = m_background1;
        }

        m_currentBackground.clip = clip;
        if (clip != null) {
            m_currentBackground.Play ();
        }

        StartCoroutine(FadeBetween(oldBackground, m_currentBackground));     
    }

    /// <summary>
    /// Initialize the audio manager.
    /// </summary>
    void Initialize() {
        m_banks = new Dictionary<string, AudioSource> ();

        for (int i = 0; i < numberOfSFXBanks; ++i) {
            m_banks [AudioManager.SFXPrefix + i] = CreateAudioSource ();
        }

        m_background1 = CreateAudioSource ();
        m_background1.loop = true;

        m_background2 = CreateAudioSource ();
        m_background2.loop = true;

        m_currentBackground = m_background1;
    }

    AudioSource CreateAudioSource() {
        AudioSource source = gameObject.AddComponent <AudioSource>() as AudioSource;
        source.playOnAwake = false;

        return source;
    }

    IEnumerator FadeBetween(AudioSource oldSource, AudioSource newSource) {
        newSource.volume = 0f;
        oldSource.volume = maxBackgroundVolume;

        float elapsed = 0f;
        float timeSlice = 0.1f;
        while (elapsed <= bgmTransitionTime) {
            float oldVolume = Mathf.Lerp (maxBackgroundVolume, 0f, elapsed / bgmTransitionTime);
            oldSource.volume = oldVolume;
            newSource.volume = maxBackgroundVolume - oldVolume;
            elapsed += timeSlice;
            yield return new WaitForSecondsRealtime (timeSlice);
        }

        oldSource.Stop ();
        newSource.volume = maxBackgroundVolume;
    }
}

