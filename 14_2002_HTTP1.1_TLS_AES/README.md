# 14 · 2002 · HTTP/1.1 · TLS_AES

TLS1.0: AES-CBC замість 3DES.

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
diff -ru --exclude=bin --exclude=obj ../13_1999_HTTP1.1_TLS10/project project
```

[Усі етапи та клієнтські команди](../README.md).
