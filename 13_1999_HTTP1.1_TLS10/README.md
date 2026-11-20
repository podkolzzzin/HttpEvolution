# 13 · 1999 · HTTP/1.1 · TLS10

TLS1.0: PRF, HMAC і новий Finished.

Проєкт запускається з цієї папки; linked files видно в IDE поруч із локальним кодом:

```sh
dotnet run -- --tls10
```

Перевірка в іншому терміналі (PIN із виводу сервера):

```sh
dotnet run -- --tls10-client PIN
```

Код, доданий або змінений на цьому етапі:

- [LegacyCipher.cs](LegacyCipher.cs).
- [LegacyRecords.cs](LegacyRecords.cs).
- [LegacySsl.Client.cs](LegacySsl.Client.cs).
- [LegacySsl.cs](LegacySsl.cs).
- [Program.cs](Program.cs).
- [Tls10Crypto.cs](../common/Tls10Crypto.cs).

Порівняти фактичний код із попереднім кроком (код завершення 1 означає відмінності):

```sh
diff -u ../04_1996_HTTP1.0_SSL3_RECORDS/LegacyCipher.cs LegacyCipher.cs
diff -u ../05_1996_HTTP1.0_SSL3_MESSAGES/LegacyRecords.cs LegacyRecords.cs
diff -u ../09_1996_HTTP1.0_SSL3_CLIENT/LegacySsl.Client.cs LegacySsl.Client.cs
diff -u ../08_1996_HTTP1.0_SSL3_HTTPS/LegacySsl.cs LegacySsl.cs
diff -u ../12_1997_HTTP1.1_CHUNKED/Program.cs Program.cs
diff -u /dev/null ../common/Tls10Crypto.cs
```

Спільні допоміжні файли зберігаються в [`common/`](../common/).
Потрібні попередні версії інших файлів проєкт підключає безпосередньо з етапів,
де вони вперше з’явилися. Повний склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).
Нову історичну версію зберігай у новому етапі, залишаючи попередню на місці.

[Усі етапи та клієнтські команди](../README.md).
