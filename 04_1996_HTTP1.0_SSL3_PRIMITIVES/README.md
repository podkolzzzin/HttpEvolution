# 04 · 1996 · HTTP/1.0 · SSL 3.0 primitives

How does SSL 3.0 protect an HTTP request and carry the response in a record stream?
Run the [console example](Program.cs) from this directory.

```sh
dotnet run
```

Each run demonstrates four parts of SSL 3.0 in memory.

1. [LegacyCipher](LegacyCipher.cs) adds a MAC to the request, encrypts it with 3DES-CBC, and recovers the line `GET / HTTP/1.0`.
2. [LegacyRecords](LegacyRecords.cs) wraps `ServerHelloDone` in a handshake record. The console shows the bytes of both the record and its message.
3. [LegacyIdentity](../common/Tls/LegacyIdentity.cs) provides the server certificate. The demo prints its subject, signature algorithm, and public-key fingerprint (the SPKI pin).
4. The server and client receive the same fixed key block. After `ChangeCipherSpec`, the server sends an encrypted response. The client reads `HTTP/1.0 200 OK` through [TlsStream](../common/Tls/TlsStream.cs) and stops reading after `close_notify`.

Compare with the previous [HTTP/1.0 server](../03_1996_HTTP1.0_HEAD/).

```sh
diff -u ../03_1996_HTTP1.0_HEAD/Program.cs Program.cs
```

In [step 08](../08_1996_HTTP1.0_SSL3_HTTPS/), these pieces work together in an HTTPS server over TCP with a teaching implementation of the SSL 3.0 handshake.

[All steps](../README.md).
