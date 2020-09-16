using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using AmplifyShaderEditor;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    public GameObject dynamicAudioPlayer;
    public Transform effectsContainer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public void SpawnParticlesAtPoint(GameObject particles, Vector3 position, Quaternion rotation, float lifeTime = 5f)
    {
        GameObject spawnedParticles = Instantiate(particles, position, rotation, effectsContainer);
        Destroy(spawnedParticles, lifeTime);
    }

    public void PlayClipAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f, float lifeTime = 5f)
    {
        GameObject dynamicPlayer = Instantiate(dynamicAudioPlayer, position, Quaternion.identity,
            effectsContainer);
        AudioSource dynamicSource = dynamicPlayer.GetComponent<AudioSource>();
        dynamicSource.clip = clip;
        dynamicSource.volume = volumeScale;
        dynamicSource.Play();
        Destroy(dynamicPlayer, lifeTime);
    }
}
