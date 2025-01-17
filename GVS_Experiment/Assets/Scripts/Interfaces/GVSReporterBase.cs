using System;
using UnityEngine;

public abstract class GVSReporterBase : TrackerBase, IGvsReporter
{
    public abstract void Subscribe(Action<Vector3, AccelerationTypes> subscriber);
    public abstract void TriggerVectorListeners(AccelerationTypes type);
    public abstract void Unsubscribe(Action<Vector3, AccelerationTypes> subscriber);
}
    