import { memo, useState } from "react";
import type { ComponentProps } from "react";
import { BodyCompressionDemo } from "../demos/BodyCompressionDemo";
import { RequestCompressionDemo } from "../demos/RequestCompressionDemo";
import { ChannelDiagram } from "../demos/ChannelDiagram";
import { SlideAnimation } from "../demos/SlideAnimation";
import { TlsChanges, TlsExchange, TlsFinished, AlpnChoice } from "../demos/TlsNegotiation";
import { QuicProtection, QuicOffsets, QuicFlow } from "../demos/QuicWorkbench";
import { QuicRecovery } from "../demos/QuicRecovery";
import { QuicReliability } from "../demos/QuicReliability";
import { Gallery, UdpStack, OperatorLimits, InstagramRollout } from "../demos/DeploymentStories";
import WindowCredits from "../demos/WindowCredits";
import { FrameDemo } from "../demos/FrameDemo";
import { HostPrivacyDemo } from "../demos/HostPrivacyDemo";
import { HostValidationDemo } from "../demos/HostValidationDemo";
import { HpackDemo } from "../demos/HpackDemo";
import { HpackStaticTable } from "../demos/HpackStaticTable";
import { PacketLossDemo } from "../demos/PacketLossDemo";
import { ServerMemoryDemo } from "../demos/ServerMemoryDemo";
import { SslTrafficDemo } from "../demos/SslTrafficDemo";
import { SlideContent } from "./SlideContent";

import {
    Timeline,
    ResourceTree,
    SslDesign,
    KeepAlive,
    HttpQueue,
} from "../demos/HttpOneDiagrams";
import {
    RealtimeYears,
    WebSocketChatDiagram,
    SocketBytes,
    WebSocketSecurity,
} from "../demos/RealtimeDiagrams";
import { RtcChat } from "../demos/RtcChat";
import {
    StreamAssembly,
    Continuation,
    HttpTwoState,
} from "../demos/HttpTwoDiagrams";
import {
    HpackLiteral,
    HpackRepeat,
    HuffmanBits,
} from "../demos/HeaderCompressionDiagrams";

const diagrams = {
    "body-gzip": () => <BodyCompressionDemo mode="gzip" />,
    "request-compression": RequestCompressionDemo,
    "body-codecs": () => <BodyCompressionDemo mode="modern" />,
    gallery: Gallery,
    "tls-changes": TlsChanges,
    "tls-exchange": TlsExchange,
    "tls-finished": TlsFinished,
    "alpn-choice": AlpnChoice,
    "udp-stack": UdpStack,
    "quic-protection": QuicProtection,
    "quic-reliability": QuicReliability,
    "quic-offsets": QuicOffsets,
    "quic-flow": QuicFlow,
    "quic-recovery": QuicRecovery,
    "operator-limits": OperatorLimits,
    "instagram-rollout": InstagramRollout,
    resources: ResourceTree,
    "ssl-design": SslDesign,
    keepalive: KeepAlive,
    "realtime-years": RealtimeYears,
    "ws-chat": WebSocketChatDiagram,
    "ws-bytes": SocketBytes,
    wss: WebSocketSecurity,
    rtc: RtcChat,
    queue: HttpQueue,
    streams: StreamAssembly,
    literal: HpackLiteral,
    repeat: HpackRepeat,
    "huffman-bits": HuffmanBits,
    windows: WindowCredits,
    continuation: Continuation,
    "h2-state": HttpTwoState,
};

type Props = Omit<
    ComponentProps<typeof SlideContent>,
    "lead" | "change" | "visual" | "exercise"
>;

function SslTrafficSlide(props: Props) {
    const [ssl, setSsl] = useState(true);
    return (
        <SlideContent
            {...props}
            change={
                ssl
                    ? props.slide.change
                    : "Без SSL рядок запиту, HTTP-заголовки й тіло передаються відкрито."
            }
            visual={<SslTrafficDemo ssl={ssl} onModeChange={setSsl} />}
        />
    );
}

export const Slide = memo(function Slide(props: Props) {
    const { slide } = props;
    const { animation } = slide;

    if (animation) {
        return (
            <SlideContent
                {...props}
                visual={
                    <SlideAnimation
                        animation={animation}
                        active={props.active !== false}
                    />
                }
                exercise={animation.kind === "hpack" && <HpackStaticTable />}
            />
        );
    }

    if (slide.interactive && Object.hasOwn(diagrams, slide.interactive)) {
        const Diagram = diagrams[slide.interactive as keyof typeof diagrams];
        // Stop live media and release peer connections on slide exit.
        const visual =
            slide.interactive === "rtc" && props.active === false ? null : (
                <Diagram />
            );
        return <SlideContent {...props} visual={visual} />;
    }

    switch (slide.interactive) {
        case "timeline":
            return (
                <SlideContent
                    {...props}
                    visual={<Timeline active={props.active !== false} />}
                />
            );
        case "ssl-traffic":
            return <SslTrafficSlide {...props} />;
        case "host":
            return (
                <SlideContent
                    {...props}
                    visual={<HostPrivacyDemo />}
                    exercise={<HostValidationDemo />}
                />
            );
        case "frame":
            return <SlideContent {...props} visual={<FrameDemo />} />;
        case "hpack": {
            const schemeStart = slide.why.indexOf("http://");
            const lead =
                schemeStart < 0 ? (
                    slide.why
                ) : (
                    <>
                        {slide.why.slice(0, schemeStart)}
                        <span className="hp-scheme">http</span>
                        {slide.why.slice(schemeStart + 4)}
                    </>
                );
            return (
                <SlideContent {...props} lead={lead} visual={<HpackDemo />} />
            );
        }
        case "loss":
            return <SlideContent {...props} exercise={<PacketLossDemo />} />;
        case "server-memory":
            return <SlideContent {...props} visual={<ServerMemoryDemo />} />;
        default:
            return (
                <SlideContent
                    {...props}
                    visual={
                        slide.channel && (
                            <ChannelDiagram channel={slide.channel} />
                        )
                    }
                />
            );
    }
});
