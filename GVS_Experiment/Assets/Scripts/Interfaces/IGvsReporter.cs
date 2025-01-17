using System;
using UnityEngine;

public interface IGvsReporter
{
    void Subscribe(Action<Vector3, AccelerationTypes> subscriber);
    void Unsubscribe(Action<Vector3, AccelerationTypes> subscriber);
    void TriggerVectorListeners(AccelerationTypes type);
}
