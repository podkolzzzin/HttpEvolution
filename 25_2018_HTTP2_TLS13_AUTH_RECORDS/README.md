# 25 · 2018 · TLS 1.3 · Authentication and protected records

[Program.cs](Program.cs) continues the key exchange and sends the first application data record. Run it from this directory.

```sh
dotnet run
```

The server prints the localhost certificate details and the message sequence from EncryptedExtensions to Finished. In CertificateVerify, it signs the handshake transcript with its ECDSA key. The demo then computes the client Finished in memory and passes it to the server for verification.

After `Client Finished verified: True`, the server writes an encrypted message to a `MemoryStream`. The client uses the server traffic key to recover inner type 23 and the text `Hello inside a TLS1.3 record!`. The next [HTTPS example](../28_2018_HTTP2_TLS13_HTTPS/) performs the handshake with a network client.

Compare the two versions of the handshake handler.

```sh
diff -u ../22_2018_HTTP2_TLS13_KEY_EXCHANGE/TlsHandshake.cs TlsHandshake.cs
```

[All steps](../README.md).
