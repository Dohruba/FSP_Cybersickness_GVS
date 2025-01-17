using System;
using System.Collections.Generic;

public interface ITracker
{
    void Track();
    bool IsRecording();
    void StopRecording();
    void StartRecording();

    void Subscribe(Action<List<string>> subscriber);
    void Unsubscribe(Action<List<string>> subscriber);
    void Trigger();
}
