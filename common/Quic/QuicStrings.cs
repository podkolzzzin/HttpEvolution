internal static class QuicStrings
{
    public const string Http3Alpn = "h3";
    public const string KeyLabelPrefix = "quic ";
    public const string HeaderProtectionLabel = KeyLabelPrefix + "hp";
    public const string ServerInitialLabel = "server in";
    public const string ClientInitialLabel = "client in";
    public const string HandshakeComplete = "QUIC handshake complete; ALPN " + Http3Alpn + ".";
    public const string ListenerProtocol = "QUIC/TLS 1.3";
    public const string MissingTransportParameters = "Missing QUIC parameters.";
    public const string UnexpectedCryptoLevel = "Unexpected CRYPTO level.";
    public const string InvalidPacketFlags = "Invalid reserved/key-phase bits.";
    public const string WrongFrameLevel = "Frame at the wrong encryption level.";
    public const string InvalidConnectionId = "Invalid new connection ID.";
    public const string UnknownFrame = "Unknown QUIC frame.";
    public const string OversizedOutgoingFrame = "Oversized outgoing QUIC frame.";
}
