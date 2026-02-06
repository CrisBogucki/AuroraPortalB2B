# Testing

This document explains how to run the test suite, why certain settings are required, and how to troubleshoot common issues in constrained environments.

## Why `test.runsettings`
The solution uses `test.runsettings` to force single-process execution. Some sandboxed or CI environments block the socket usage required by VSTest, which can cause errors like:

```
System.Net.Sockets.SocketException (13): Permission denied
```

Running tests with `-m:1` and the provided settings avoids those failures.

## Recommended Local Run
```bash
./scripts/test.sh
```

## Direct Command
```bash
dotnet test -m:1 --settings test.runsettings
```

## When Tests Fail in CI
If you see socket or discovery errors:
- Ensure `-m:1` is used.
- Ensure `test.runsettings` is passed.
- Avoid parallel test execution in constrained environments.
