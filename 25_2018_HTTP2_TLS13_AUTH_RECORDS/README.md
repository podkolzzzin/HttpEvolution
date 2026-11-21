# 25 · 2018 · TLS 1.3 · Автентифікація й захищені записи

[Program.cs](Program.cs) продовжує обмін ключами та передає перший запис із даними застосунку. Запуск із цієї папки:

```sh
dotnet run
```

Сервер виводить дані сертифіката localhost та послідовність повідомлень від EncryptedExtensions до Finished. У CertificateVerify він підписує історію handshake ключем ECDSA. Потім демо обчислює клієнтський Finished у пам’яті та передає його на перевірку серверу.

Після `Client Finished verified: True` сервер записує зашифроване повідомлення в MemoryStream. Клієнт читає його з ключем серверного трафіку й отримує внутрішній тип 23 та текст `Hello inside a TLS1.3 record!`. Наступний [приклад HTTPS](../28_2018_HTTP2_TLS13_HTTPS/) виконує handshake із мережевим клієнтом.

Порівняти дві версії обробника handshake:

```sh
diff -u ../22_2018_HTTP2_TLS13_KEY_EXCHANGE/TlsHandshake.cs TlsHandshake.cs
```

[Усі етапи](../README.md).
