using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public static GameStateController Instance { get; private set; }

    public bool IsPaused { get; private set; }

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
            PauseGame();
        else
            ResumeGame();
    }
}