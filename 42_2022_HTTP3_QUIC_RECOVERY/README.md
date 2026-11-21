# 42 · 2022 · HTTP/3 · QUIC_RECOVERY

QUIC для HTTP/3: повтори з новими packet number.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

Клієнт HTTP/3 та SPKI pin описані в [головному README](../README.md#http3).

За замовчуванням — HTTP/3 із recovery. Інші готові режими: `--http`, [`--sse` та `--ws`](../15b_2011_HTTP1.1_REALTIME/), `--ssl3`, `--tls10`, `--tls`, `--h2`, `--h2 --tls` та pinned клієнти.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [QuicSession.cs](QuicSession.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../41_2022_HTTP3_RESPONSE/Program.cs Program.cs
diff -u ../41_2022_HTTP3_RESPONSE/QuicSession.cs QuicSession.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
