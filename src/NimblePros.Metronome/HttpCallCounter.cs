namespace NimblePros.Metronome;

public class HttpCallCounter
{
  private readonly object _lock = new();

  public int CallCount { get; private set; }
  public TimeSpan TotalTimeSpan { get; private set; }

  // HTTP calls within a request often run concurrently (e.g. Task.WhenAll).
  internal void RecordCall(TimeSpan duration)
  {
    lock (_lock)
    {
      CallCount++;
      TotalTimeSpan += duration;
    }
  }
}
