using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
    public int numberOfSFXBanks = 4;

    Dictionary<string, AudioSource> m_banks;
    AudioSource m_background1;
    AudioSource m_background2;
    AudioSource m_currentBackground;

    static string SFXPrefix = "_sfx";

    void Awake() {
        Initialize ();
    }

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

    public void PlaySFX(AudioClip clip) {
        // Find an idle AudioSource to use.
        for (int i = 0; i < m_banks.Count; ++i) {
            if (!m_banks [AudioManager.SFXPrefix + i].isPlaying) {
                m_banks [AudioManager.SFXPrefix + i].PlayOneShot (clip);
                return;
            }
        }

        // If all the audio soruces are playing clips, overwrite the first one.
        m_banks [AudioManager.SFXPrefix + "0"].PlayOneShot (clip);
    }

    public void SetBGM(AudioClip clip) {
        if (m_currentBackground == m_background1) {
            m_currentBackground = m_background2;
            m_background1.Stop ();
        }
        else {
            m_currentBackground = m_background1;
            m_background2.Stop ();
        }

        m_currentBackground.clip = clip;
        if (clip != null) {
            m_currentBackground.Play ();
        }

        // @TODO Fade the clips.
    }

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
}
