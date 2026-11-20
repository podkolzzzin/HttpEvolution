# 13 · 1999 · HTTP/1.1 · TLS10

TLS1.0: PRF, HMAC і новий Finished.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --tls10
```

Перевірка в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run --project project -- --tls10-client PIN
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../12_1997_HTTP1.1_CHUNKED/project project
```

[Усі етапи та клієнтські команди](../README.md).
