# 15 · 2010 · HTTP/1.1 · TLS_RENEGOTIATION

TLS: renegotiation indication.

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
diff -ru --exclude=bin --exclude=obj ../14_2002_HTTP1.1_TLS_AES/project project
```

[Усі етапи та клієнтські команди](../README.md).
