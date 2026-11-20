# 09 · 1996 · HTTP/1.0 · SSL3_CLIENT

SSL3: TcpClient із перевіркою SPKI pin.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --ssl3
```

Перевірка в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run --project project -- --ssl3-client PIN
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../08_1996_HTTP1.0_SSL3_HTTPS/project project
```

[Усі етапи та клієнтські команди](../README.md).
