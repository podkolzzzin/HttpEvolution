# 17 · 2015 · HTTP/2 · HPACK and Huffman coding

[Program.cs](Program.cs) decodes two requests on the same connection. Run it from this directory.

```sh
dotnet run
```

Both requests print the same method (`GET`), scheme (`http`), path (`/`), and host (`www.example.com`). Compare the sizes of the header blocks.

- The first request uses 17 bytes. Static table indices encode the method, scheme, and path. Huffman coding reduces the host to 12 bytes. The decoder adds `:authority = www.example.com` to the dynamic table.
- The repeated request uses four bytes. Its final byte, `BE`, points to entry 62 in the same decoder’s dynamic table.

[Hpack.cs](../common/HeaderCompression/Hpack.cs) handles indices. [HeaderCompression.cs](../common/HeaderCompression/HeaderCompression.cs) handles the encoding flag and string length. [Huffman.cs](../common/HeaderCompression/Huffman.cs) decodes the host bits.

Compare with the previous step.

```sh
diff -u ../16_2015_HTTP2_FRAMES/Program.cs Program.cs
```

[All steps](../README.md).
