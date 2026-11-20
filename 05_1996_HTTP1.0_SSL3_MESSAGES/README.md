# 05 · 1996 · HTTP/1.0 · SSL3_MESSAGES

SSL3: повідомлення handshake та ChangeCipherSpec.

Проєкт запускається з цієї папки; linked files видно в IDE поруч із локальним кодом:

```sh
dotnet run
```

Це підготовчий крок SSL3: класи вже збираються, але HTTPS ще не підключено. Запускається HTTP/1.0; SSL3 доступний з етапу 08.

Код, доданий або змінений на цьому етапі:

- [LegacyRecords.cs](LegacyRecords.cs).
- [RecordLayer.cs](../common/RecordLayer.cs).

Порівняти фактичний код із попереднім кроком (код завершення 1 означає відмінності):

```sh
diff -u /dev/null LegacyRecords.cs
diff -u /dev/null ../common/RecordLayer.cs
```

Спільні допоміжні файли зберігаються в [`common/`](../common/).
Потрібні попередні версії інших файлів проєкт підключає безпосередньо з етапів,
де вони вперше з’явилися. Повний склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).
Нову історичну версію зберігай у новому етапі, залишаючи попередню на місці.

[Усі етапи та клієнтські команди](../README.md).
