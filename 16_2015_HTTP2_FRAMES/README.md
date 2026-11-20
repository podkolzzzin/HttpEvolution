# 16 · 2015 · HTTP/2 · FRAMES

HTTP/2: preface, SETTINGS і кадри через TCP.

Файли проєкту лежать безпосередньо в цій папці. Запуск:

```sh
dotnet run -- --h2
```

Цей крок показує preface та службові кадри HTTP/2. Відповіді GET/HEAD з’являються на етапі 19.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj --exclude=README.md ../15_2010_HTTP1.1_TLS_RENEGOTIATION .
```

[Усі етапи та клієнтські команди](../README.md).

Незмінні допоміжні файли підключені з [`common/`](../common/) через
`Compile Include` та `Link` у `HttpEvolution.csproj`. В IDE вони
відображаються у віртуальній папці `common`.
