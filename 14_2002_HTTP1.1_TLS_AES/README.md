# 14 · 2002 · HTTP/1.1 · TLS_AES

TLS1.0: AES-CBC замість 3DES.

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
- [LegacySsl.cs](LegacySsl.cs).

Порівняти фактичний код із попереднім кроком (код завершення 1 означає відмінності):

```sh
diff -u ../13_1999_HTTP1.1_TLS10/LegacyCipher.cs LegacyCipher.cs
diff -u ../13_1999_HTTP1.1_TLS10/LegacyRecords.cs LegacyRecords.cs
diff -u ../13_1999_HTTP1.1_TLS10/LegacySsl.cs LegacySsl.cs
```

Спільні допоміжні файли зберігаються в [`common/`](../common/).
Потрібні попередні версії інших файлів проєкт підключає безпосередньо з етапів,
де вони вперше з’явилися. Повний склад видно в [HttpEvolution.csproj](HttpEvolution.csproj).
Нову історичну версію зберігай у новому етапі, залишаючи попередню на місці.

[Усі етапи та клієнтські команди](../README.md).
