# 09 · 1996 · HTTP/1.0 · SSL3_CLIENT

SSL3: TcpClient із перевіркою SPKI pin.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --ssl3
```

Перевірка в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run -- --ssl3-client PIN
```

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../08_1996_HTTP1.0_SSL3_HTTPS .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
