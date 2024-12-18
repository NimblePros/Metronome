namespace NimblePros.Metronome;

public class HttpCallCounter
{
  public int CallCount { get; internal set; }
  public TimeSpan TotalTimeSpan { get; internal set; }
}
