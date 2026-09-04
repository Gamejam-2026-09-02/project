using UnityEngine;
using UnityEngine.Pool;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Prefab")]
    [SerializeField] private PooledAudioPlayer audioPrefab;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 50;

    private ObjectPool<PooledAudioPlayer> pool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        pool = new ObjectPool<PooledAudioPlayer>(
            CreatePlayer,
            OnGetPlayer,
            OnReleasePlayer,
            OnDestroyPlayer,
            true,
            defaultCapacity,
            maxPoolSize
        );
    }

    private PooledAudioPlayer CreatePlayer()
    {
        PooledAudioPlayer player = Instantiate(audioPrefab, transform);
        player.Initialize(pool);
        return player;
    }

    private void OnGetPlayer(PooledAudioPlayer player)
    {
        player.gameObject.SetActive(true);
    }

    private void OnReleasePlayer(PooledAudioPlayer player)
    {
        player.ResetPlayer();
        player.gameObject.SetActive(false);
    }

    private void OnDestroyPlayer(PooledAudioPlayer player)
    {
        Destroy(player.gameObject);
    }

    public void Play(
        AudioClip clip,
        Vector3 position,
        float volume = 1f)
    {
        if (clip == null)
            return;

        PooledAudioPlayer player = pool.Get();

        player.Play(
            clip,
            position,
            volume
        );
    }

    public void Play(
        AudioClip clip,
        Vector3 position,
        float volumeMin,
        float volumeMax)
    {
        if (clip == null)
            return;

        PooledAudioPlayer player = pool.Get();

        float volume = Random.Range(volumeMin, volumeMax);

        player.Play(
            clip,
            position,
            volume
        );
    }
}