internal sealed record HeaderField(string Name, string Value = "")
{
    // Each compression table entry accounts for 32 bytes of overhead.
    public int TableSize => Name.Length + Value.Length + 32;
}
