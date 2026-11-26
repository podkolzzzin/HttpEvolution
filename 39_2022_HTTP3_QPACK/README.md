# 39 · 2022 · HTTP/3 · QPACK

QPACK compresses HTTP/3 headers and shares its integer and string formats with HPACK.

Run [Program.cs](Program.cs) from this directory.

```sh
dotnet run
```

After the literal example, the console shows three events.

1. Stream `0` receives `HEADERS` referencing the first insertion and waits.
2. The decoder uses the static table to decode stream `4` while stream `0` waits.
3. The encoder stream delivers `x-demo: ready`. The decoder can now resolve the reference on stream `0`.

## Files added or changed in this step

- [Program.cs](Program.cs).
- [Qpack.cs](../common/HeaderCompression/Qpack.cs).
- [QpackDynamicTable.cs](../common/HeaderCompression/QpackDynamicTable.cs).

Compare with the previous step. Exit status 1 means that `diff` found differences.

```sh
diff -u ../38_2021_HTTP3_QUIC_PTO/Program.cs Program.cs
diff -u /dev/null ../common/HeaderCompression/Qpack.cs
```

This demo has its own `Program.cs`. [HttpEvolution.csproj](HttpEvolution.csproj) links to shared files in [`common/`](../common/) and to the stages that introduced each version of a helper class.

[All steps](../README.md).
