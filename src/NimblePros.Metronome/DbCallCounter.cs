namespace NimblePros.Metronome;

public class DbCallCounter
{
  private readonly object _lock = new();

  public int CallCount { get; private set; }
  public TimeSpan TotalTimeSpan { get; private set; }

  // Unlike a DbContext, Dapper connections can run concurrently within a request (e.g. Task.WhenAll).
  internal void RecordCall(TimeSpan duration)
  {
    lock (_lock)
    {
      CallCount++;
      TotalTimeSpan += duration;
    }
  }
}
