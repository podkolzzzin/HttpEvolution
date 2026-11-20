# 37 · 2021 · HTTP/3 · QUIC_UDP

QUIC: приймання сеансів через UdpClient.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Демо запускає локальний сервер; зупинка — Ctrl+C.

UDP-сервер показує QUIC/TLS handshake; повна HTTP/3-відповідь доступна з етапу 41.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [QuicServer.cs](QuicServer.cs).
- [TlsCertificate.Pin.cs](TlsCertificate.Pin.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../36_2021_HTTP3_QUIC_TLS/Program.cs Program.cs
diff -u /dev/null QuicServer.cs
diff -u /dev/null TlsCertificate.Pin.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
