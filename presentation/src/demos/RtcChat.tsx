import { useEffect, useRef, useState } from "react";

// Both peers live in this tab. The offer and answer travel in memory.
export function RtcChat() {
    const canvas = useRef<HTMLCanvasElement>(null);
    const video = useRef<HTMLVideoElement>(null);
    const channels = useRef<Partial<Record<"А" | "Б", RTCDataChannel>>>({});
    const [sender, setSender] = useState<"А" | "Б">("А");
    const [receiverStream, setReceiverStream] = useState<MediaStream | null>(null);
    const [status, setStatus] = useState("Готово до виклику");
    const [ready, setReady] = useState(false);
    const [messages, setMessages] = useState<string[]>([]);
    const [draft, setDraft] = useState("Привіт!");

    useEffect(() => {
        if (!receiverStream || !canvas.current || !video.current) return;
        const senderCanvas = canvas.current;
        const receiverVideo = video.current;
        const incoming = receiverStream;
        let left: RTCPeerConnection | undefined;
        let right: RTCPeerConnection | undefined;
        let source: MediaStream | undefined;
        let timer: number | undefined;
        let closed = false;
        function clear() {
            closed = true;
            window.clearInterval(timer);
            source?.getTracks().forEach((track) => track.stop());
            left?.close();
            right?.close();
            channels.current = {};
            receiverVideo.srcObject = null;
            receiverVideo.controls = false;
        }
        async function connect() {
            const a = (left = new RTCPeerConnection({ iceServers: [] }));
            const b = (right = new RTCPeerConnection({ iceServers: [] }));
            const ctx = senderCanvas.getContext("2d");
            if (!ctx) throw new Error();
            let tick = 0;
            function draw() {
                if (!ctx) return;
                ctx.fillStyle = "#17392f";
                ctx.fillRect(0, 0, 320, 180);
                ctx.strokeStyle = "#56806f";
                for (let i = 0; i <= 320; i += 32) {
                    ctx.strokeRect(i, 0, 32, 180);
                }
                ctx.fillStyle = "#d3f576";
                ctx.beginPath();
                ctx.arc(
                    160 + Math.sin(tick / 20) * 108,
                    88 + Math.cos(tick / 26) * 40,
                    18,
                    0,
                    Math.PI * 2,
                );
                ctx.fill();
                ctx.font = "15px monospace";
                ctx.fillStyle = "#fff";
                ctx.fillText(
                    "VIDEO  /  " + String(tick++).padStart(4, "0"),
                    14,
                    160,
                );
            }
            draw();
            source = senderCanvas.captureStream(12);
            timer = window.setInterval(draw, 1000 / 12);
            source.getTracks().forEach((track) => a.addTrack(track, source!));
            b.ontrack = (event) => {
                if (!closed) incoming.addTrack(event.track);
            };
            const textChannel = a.createDataChannel("chat");
            function receive(target: "А" | "Б", data: unknown) {
                if (!closed)
                    setMessages((prev) => [
                        ...prev.slice(-2),
                        target + " отримує «" + String(data) + "»",
                    ]);
            }
            channels.current.А = textChannel;
            textChannel.onmessage = (event) => receive("А", event.data);
            textChannel.onopen = () => {
                if (!closed) {
                    setReady(true);
                    setStatus("З’єднано");
                }
            };
            b.ondatachannel = (event) => {
                channels.current.Б = event.channel;
                event.channel.onmessage = (message) =>
                    receive("Б", message.data);
            };
            const pending: [RTCPeerConnection, RTCIceCandidate][] = [];
            const relay = (
                peer: RTCPeerConnection,
                candidate: RTCIceCandidate | null,
            ) => {
                if (!candidate || closed) return;
                if (peer.remoteDescription)
                    void peer.addIceCandidate(candidate).catch(fail);
                else pending.push([peer, candidate]);
            };
            a.onicecandidate = (event) => relay(b, event.candidate);
            b.onicecandidate = (event) => relay(a, event.candidate);
            a.onconnectionstatechange = () => {
                if (a.connectionState === "failed") fail();
            };
            await a.setLocalDescription(await a.createOffer());
            if (closed) return;
            await b.setRemoteDescription(a.localDescription!);
            await b.setLocalDescription(await b.createAnswer());
            if (closed) return;
            await a.setRemoteDescription(b.localDescription!);
            for (const [peer, candidate] of pending)
                await peer.addIceCandidate(candidate);
        }
        function fail() {
            if (closed) return;
            setStatus("Канал недоступний у цьому браузері");
            setReady(false);
            setReceiverStream(null);
        }
        void connect().catch(fail);
        return clear;
    }, [receiverStream]);

    function toggleCall() {
        setMessages([]);
        setReady(false);
        if (receiverStream) {
            setStatus("Виклик завершено");
            setReceiverStream(null);
            return;
        }

        const receiverVideo = video.current;
        if (
            !receiverVideo ||
            typeof MediaStream === "undefined" ||
            typeof RTCPeerConnection === "undefined"
        ) {
            setStatus("WebRTC недоступний у цьому браузері");
            return;
        }
        // Start playback during the click. Firefox can block autoplay by the time ontrack fires.
        const stream = new MediaStream();
        receiverVideo.srcObject = stream;
        void receiverVideo.play().catch(() => {
            if (receiverVideo.srcObject === stream) receiverVideo.controls = true;
        });
        setStatus("З’єднання…");
        setReceiverStream(stream);
    }

    return (
        <div className="protocol-panel rtc-demo">
            <div className="protocol-head">
                <strong>Два локальні учасники</strong>
                <span role="status">{status}</span>
            </div>
            <div className="rtc-screens">
                <figure>
                    <div>
                        <canvas ref={canvas} width="320" height="180" />
                        {!receiverStream && <span>А</span>}
                    </div>
                    <figcaption>Відправник · тестове відео</figcaption>
                </figure>
                <b aria-label="Медіаканал">→</b>
                <figure>
                    <div>
                        <video playsInline muted autoPlay ref={video} />
                        {!ready && <span>Б</span>}
                    </div>
                    <figcaption>Одержувач · відеотрек</figcaption>
                </figure>
            </div>
            <p className="protocol-caption">
                Сигналізація → узгодження каналу · відео через SRTP · текст
                через RTCDataChannel
            </p>
            <div className="rtc-controls">
                <button onClick={toggleCall}>
                    {receiverStream ? "Завершити виклик" : "Почати виклик"}
                </button>
                <form
                    onSubmit={(e) => {
                        e.preventDefault();
                        const channel = channels.current[sender];
                        if (channel?.readyState === "open" && draft.trim())
                            channel.send(draft.trim());
                    }}
                >
                    <label>
                        Від{" "}
                        <select
                            value={sender}
                            onChange={(event) =>
                                setSender(
                                    event.target.value === "Б" ? "Б" : "А",
                                )
                            }
                        >
                            <option>А</option>
                            <option>Б</option>
                        </select>
                    </label>
                    <input
                        value={draft}
                        maxLength={100}
                        aria-label="Текст повідомлення"
                        onChange={(e) => setDraft(e.target.value)}
                    />
                    <button disabled={!ready || !draft.trim()}>
                        Надіслати → {sender === "А" ? "Б" : "А"}
                    </button>
                </form>
            </div>
            <div className="rtc-inbox" aria-live="polite">
                <b>Доставлено</b> {messages.length ? messages.join(" · ") : "—"}
            </div>
        </div>
    );
}
