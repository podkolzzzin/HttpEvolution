# 17 · 2015 · HTTP/2 · HPACK і кодування Гаффмана

[Program.cs](Program.cs) декодує два запити в межах одного з’єднання. Запуск із цієї папки:

```sh
dotnet run
```

В обох запитах виводяться той самий метод GET, схема http, шлях / і домен www.example.com. Зверни увагу на розмір блоку заголовків:

- У першому запиті (17 байтів) метод, схему й шлях задають індекси статичної таблиці. Домен займає 12 байтів завдяки кодуванню Гаффмана. Декодер додає пару `:authority = www.example.com` до динамічної таблиці.
- У повторному запиті (4 байти) останній байт `BE` вказує на запис 62. Той самий декодер уже пам’ятає домен.

За індекси відповідає [Hpack.cs](../common/HeaderCompression/Hpack.cs), за ознаку кодування та довжину рядка — [HeaderCompression.cs](../common/HeaderCompression/HeaderCompression.cs), а біти домену декодує [Huffman.cs](../common/HeaderCompression/Huffman.cs).

Порівняти з попереднім етапом:

```sh
diff -u ../16_2015_HTTP2_FRAMES/Program.cs Program.cs
```

[Усі етапи](../README.md).
