// Regenerate the offline demo with Node 22.15+ from any directory.
import assert from "node:assert/strict";
import { readFile, writeFile } from "node:fs/promises";
import {
    brotliCompressSync, brotliDecompressSync, constants, gzipSync, gunzipSync,
    zstdCompressSync, zstdDecompressSync,
} from "node:zlib";

const html = "<html>\n<body>\n<ul>\n" + Array.from({ length: 24 }, (_, i) =>
    `<li><a href="/photos/${i + 1}">Photograph ${i + 1}</a></li>`).join("\n")
    + "\n</ul>\n</body>\n</html>\n";

function measure(body) {
    const gzip = gzipSync(body, { level: 6 });
    const br = brotliCompressSync(body, {
        params: { [constants.BROTLI_PARAM_QUALITY]: 5 },
    });
    const zstd = zstdCompressSync(body, {
        params: { [constants.ZSTD_c_compressionLevel]: 3 },
    });
    for (const decoded of [gunzipSync(gzip), brotliDecompressSync(br), zstdDecompressSync(zstd)]) {
        assert.deepEqual(decoded, body);
    }
    return { identity: body.length, gzip: gzip.length, br: br.length, zstd: zstd.length };
}

const jpeg = await readFile(new URL("../src/assets/gallery/as17-148-22727.jpg", import.meta.url));
const examples = {
    html: { source: html, sizes: measure(Buffer.from(html)) },
    ok: { source: "OK", sizes: measure(Buffer.from("OK")) },
    jpeg: { sizes: measure(jpeg) },
};
await writeFile(new URL("../src/data/body-compression.json", import.meta.url),
    JSON.stringify(examples, null, 4) + "\n");
