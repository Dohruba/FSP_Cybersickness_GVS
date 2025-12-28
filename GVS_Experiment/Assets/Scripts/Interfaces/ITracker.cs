using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITracker
{
    void Track();
    bool IsTracking();
    void StopTracking();
    void StartTracking();

    void Subscribe(Action<Vector3> subscriber);
    void Unsubscribe(Action<Vector3> subscriber);
    void TriggerStringListeners();
}
