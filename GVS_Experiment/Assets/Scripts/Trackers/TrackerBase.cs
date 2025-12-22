using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class TrackerBase : MonoBehaviour, ITracker
{
    public abstract bool IsTracking();

    public abstract void StartTracking();

    public abstract void StopTracking();

    public abstract void Subscribe(Action<Vector3> subscriber);

    public abstract void Unsubscribe(Action<Vector3> subscriber);

    public abstract void Track();

    public abstract void TriggerStringListeners();
}
