# 16 · 2015 · HTTP/2 · FRAMES

HTTP/2: preface, SETTINGS і кадри через TCP.

Проєкт запускається з цієї папки; linked files видно в IDE поруч із локальним кодом:

```sh
dotnet run -- --h2
```

Цей крок показує preface та службові кадри HTTP/2. Відповіді GET/HEAD з’являються на етапі 19.

Код, доданий або змінений на цьому етапі:

- [Http2.cs](Http2.cs).
- [Http2Frames.cs](../common/Http2Frames.cs).
- [Program.cs](Program.cs).

Порівняти фактичний код із попереднім кроком (код завершення 1 означає відмінності):

```sh
diff -u /dev/null Http2.cs
diff -u /dev/null ../common/Http2Frames.cs
diff -u ../13_1999_HTTP1.1_TLS10/Program.cs Program.cs
```

Спільні допоміжні файли зберігаються в [`common/`](../common/).
Потрібні попередні версії інших файлів проєкт підключає безпосередньо з етапів,
де вони вперше з’явилися. Повний склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).
Нову історичну версію зберігай у новому етапі, залишаючи попередню на місці.

[Усі етапи та клієнтські команди](../README.md).
