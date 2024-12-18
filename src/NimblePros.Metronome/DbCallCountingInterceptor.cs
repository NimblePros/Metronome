using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

using NimblePros.Metronome;

public class DbCallCountingInterceptor : DbCommandInterceptor
{
    private readonly DbCallCounter _counter;

    public DbCallCountingInterceptor(DbCallCounter counter)
    {
        _counter = counter;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        TrackCall(eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        TrackCall(eventData.Duration);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        TrackCall(eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        TrackCall(eventData.Duration);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object result)
    {
        TrackCall(eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object result,
        CancellationToken cancellationToken = default)
    {
        TrackCall(eventData.Duration);
        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void TrackCall(TimeSpan duration)
    {

        Console.WriteLine($"counter in db interceptor: {_counter.GetHashCode()}");
        _counter.CallCount++;
        _counter.TotalTimeMs += (long)duration.TotalMilliseconds;
    }
}
