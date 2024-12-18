namespace NimblePros.Metronome;

public class DbCallCounter
{
    public int CallCount { get; set; }
    public long TotalTimeMs { get; internal set; }
}
