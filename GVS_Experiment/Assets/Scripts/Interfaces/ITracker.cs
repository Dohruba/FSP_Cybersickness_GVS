using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITracker
{
    void Track();
    bool IsRecording();
    void StopRecording();
    void StartRecording();

    void Subscribe(Action<List<string>> subscriber);
    void Unsubscribe(Action<List<string>> subscriber);
    void TriggerStringListeners();
}
