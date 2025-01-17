using UnityEngine;

public class GVSCDataSender : MonoBehaviour
{
    [SerializeField]
    private GVSReporterBase[] gvsInfluencers;

    private void Awake()
    {
        SubscribeToTrackers();
    }

    private void SubscribeToTrackers()
    {
        foreach (var tracker in gvsInfluencers)
        {
            if (tracker != null)
            {
                tracker.Subscribe(HandleAcceleration);
            }
        }
    }

    private void HandleAcceleration(Vector3 direction, AccelerationTypes type)
    {

    }

}
