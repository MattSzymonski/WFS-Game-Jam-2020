/* 
NAME:
    Mighty Audio Manager

DESCRIPTION:


USAGE:
    Look under the code below 

TODO:
   There is bug when playing random sound multiple are being played (one by one?)
*/

using UnityEngine.Audio;
using UnityEngine;
using System;
using NaughtyAttributes;

namespace MightyGamePack
{
    public enum SoundType
    {
        Effect,
        Music
    };

    [System.Serializable]
    public class MightySound
    {
        public string name;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1.0f;
        [Range(0.1f, 3f)]
        public float pitch = 1.0f;

        public bool playOnAwake;
        public bool loop;


        public SoundType soundType;

        [HideInInspector]
        public AudioSource source;
    }

    public class MightyAudioManager : MonoBehaviour
    {

        [ReorderableList] public MightySound[] sounds;

        MightyGameManager gameManager;


        [HideInInspector] public static MightyAudioManager instance;

        public AudioMixerGroup musicMixerGroup;
        public AudioMixerGroup effectsMixerGroup;

        void Awake()
        {
            if (instance == null) //Make AudioManager a singleton
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            //DontDestroyOnLoad(gameObject);

            if (musicMixerGroup == null || effectsMixerGroup == null)
            {
                Debug.LogError("Sound mixer groups not set");
            }

            foreach (MightySound sound in sounds)
            {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;
                sound.source.volume = sound.volume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;
                sound.source.playOnAwake = sound.playOnAwake;

                if (sound.soundType == SoundType.Effect)
                {
                    sound.source.outputAudioMixerGroup = effectsMixerGroup;
                }
                if (sound.soundType == SoundType.Music)
                {
                    sound.source.outputAudioMixerGroup = musicMixerGroup;
                }
                if (sound.playOnAwake)
                {
                    sound.source.Play();
                }
            }
        }

        public void Start()
        {
            gameManager = GameObject.Find("GameManager").GetComponent<MightyGameManager>();
        }

        public void PlaySound(string soundName)
        {
            MightySound sound = Array.Find(sounds, soundFind => soundFind.name == soundName);
            if (sound == null)
            {
                Debug.LogWarning("Sound: " + soundName + " not found!");
                return;
            }
            sound.source.Play();
        }

        public void StopSound(string soundName)
        {
            MightySound sound = Array.Find(sounds, soundFind => soundFind.name == soundName);
            if (sound == null)
            {
                Debug.LogWarning("Sound: " + soundName + " not found!");
                return;
            }
            sound.source.Stop();
        }

        public bool IsSoundPlaying(string soundName)
        {
            MightySound sound = Array.Find(sounds, soundFind => soundFind.name == soundName);
            if (sound == null)
            {
                Debug.LogWarning("Sound: " + soundName + " not found!");
                return false;
            }
            return sound.source.isPlaying;
        }

        public void PlayRandomSound(params string[] soundNames)
        {
            string soundName = soundNames[UnityEngine.Random.Range(0, soundNames.Length)];
            MightySound sound = Array.Find(sounds, soundFind => soundFind.name == soundName);
            if (sound == null)
            {
                Debug.LogWarning("Randomized sound: " + soundName + " not found!");
                return;
            }
            sound.source.Play();
        }

        public void SetMusicMixerVolume(float sliderValue)
        {
            musicMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
        }

        public void SetEffectsMixerVolume(float sliderValue)
        {
            effectsMixerGroup.audioMixer.SetFloat("EffectsVolume", Mathf.Log10(sliderValue) * 20);
        }
    }
}

/* 
USAGE:
    Add this component to GameObject in Hierarchy
    Add sounds to it in inspector
    audioManager.PlaySound("SoundName"); // Play sound named "SoundName" in inspector
    audioManager.StopSound("SoundName"); // Stop playing sound named "SoundName" in inspector
    audioManager.PlayRandomSound("Sound1", "Sound2", "Sound3",) // Randomly choose and play one of sounds passed as parameter
    
*/
