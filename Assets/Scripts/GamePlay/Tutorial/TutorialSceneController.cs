using UnityEngine;

public class TutorialSceneController : MonoBehaviour
{
    [Header("进入场景时是否暂停")]
    [SerializeField]
    private bool pauseOnStart = true;


    private void Start()
    {
        if (pauseOnStart)
        {
            GameStateController.Instance.PauseGame();
        }
    }


    public void FinishTutorial()
    {
        GameStateController.Instance.ResumeGame();
    }
}