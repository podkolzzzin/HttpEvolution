# 22 · 2018 · TLS 1.3 · Key exchange

[Program.cs](Program.cs) shows how the server derives keys after ClientHello to protect the remaining handshake messages. Run it from this directory.

```sh
dotnet run
```

In the first example, the client supports P-256 and sends an empty `key_share` list. The server replies with HelloRetryRequest. In the second example, a new connection starts with a P-256 key share. [TlsHandshake.cs](TlsHandshake.cs) constructs ServerHello and calculates the ECDH shared secret. The shared secret and handshake transcript produce separate client and server handshake traffic secrets.

The last two lines show the ciphertext with its AES-GCM tag and the decrypted EncryptedExtensions message with inner type 22 (handshake). Both examples run in memory and generate fresh keys on each run.

Compare with the previous step.

```sh
diff -u ../21_2015_HTTP2_CONTINUATION/Program.cs Program.cs
```

[All steps](../README.md).
