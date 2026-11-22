internal sealed class QpackDynamicTable(int maxCapacity)
{
    private readonly List<HeaderField> entries = [];
    private int capacity;
    private int size;

    public int MaxCapacity { get; } = maxCapacity;
    public int InsertCount { get; private set; }

    public void SetCapacity(int value)
    {
        if (value > MaxCapacity)
        {
            throw new InvalidDataException("QPACK table capacity exceeds SETTINGS.");
        }

        capacity = value;
        EvictToFit(0);
    }

    public void Insert(HeaderField field)
    {
        if (field.TableSize > capacity)
        {
            throw new InvalidDataException("QPACK entry exceeds table capacity.");
        }

        EvictToFit(field.TableSize);
        entries.Add(field);
        size += field.TableSize;
        InsertCount++;
    }

    public HeaderField AtRelative(int index) => At(InsertCount - 1L - index);

    public HeaderField At(long absoluteIndex)
    {
        var index = absoluteIndex - (InsertCount - entries.Count);
        if (index < 0 || index >= entries.Count)
        {
            throw new InvalidDataException("Invalid QPACK dynamic index.");
        }

        return entries[(int)index];
    }

    private void EvictToFit(int extra)
    {
        while (size + extra > capacity)
        {
            size -= entries[0].TableSize;
            entries.RemoveAt(0);
        }
    }
}
