import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { App } from "./deck/App";
import "./styles/base.css";
import "./styles/deck.css";
import "./styles/slides.css";
import "./styles/demos.css";
import "./styles/traffic.css";
import "./styles/memory.css";
import "./styles/hpack.css";

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <App />
    </StrictMode>,
);
