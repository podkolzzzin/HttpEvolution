# 02b · 1996 · HTTP/1.0 · Стискання тіла

Запусти сервер із кореня репозиторію. Команди клієнта виконуй у другому терміналі.

```sh
dotnet run --project 02b_1996_HTTP1.0_CONTENT_ENCODING
```

## 1996 · Дві версії однієї сторінки

```sh
demo_dir=$(mktemp -d)
cd "$demo_dir"
url=http://127.0.0.1:8096

curl -sS --http1.0 -D - "$url/" -o page.html
curl -sS --http1.0 -D - -H 'Accept-Encoding: x-gzip' "$url/" -o page.gz
wc -c page.html page.gz
gzip -dc page.gz | cmp - page.html && echo 'Після розпакування HTML збігається'
```

У другій відповіді з’явиться `Content-Encoding: x-gzip`. `Content-Length` покаже розмір gzip-потоку, а `Content-Type` в обох відповідях залишиться `text/html; charset=utf-8`. Цей сервер також розуміє сучасну назву `gzip`.

У 1996 році RFC 1945 уже описував `x-gzip` і `x-compress` для HTTP/1.0. `Accept-Encoding` згадано в додатку серед заголовків, які використовували окремі реалізації. [RFC 1945, §3.5](https://www.rfc-editor.org/rfc/rfc1945#section-3.5) · [Додаток D.2.3](https://www.rfc-editor.org/rfc/rfc1945#appendix-D.2.3).

## 2015 · Як дізнатися, чи сервер приймає стиснений POST

```sh
awk 'BEGIN {for (i = 0; i < 64; i++) print "{\"event\":\"view\"}"}' > events.ndjson
gzip -nc events.ndjson > events.gz
wc -c events.ndjson events.gz

curl -si --http1.1 -H 'Content-Type: application/x-ndjson' -H 'Content-Encoding: gzip' \
  --data-binary @events.gz "$url/events"

curl -si --http1.1 -H 'Content-Type: application/x-ndjson' -H 'Content-Encoding: gzip' \
  --data-binary @events.gz "$url/legacy/events"

curl -si --http1.1 -H 'Content-Type: application/x-ndjson' \
  --data-binary @events.ndjson "$url/legacy/events"
```

1. `/events` розпаковує gzip, друкує обидва розміри в терміналі сервера й відповідає `204 No Content`.
2. `/legacy/events` приймає лише нестиснені дані. На gzip відповідає `415 Unsupported Media Type` із `Accept-Encoding: identity`.
3. Після підказки серверу надсилають ті самі події без стискання й отримують `204`.

У листопаді 2015-го RFC 7694 описав, як сервер повідомляє клієнту про підтримувані кодування тіла **запиту** через `Accept-Encoding` у **відповіді**. Підтримка залежить від ресурсу. [RFC 7694, §3](https://www.rfc-editor.org/rfc/rfc7694#section-3).

Для POST демо читає тіло з відомим `Content-Length`. Межа для переданого й розпакованого тіла — 64 КіБ. Зупинка сервера — Ctrl+C.

Порівняй вибір представлення з попереднім етапом (із каталогу проєкту демо).

```sh
diff -u ../02_1996_HTTP1.0_HEADERS/Http10Connection.cs ContentCodingConnection.cs
```

[Код обміну](ContentCodingConnection.cs) · [Усі етапи](../README.md).
