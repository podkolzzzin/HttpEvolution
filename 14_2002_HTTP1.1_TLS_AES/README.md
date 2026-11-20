# 14 · 2002 · HTTP/1.1 · TLS_AES

TLS1.0: AES-CBC замість 3DES.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run -- --tls10-client PIN
```

AES-клієнт в іншому терміналі. Цей етап ще не має renegotiation indication (додано на етапі 15), тому OpenSSL потребує `-legacy_server_connect`:

```sh
printf 'GET / HTTP/1.0\r\n\r\n' | openssl s_client -quiet -tls1 -legacy_server_connect -cipher 'AES128-SHA:@SECLEVEL=0' -connect 127.0.0.1:8441
```

Код, доданий або змінений на цьому етапі:

- [LegacyCipher.cs](LegacyCipher.cs).
- [LegacyRecords.cs](LegacyRecords.cs).
- [LegacySsl.cs](LegacySsl.cs).
- [Program.cs](Program.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../13_1999_HTTP1.1_TLS10/LegacyCipher.cs LegacyCipher.cs
diff -u ../13_1999_HTTP1.1_TLS10/LegacyRecords.cs LegacyRecords.cs
diff -u ../13_1999_HTTP1.1_TLS10/LegacySsl.cs LegacySsl.cs
diff -u ../13_1999_HTTP1.1_TLS10/Program.cs Program.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
