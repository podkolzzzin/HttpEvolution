internal static class TlsConstants
{
    public static readonly byte[] Sha1WithRsaAlgorithmIdentifier =
        Convert.FromHexString("300d06092a864886f70d0101050500");

    public static readonly byte[] HelloRetryRequestRandom =
        Convert.FromHexString("cf21ad74e59a6111be1d8c021e65b891c2a211167abb8c5e079e09e2c8a8339c");
}
