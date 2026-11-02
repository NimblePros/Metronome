using Microsoft.Data.Sqlite;
using Xunit;

namespace NimblePros.Metronome.Tests;

public sealed class DapperExtensionsTests : IDisposable
{
  private readonly SqliteConnection _connection = new("Data Source=:memory:");
  private readonly DbCallCounter _counter = new();

  public DapperExtensionsTests()
  {
    _connection.Open();
    using var command = _connection.CreateCommand();
    command.CommandText = """
      CREATE TABLE Items (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);
      INSERT INTO Items (Id, Name) VALUES (1, 'one'), (2, 'two');
      """;
    command.ExecuteNonQuery();
  }

  public void Dispose() => _connection.Dispose();

  private void AssertOneCallRecorded()
  {
    Assert.Equal(1, _counter.CallCount);
    Assert.True(_counter.TotalTimeSpan > TimeSpan.Zero);
  }

  [Fact]
  public void Query_ReturnsRowsAndRecordsCall()
  {
    var names = _connection.Query<string>(_counter, "SELECT Name FROM Items ORDER BY Id");

    Assert.Equal(["one", "two"], names);
    AssertOneCallRecorded();
  }

  [Fact]
  public async Task QueryAsync_ReturnsRowsAndRecordsCall()
  {
    var names = await _connection.QueryAsync<string>(_counter, "SELECT Name FROM Items ORDER BY Id");

    Assert.Equal(["one", "two"], names);
    AssertOneCallRecorded();
  }

  [Fact]
  public void QueryFirst_ReturnsFirstRowAndRecordsCall()
  {
    Assert.Equal("one", _connection.QueryFirst<string>(_counter, "SELECT Name FROM Items ORDER BY Id"));
    AssertOneCallRecorded();
  }

  [Fact]
  public async Task QueryFirstAsync_ReturnsFirstRowAndRecordsCall()
  {
    Assert.Equal("one", await _connection.QueryFirstAsync<string>(_counter, "SELECT Name FROM Items ORDER BY Id"));
    AssertOneCallRecorded();
  }

  [Fact]
  public void QueryFirstOrDefault_ReturnsDefaultAndRecordsCall()
  {
    Assert.Null(_connection.QueryFirstOrDefault<string>(_counter, "SELECT Name FROM Items WHERE Id = 99"));
    AssertOneCallRecorded();
  }

  [Fact]
  public async Task QueryFirstOrDefaultAsync_ReturnsDefaultAndRecordsCall()
  {
    Assert.Null(await _connection.QueryFirstOrDefaultAsync<string>(_counter, "SELECT Name FROM Items WHERE Id = 99"));
    AssertOneCallRecorded();
  }

  [Fact]
  public void QuerySingle_UsesParametersAndRecordsCall()
  {
    Assert.Equal("two", _connection.QuerySingle<string>(_counter, "SELECT Name FROM Items WHERE Id = @id", new { id = 2 }));
    AssertOneCallRecorded();
  }

  [Fact]
  public async Task QuerySingleAsync_UsesParametersAndRecordsCall()
  {
    Assert.Equal("two", await _connection.QuerySingleAsync<string>(_counter, "SELECT Name FROM Items WHERE Id = @id", new { id = 2 }));
    AssertOneCallRecorded();
  }

  [Fact]
  public void QuerySingleOrDefault_ReturnsDefaultAndRecordsCall()
  {
    Assert.Null(_connection.QuerySingleOrDefault<string>(_counter, "SELECT Name FROM Items WHERE Id = 99"));
    AssertOneCallRecorded();
  }

  [Fact]
  public async Task QuerySingleOrDefaultAsync_ReturnsDefaultAndRecordsCall()
  {
    Assert.Null(await _connection.QuerySingleOrDefaultAsync<string>(_counter, "SELECT Name FROM Items WHERE Id = 99"));
    AssertOneCallRecorded();
  }

  [Fact]
  public void Execute_ReturnsAffectedRowsAndRecordsCall()
  {
    Assert.Equal(2, _connection.Execute(_counter, "UPDATE Items SET Name = 'x'"));
    AssertOneCallRecorded();
  }

  [Fact]
  public async Task ExecuteAsync_ReturnsAffectedRowsAndRecordsCall()
  {
    Assert.Equal(2, await _connection.ExecuteAsync(_counter, "UPDATE Items SET Name = 'x'"));
    AssertOneCallRecorded();
  }

  [Fact]
  public void ExecuteScalar_ReturnsValueAndRecordsCall()
  {
    Assert.Equal(2, _connection.ExecuteScalar<int>(_counter, "SELECT COUNT(*) FROM Items"));
    AssertOneCallRecorded();
  }

  [Fact]
  public async Task ExecuteScalarAsync_ReturnsValueAndRecordsCall()
  {
    Assert.Equal(2, await _connection.ExecuteScalarAsync<int>(_counter, "SELECT COUNT(*) FROM Items"));
    AssertOneCallRecorded();
  }

  [Fact]
  public async Task MultipleCalls_AccumulateOnCounter()
  {
    await _connection.ExecuteScalarAsync<int>(_counter, "SELECT COUNT(*) FROM Items");
    await _connection.QueryAsync<string>(_counter, "SELECT Name FROM Items");
    _connection.Execute(_counter, "DELETE FROM Items");

    Assert.Equal(3, _counter.CallCount);
  }

  [Fact]
  public async Task FailedCall_IsNotRecorded()
  {
    Assert.Throws<SqliteException>(() => _connection.Execute(_counter, "SELECT * FROM Missing"));
    await Assert.ThrowsAsync<SqliteException>(() => _connection.ExecuteAsync(_counter, "SELECT * FROM Missing"));

    Assert.Equal(0, _counter.CallCount);
  }

  [Fact]
  public async Task QueryAsync_HonorsCancellationToken()
  {
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(
      () => _connection.QueryAsync<string>(_counter, "SELECT Name FROM Items", cancellationToken: cts.Token));
  }

  [Fact]
  public async Task ConcurrentCalls_AreAllRecorded()
  {
    const int callsPerTask = 500;
    const int taskCount = 8;

    await Task.WhenAll(Enumerable.Range(0, taskCount).Select(_ => Task.Run(() =>
    {
      using var connection = new SqliteConnection("Data Source=:memory:");
      for (var i = 0; i < callsPerTask; i++)
      {
        connection.ExecuteScalar<int>(_counter, "SELECT 1");
      }
    })));

    Assert.Equal(callsPerTask * taskCount, _counter.CallCount);
  }
}
