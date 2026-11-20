# 05 · 1996 · HTTP/1.0 · SSL3_MESSAGES

SSL3: повідомлення handshake та ChangeCipherSpec.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project
```

Це підготовчий крок SSL3: класи вже збираються, але HTTPS ще не підключено. Запускається HTTP/1.0; SSL3 доступний з етапу 08.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../04_1996_HTTP1.0_SSL3_RECORDS/project project
```

[Усі етапи та клієнтські команди](../README.md).
