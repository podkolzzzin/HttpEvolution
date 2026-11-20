# 15 · 2010 · HTTP/1.1 · TLS_RENEGOTIATION

TLS: renegotiation indication.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run -- --tls10-client PIN
```

AES-клієнт в іншому терміналі:

```sh
printf 'GET / HTTP/1.0\r\n\r\n' | openssl s_client -quiet -tls1 -cipher 'AES128-SHA:@SECLEVEL=0' -connect 127.0.0.1:8441
```

Код, доданий або змінений на цьому етапі:

- [LegacySsl.cs](LegacySsl.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../14_2002_HTTP1.1_TLS_AES/LegacySsl.cs LegacySsl.cs
diff -u ../14_2002_HTTP1.1_TLS_AES/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
