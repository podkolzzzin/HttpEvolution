# 06 · 1996 · HTTP/1.0 · SSL3_IDENTITY

SSL3: сертифікат RSA/SHA-1.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run
```

Це підготовчий крок SSL3: класи вже збираються, але HTTPS ще не підключено. Запускається HTTP/1.0; SSL3 доступний з етапу 08.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../05_1996_HTTP1.0_SSL3_MESSAGES .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
