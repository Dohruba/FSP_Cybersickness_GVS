using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class TrackerBase : MonoBehaviour, ITracker
{
    public abstract bool IsRecording();

    public abstract void StartRecording();

    public abstract void StopRecording();

    public abstract void Subscribe(Action<List<string>> subscriber);

    public abstract void Track();

    public abstract void Trigger();

    public abstract void Unsubscribe(Action<List<string>> subscriber);


}
