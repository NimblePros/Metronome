using System.Data;
using System.Diagnostics;
using Dapper;

namespace NimblePros.Metronome;

/// <summary>
/// Wrappers for Dapper's extension methods that record each call on a <see cref="DbCallCounter"/>,
/// so that Dapper calls are reported along with EF Core calls. Add the counter as the first argument
/// of a Dapper call to track it. As with EF Core tracking, only calls that succeed are recorded.
/// </summary>
public static class DapperExtensions
{
  public static IEnumerable<T> Query<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
  {
    return Track(counter, () => SqlMapper.Query<T>(connection, sql, param, transaction, buffered: true, commandTimeout, commandType));
  }

  public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null,
    CancellationToken cancellationToken = default)
  {
    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.Buffered, cancellationToken);
    return TrackAsync(counter, () => SqlMapper.QueryAsync<T>(connection, command));
  }

  public static T QueryFirst<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
  {
    return Track(counter, () => SqlMapper.QueryFirst<T>(connection, sql, param, transaction, commandTimeout, commandType));
  }

  public static Task<T> QueryFirstAsync<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null,
    CancellationToken cancellationToken = default)
  {
    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
    return TrackAsync(counter, () => SqlMapper.QueryFirstAsync<T>(connection, command));
  }

  public static T? QueryFirstOrDefault<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
  {
    return Track(counter, () => SqlMapper.QueryFirstOrDefault<T>(connection, sql, param, transaction, commandTimeout, commandType));
  }

  public static Task<T?> QueryFirstOrDefaultAsync<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null,
    CancellationToken cancellationToken = default)
  {
    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
    return TrackAsync(counter, () => SqlMapper.QueryFirstOrDefaultAsync<T>(connection, command));
  }

  public static T QuerySingle<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
  {
    return Track(counter, () => SqlMapper.QuerySingle<T>(connection, sql, param, transaction, commandTimeout, commandType));
  }

  public static Task<T> QuerySingleAsync<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null,
    CancellationToken cancellationToken = default)
  {
    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
    return TrackAsync(counter, () => SqlMapper.QuerySingleAsync<T>(connection, command));
  }

  public static T? QuerySingleOrDefault<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
  {
    return Track(counter, () => SqlMapper.QuerySingleOrDefault<T>(connection, sql, param, transaction, commandTimeout, commandType));
  }

  public static Task<T?> QuerySingleOrDefaultAsync<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null,
    CancellationToken cancellationToken = default)
  {
    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
    return TrackAsync(counter, () => SqlMapper.QuerySingleOrDefaultAsync<T>(connection, command));
  }

  public static int Execute(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
  {
    return Track(counter, () => SqlMapper.Execute(connection, sql, param, transaction, commandTimeout, commandType));
  }

  public static Task<int> ExecuteAsync(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null,
    CancellationToken cancellationToken = default)
  {
    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
    return TrackAsync(counter, () => SqlMapper.ExecuteAsync(connection, command));
  }

  public static T? ExecuteScalar<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
  {
    return Track(counter, () => SqlMapper.ExecuteScalar<T>(connection, sql, param, transaction, commandTimeout, commandType));
  }

  public static Task<T?> ExecuteScalarAsync<T>(this IDbConnection connection, DbCallCounter counter, string sql,
    object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null,
    CancellationToken cancellationToken = default)
  {
    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
    return TrackAsync(counter, () => SqlMapper.ExecuteScalarAsync<T>(connection, command));
  }

  private static T Track<T>(DbCallCounter counter, Func<T> call)
  {
    var stopwatch = Stopwatch.StartNew();
    var result = call();
    counter.RecordCall(stopwatch.Elapsed);
    return result;
  }

  private static async Task<T> TrackAsync<T>(DbCallCounter counter, Func<Task<T>> call)
  {
    var stopwatch = Stopwatch.StartNew();
    var result = await call();
    counter.RecordCall(stopwatch.Elapsed);
    return result;
  }
}
