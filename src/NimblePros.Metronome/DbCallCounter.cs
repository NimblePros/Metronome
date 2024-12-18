namespace NimblePros.Metronome;

public class DbCallCounter
{
  public int CallCount { get; internal set; }
  public TimeSpan TotalTimeSpan { get; internal set; }
}
