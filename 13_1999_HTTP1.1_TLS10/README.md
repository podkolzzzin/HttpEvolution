# 13 · 1999 · HTTP/1.1 · TLS10

TLS1.0: PRF, HMAC і новий Finished.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --tls10
```

Перевірка в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run -- --tls10-client PIN
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../12_1997_HTTP1.1_CHUNKED .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
