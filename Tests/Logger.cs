using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Tests;

public class TestsLogger<T>(ITestOutputHelper output) : ILogger<T> {
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
		throw new NotImplementedException();
	}

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
		output.WriteLine($"{logLevel.ToString()}: {formatter(state, exception)}");
	}
}
