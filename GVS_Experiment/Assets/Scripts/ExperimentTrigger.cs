using UnityEngine;

public class ExperimentTrigger : MonoBehaviour
{
    [SerializeField]
    private ExperimentManager experimentManager;
    private void OnTriggerEnter(Collider other)
    {
        experimentManager.StartExperiment();
        experimentManager.IsManual = false;
    }
}
