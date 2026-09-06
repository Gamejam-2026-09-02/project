using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public static GameStateController Instance { get; private set; }

    public bool IsPaused { get; private set; }

    private bool timePaused;


    [Header("днЭЃекеж")]
    [SerializeField]
    private GameObject pauseMask;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetPause(!timePaused);
        }
    }


    public void PauseGame()
    {
        if (IsPaused)
            return;

        IsPaused = true;

        Time.timeScale = 0f;
    }


    public void ResumeGame()
    {
        if (!IsPaused)
            return;

        IsPaused = false;

        Time.timeScale = 1f;
    }


    public void SetPause(bool pause)
    {
        if (pause)
        {
            timePaused = true;
            Time.timeScale = 0f;
            pauseMask.SetActive(true);
        }
        else
        {
            timePaused = false;
            Time.timeScale = 1f;
            pauseMask.SetActive(false);
        }
    }
}