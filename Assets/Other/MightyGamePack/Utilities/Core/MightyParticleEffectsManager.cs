using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;

namespace MightyGamePack
{
    [System.Serializable]
    public class ParticleEffect
    {
        public string name;
        public GameObject prefab;
    }

    public class ParticleEffectDestroyer : MonoBehaviour
    {

        [ReadOnly] public float timeToDestroy;
        [ReadOnly] public float timer;

        void Update()
        {
            if(timer < timeToDestroy)
            {
                timer += Time.unscaledDeltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public class MightyParticleEffectsManager : MonoBehaviour
    {
        MightyGameManager gameManager;


        [ReorderableList]
        public ParticleEffect[] particleEffects;

        [ReadOnly]
        public List<GameObject> activeParticleEffects;

        float cleanActiveParticleEffectsListTimer;


        void Start()
        {
            gameManager = GameObject.Find("GameManager").GetComponent<MightyGameManager>();
        }


        void Update()
        {
            CleanActiveParticleEffectsList();
        }

        public void SpawnParticleEffect(Vector3 position, Quaternion rotation, float timeToDestroy, float spawnDelay, string particleEffectName)
        {
            ParticleEffect particleEffect = Array.Find(particleEffects, particleEffectFind => particleEffectFind.name == particleEffectName);
            if (particleEffectName == null)
            {
                Debug.LogWarning("Particle effect: " + particleEffectName + " not found!");
                return;
            }

            StartCoroutine(Spawn(particleEffect, position, rotation, timeToDestroy, spawnDelay));
        }

        public void SpawnRandomParticleEffect(Vector3 position, Quaternion rotation, float timeToDestroy, float spawnDelay, params string[] particleEffectNames)
        {
            string particleEffectName = particleEffectNames[UnityEngine.Random.Range(0, particleEffectNames.Length)];

            ParticleEffect particleEffect = Array.Find(particleEffects, particleEffectFind => particleEffectFind.name == particleEffectName);
            if (particleEffectName == null)
            {
                Debug.LogWarning("Randomized particle effect: " + particleEffectName + " not found!");
                return;
            }

            StartCoroutine(Spawn(particleEffect, position, rotation, timeToDestroy, spawnDelay));
        }


        IEnumerator Spawn(ParticleEffect particleEffect, Vector3 position, Quaternion rotation, float timeToDestroy, float spawnDelay)
        {
            yield return new WaitForSeconds(spawnDelay);
            GameObject particleEffectObject = Instantiate(particleEffect.prefab, position, rotation) as GameObject;
            ParticleEffectDestroyer particleEffectDestroyer = particleEffectObject.AddComponent<ParticleEffectDestroyer>();
            particleEffectDestroyer.timeToDestroy = timeToDestroy;

            particleEffectObject.transform.parent = gameObject.transform;
            activeParticleEffects.Add(particleEffectObject);
        }

        public void DestroyAllParticleEffects()
        {
            for (int i = activeParticleEffects.Count - 1; i >= 0; i--)
            {
                GameObject objToDestroy = activeParticleEffects[i];
                activeParticleEffects.RemoveAt(i);
                if(objToDestroy != null)
                {
                    Destroy(objToDestroy);
                }
            }
        }

        void CleanActiveParticleEffectsList()
        {
            if (cleanActiveParticleEffectsListTimer < 3)
            {
                cleanActiveParticleEffectsListTimer += Time.deltaTime;
                for (int i = activeParticleEffects.Count - 1; i >= 0; i--)
                {
                    if (activeParticleEffects[i] == null)
                    {
                        activeParticleEffects.RemoveAt(i);
                    }
                }
                cleanActiveParticleEffectsListTimer = 0;
            }
        }
    }
}
