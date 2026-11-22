# 39 · 2022 · HTTP/3 · QPACK

QPACK стискає заголовки HTTP/3. Формати цілих чисел і рядків спільні з HPACK.

Власне демо: [Program.cs](Program.cs). Із цієї папки:

```sh
dotnet run
```

Після прикладу з літералами консоль покаже три події:

1. Потік `0` отримує `HEADERS`, що посилається на першу вставку, і чекає.
2. Потік `4` декодується за статичною таблицею, поки потік `0` чекає.
3. Encoder stream доставляє `x-demo: ready`. Тепер потік `0` декодує своє посилання.

Код, доданий або змінений на цьому етапі:

- [Program.cs](Program.cs).
- [Qpack.cs](../common/HeaderCompression/Qpack.cs).
- [QpackDynamicTable.cs](../common/HeaderCompression/QpackDynamicTable.cs).

Порівняти з попереднім етапом (код завершення 1 означає відмінності):

```sh
diff -u ../38_2021_HTTP3_QUIC_PTO/Program.cs Program.cs
diff -u /dev/null ../common/HeaderCompression/Qpack.cs
```

`Program.cs` завжди локальний і належить цьому демо. Допоміжні класи підключені з [`common/`](../common/) або з етапів, де з’явилася потрібна версія; склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).

[Усі етапи](../README.md).
