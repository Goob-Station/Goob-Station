using Robust.Shared.Timing;

namespace Content.Shared._Trauma.Utility;

/// <summary>
/// A ringbuffer that stores items in order of a popping timespan.
/// Trying to add more items than the buffer supports will pop the oldest one.
/// If used with entity-specific data it should be <c>Reset</c> when the round restarts to avoid problems.
/// </summary>
public sealed class TimedRingBuffer<T>
{
    private IGameTiming _timing;
    private RingBuffer<(TimeSpan, T)> _buffer; // not using inheritance since some semantics change

    /// <summary>
    /// How many items are stored
    /// </summary>
    public int Count => _buffer.Count;

    /// <summary>
    /// How many items can be stored.
    /// </summary>
    public int Capacity => _buffer.Capacity;

    /// <summary>
    /// True if no items are stored.
    /// </summary>
    public bool IsEmpty => _buffer.IsEmpty;

    private TimeSpan _popDelay;

    /// <summary>
    /// How long items last for before being popped.
    /// </summary>
    public TimeSpan PopDelay
    {
        get => _popDelay;
        set
        {
            if (_popDelay == value)
                return;

            var diff = value - _popDelay;
            _popDelay = value;
            // update old times so they're as if they were using this delay all along. preserves order when pushing new items
            _buffer.VisitItems((ref pair) => pair.Item1 += diff);
        }
    }

    /// <summary>
    /// Create a timed ring buffer with a given capacity and pop delay.
    /// </summary>
    public TimedRingBuffer(int capacity, TimeSpan popDelay, IGameTiming timing)
    {
        _buffer = new(capacity);
        _popDelay = popDelay;
        _timing = timing;
    }

    /// <summary>
    /// Push a new item to the buffer which will be popped after some time.
    /// If it is full, it will return the oldest item which has been removed to fit this one.
    /// The caller must handle the removal of the old item if it's not null.
    /// </summary>
    public bool Push(T item, out T old)
    {
        var popTime = _timing.CurTime + _popDelay;
        old = default!;
        if (!_buffer.Push((popTime, item), out var oldPair))
            return false;

        old = oldPair.Item2;
        return true;
    }

    /// <summary>
    /// Immediately pops the oldest item, not checking it against the current time.
    /// </summary>
    public bool PopImmediate(out T item)
    {
        item = default!;
        if (!_buffer.Pop(out var pair))
            return false;

        item = pair.Item2;
        return true;
    }

    /// <summary>
    /// Pops the oldest item that should be popped by the current time.
    /// This is what you use in an update loop.
    /// <summary>
    public bool PopNext(out T item)
    {
        if (!Peek(out var popTime, out item))
            return false;

        var now = _timing.CurTime;
        if (now < popTime)
            return false; // not meant to be popped yet. items are sorted so no future item will be either, you don't have to check every one

        return PopImmediate(out item);
    }

    /// <summary>
    /// Gets the oldest item and when it is meant to be popped.
    /// </summary>
    public bool Peek(out TimeSpan popTime, out T item)
    {
        var valid = _buffer.Peek(out var pair);
        (popTime, item) = pair;
        return valid;
    }

    /// <summary>
    /// Clear all items and allow changing the backing array to have a new capacity.
    /// </summary>
    public void Reset(int capacity)
    {
        _buffer.Reset(capacity);
    }

    /// <summary>
    /// Clear all items without changing the backing array's capacity.
    /// </summary>
    public void Reset()
    {
        _buffer.Reset();
    }
}
