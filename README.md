LogComponent
============

Small asynchronous logging component with file rotation (daily) and two stop modes (flush / no-flush).

Requirements
------------
- .NET8 SDK

Projects
--------
- `LogComponent` - the logging component library
- `Application` - example console app that uses the component
- `tests/Unit` - unit tests (xUnit)
- `tests/Integration` - integration tests (xUnit)
- `tests/LogComponent.Tests` - additional tests

Key design points
-----------------
- `AsyncLog` is non-blocking: `Write(...)` enqueues a `LogLine` quickly and returns.
- `ILogWriter` abstracts the output sink. `FileLogWriter` is the production writer that writes files and rotates when the date changes.
- `IDateTimeProvider` is injected to allow deterministic tests (simulate crossing midnight).
- Two stop modes:
 - `StopWithoutFlush()` - exits immediately; outstanding lines may be lost.
 - `StopWithFlush()` - waits for queue to drain before stopping.
- Tests use a test/fake writer and deterministic synchronization (`ManualResetEventSlim`) so test timing is reliable.

How to build
------------
From the repository root:

- Restore and build:

 dotnet restore
 dotnet build

How to run tests
----------------
Run all test projects:

 dotnet test

Run a specific test project:

 dotnet test tests/Unit/LogComponent.UnitTests.csproj
 dotnet test tests/Integration/LogComponent.IntegrationTests.csproj
 dotnet test tests/LogComponent.Tests/LogComponent.Tests.csproj

Notes about integration tests
-----------------------------
- Integration tests write real log files via `FileLogWriter`. By default the tests create a temporary directory for the logs and clean it up. You can inspect files under the temp directory shown in test output if needed.
- Tests use `MutableDateTimeProvider` to simulate crossing midnight and verify rotation.

How to run the example app
--------------------------
From `Application` folder:

 dotnet run --project Application/Application.csproj

By default the example app constructs `FileLogWriter` with `C:\LogTest`. You can change the path in `Application\Program.cs` or construct `AsyncLog` with a different `FileLogWriter` instance.

Continuous Integration
----------------------
- A GitHub Actions workflow `.github/workflows/ci.yml` is included to build the solution and run tests on push / PRs.

Troubleshooting
---------------
- Ensure .NET8 SDK is installed and `dotnet` is on PATH.
- If tests time out, run them locally with increased verbosity: `dotnet test -v diag`.

Further work / improvements
--------------------------
- Expose an optional processing event on `AsyncLog` for even simpler testing hooks (would be a small API change).
- Add configuration for rotation strategy (size-based, custom naming, retention).
- Add more robust logging for internal errors (e.g., ILogger integration).

Contact
-------
This repository was refactored and tests were added to make the component reliable and testable.
