# 37 · 2021 · HTTP/3 · QUIC_UDP

QUIC: приймання сеансів через UdpClient.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h2
```

Це підготовчий крок QUIC/HTTP/3; команда вище запускає готовий HTTP/2. Повна відповідь HTTP/3 доступна з етапу 41.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../36_2021_HTTP3_QUIC_TLS .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
