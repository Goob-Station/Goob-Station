namespace Content.Shared._Trauma.Utility;

/// <summary>
/// A ringbuffer that stores items in LIFO fashion, by pushing and popping items.
/// Trying to add more items than the buffer supports will pop the oldest one.
/// If used with entity-specific data it should be <c>Reset</c> when the round restarts to avoid problems.
/// </summary>
public sealed class RingBuffer<T>
{
    private T[] _items;

    private int _offset;

    /// <summary>
    /// How many items are stored
    /// </summary>
    public int Count { get; private set; } = 0;

    /// <summary>
    /// How many items can be stored.
    /// </summary>
    public int Capacity => _items.Length;

    /// <summary>
    /// True if no items are stored.
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Create a ring buffer with a given capacity.
    /// </summary>
    public RingBuffer(int capacity)
    {
        _items = new T[capacity];
    }

    /// <summary>
    /// Push a new item to the buffer which can be popped after all older items.
    /// If it is full, it will return the oldest item which has been removed to fit this one.
    /// The caller must handle the removal of the old item if it's not null.
    /// </summary>
    public bool Push(T item, out T old)
    {
        var popped = false;
        old = default!;
        if (Count == Capacity)
            popped = Pop(out old);

        _items[NewIndex] = item;
        Count++;
        return popped;
    }

    /// <summary>
    /// Pops the oldest item, returning false if empty.
    /// </summary>
    public bool Pop(out T item)
    {
        item = default!;
        if (Count == 0)
            return false;

        item = _items[OldIndex];
        Count--;
        _offset++;
        _offset %= Capacity; // prevent it wrapping if it gets spammed
        return true;
    }

    /// <summary>
    /// Gets the oldest item without modifying anything, returning false if empty.
    /// </summary>
    public bool Peek(out T item)
    {
        item = default!;
        if (Count == 0)
            return false;

        item = _items[OldIndex];
        return true;
    }

    /// <summary>
    /// Clear all items and allow changing the backing array to have a new capacity.
    /// </summary>
    public void Reset(int capacity)
    {
        Reset();
        if (capacity != Capacity)
            _items = new T[capacity];
    }

    /// <summary>
    /// Clear all items without changing the backing array's capacity.
    /// </summary>
    public void Reset()
    {
        _offset = 0;
        Count = 0;
    }


    /// <summary>
    /// Call a delegate for every item by-ref
    /// </summary>
    public void VisitItems(Visitor visitor)
    {
        for (int i = 0; i < Count; i++)
        {
            visitor(ref _items[Index(i)]);
        }
    }

    /// <summary>
    /// Call a delegate for every item by value and clear the buffer.
    /// </summary>
    public void Drain(DrainVisitor visitor)
    {
        for (int i = 0; i < Count; i++)
        {
            visitor(_items[Index(i)]);
        }
        Reset();
    }

    public delegate void Visitor(ref T item);
    public delegate void DrainVisitor(T item);

    // index of the oldest item
    private int OldIndex => Index(0);
    // index of where to insert a new item
    private int NewIndex => Index(Count);
    // index to the array for a given item number
    private int Index(int i)
        => (_offset + i) % Capacity;
}
