using UnityEngine;
using UnityEngine.Pool;

public class PooledAudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private IObjectPool<PooledAudioPlayer> pool;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(IObjectPool<PooledAudioPlayer> pool)
    {
        this.pool = pool;
    }

    public void Play(AudioClip clip, Vector3 position, float volume)
    {
        transform.position = position;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private void Update()
    {
        if (audioSource.isPlaying)
            return;

        pool.Release(this);
    }

    public void ResetPlayer()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }
}