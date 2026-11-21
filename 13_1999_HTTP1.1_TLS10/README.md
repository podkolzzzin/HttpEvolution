# 13 · 1999 · HTTP/1.1 · TLS10

TLS1.0: PRF, HMAC і новий Finished.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run -- --tls10-client PIN
```

Код, доданий або змінений на цьому етапі:

- [LegacyCipher.cs](LegacyCipher.cs).
- [LegacyRecords.cs](LegacyRecords.cs).
- [LegacySsl.Client.cs](LegacySsl.Client.cs).
- [LegacySsl.cs](LegacySsl.cs).
- [Program.cs](Program.cs).
- [Tls10Crypto.cs](../common/Tls/Tls10Crypto.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../04_1996_HTTP1.0_SSL3_PRIMITIVES/LegacyCipher.cs LegacyCipher.cs
diff -u ../04_1996_HTTP1.0_SSL3_PRIMITIVES/LegacyRecords.cs LegacyRecords.cs
diff -u ../09_1996_HTTP1.0_SSL3_CLIENT/LegacySsl.Client.cs LegacySsl.Client.cs
diff -u ../08_1996_HTTP1.0_SSL3_HTTPS/LegacySsl.cs LegacySsl.cs
diff -u ../12_1997_HTTP1.1_CHUNKED/Program.cs Program.cs
diff -u /dev/null ../common/Tls/Tls10Crypto.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
