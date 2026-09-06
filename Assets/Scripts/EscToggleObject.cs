using UnityEngine;

public class EscToggleObject : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (targetObject != null)
            {
                targetObject.SetActive(!targetObject.activeSelf);
            }
        }
    }
}