import { memo, useState } from "react";
import type { ComponentProps } from "react";
import { ChannelDiagram } from "../demos/ChannelDiagram";
import { FrameDemo } from "../demos/FrameDemo";
import { HostPrivacyDemo } from "../demos/HostPrivacyDemo";
import { HostValidationDemo } from "../demos/HostValidationDemo";
import { HpackDemo } from "../demos/HpackDemo";
import { PacketLossDemo } from "../demos/PacketLossDemo";
import { SslTrafficDemo } from "../demos/SslTrafficDemo";
import { SlideContent } from "./SlideContent";

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

    switch (slide.interactive) {
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
