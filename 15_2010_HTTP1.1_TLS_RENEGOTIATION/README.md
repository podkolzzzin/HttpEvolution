# 15 · 2010 · HTTP/1.1 · TLS_RENEGOTIATION

TLS: renegotiation indication.

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
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../14_2002_HTTP1.1_TLS_AES .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
