using UnityEngine;

public class FMSController : MonoBehaviour
{
    [SerializeField] private FMSTracker FMSTracker;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            FMSTracker.IncreaseFMS();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            FMSTracker.DecreaseFMS();
        }
    }
}
