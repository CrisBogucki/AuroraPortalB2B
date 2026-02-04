# Tests

## Local Run

```bash
./scripts/test.sh
```

This uses `test.runsettings` to force single-process execution. Some sandboxed environments block socket usage required by VSTest, which can cause `SocketException (13): Permission denied` during test discovery/execution.

## Direct Command

```bash
dotnet test -m:1 --settings test.runsettings
```
