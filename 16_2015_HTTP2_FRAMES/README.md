# 16 · 2015 · HTTP/2 · FRAMES

HTTP/2: preface, SETTINGS і кадри через TCP.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h2
```

Цей крок показує preface та службові кадри HTTP/2. Відповіді GET/HEAD з’являються на етапі 19.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../15_2010_HTTP1.1_TLS_RENEGOTIATION/project project
```

[Усі етапи та клієнтські команди](../README.md).
