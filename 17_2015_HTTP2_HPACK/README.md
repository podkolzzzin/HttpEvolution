# 17 · 2015 · HTTP/2 · HPACK

HPACK: таблиці заголовків.

Самостійний проєкт у `project/`. Із цієї папки:

```sh
dotnet run --project project -- --h2
```

Додається декодування заголовків. Відповіді GET/HEAD з’являються на етапі 19.

Порівняти з попереднім кроком із цієї папки:

```sh
diff -ru --exclude=bin --exclude=obj ../16_2015_HTTP2_FRAMES/project project
```

[Усі етапи та клієнтські команди](../README.md).
