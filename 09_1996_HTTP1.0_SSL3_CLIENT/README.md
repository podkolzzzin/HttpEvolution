# 09 · 1996 · HTTP/1.0 · SSL3_CLIENT

SSL3: TcpClient із перевіркою SPKI pin.

Проєкт запускається з цієї папки; linked files видно в IDE поруч із локальним кодом:

```sh
dotnet run -- --ssl3
```

Перевірка в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run -- --ssl3-client PIN
```

Код, доданий або змінений на цьому етапі:

- [LegacySsl.Client.cs](LegacySsl.Client.cs).
- [Program.cs](Program.cs).

Порівняти фактичний код із попереднім кроком (код завершення 1 означає відмінності):

```sh
diff -u /dev/null LegacySsl.Client.cs
diff -u ../08_1996_HTTP1.0_SSL3_HTTPS/Program.cs Program.cs
```

Спільні допоміжні файли зберігаються в [`common/`](../common/).
Потрібні попередні версії інших файлів проєкт підключає безпосередньо з етапів,
де вони вперше з’явилися. Повний склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).
Нову історичну версію зберігай у новому етапі, залишаючи попередню на місці.

[Усі етапи та клієнтські команди](../README.md).
