# 14 · 2002 · HTTP/1.1 · TLS_AES

TLS1.0: AES-CBC замість 3DES.

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
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../13_1999_HTTP1.1_TLS10 .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
