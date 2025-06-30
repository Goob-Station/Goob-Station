using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Shared._Shitmed.PartStatus.Events;

[Serializable, NetSerializable]
public sealed class GetPartStatusEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public GetPartStatusEvent(NetEntity uid)
    {
        Uid = uid;
    }
}

/// <summary>
///     Raised when an entity with woundables is examined. Effectively a copy of ExaminedEvent
///     ///     but without inheriting from it because sealed
/// </summary>
public sealed class PartStatusExaminedEvent : EntityEventArgs
{
    /// <summary>
    ///     The message that will be displayed as the examine text.
    ///     You should use <see cref="PushMarkup"/> and similar instead to modify this,
    ///     since it handles newlines/priority and such correctly.
    /// </summary>
    /// <seealso cref="PushMessage"/>
    /// <seealso cref="PushMarkup"/>
    /// <seealso cref="PushText"/>
    /// <seealso cref="AddMessage"/>
    /// <seealso cref="AddMarkup"/>
    /// <seealso cref="AddText"/>
    private FormattedMessage Message { get; }

    /// <summary>
    ///     Parts of the examine message that will later be sorted by priority and pushed onto <see cref="Message"/>.
    /// </summary>
    private List<ExamineMessagePart> Parts { get; } = new();

    /// <summary>
    ///     The entity performing the examining.
    /// </summary>
    public EntityUid Examiner { get; }

    /// <summary>
    ///     Entity being examined, for broadcast event purposes.
    /// </summary>
    public EntityUid Examined { get; }

    private ExamineMessagePart? _currentGroupPart;

    public PartStatusExaminedEvent(FormattedMessage message, EntityUid examined, EntityUid examiner)
    {
        Message = message;
        Examined = examined;
        Examiner = examiner;
    }

    /// <summary>
    ///     Returns <see cref="Message"/> with all <see cref="Parts"/> appended according to their priority.
    /// </summary>
    public FormattedMessage GetTotalMessage()
    {
        int Comparison(ExamineMessagePart a, ExamineMessagePart b)
        {
            // Try sort by priority, then group, then by string contents
            if (a.Priority != b.Priority)
            {
                // negative so that expected behavior is consistent with what makes sense
                // i.e. a negative priority should mean its at the bottom of the list, right?
                return -a.Priority.CompareTo(b.Priority);
            }

            if (a.Group != b.Group)
            {
                return string.Compare(a.Group, b.Group, StringComparison.Ordinal);
            }

            return string.Compare(a.Message.ToString(), b.Message.ToString(), StringComparison.Ordinal);
        }

        // tolist/clone formatted message so calling this multiple times wont fuck shit up
        // (if that happens for some reason)
        var parts = Parts.ToList();
        var totalMessage = new FormattedMessage(Message);
        parts.Sort(Comparison);

        foreach (var part in parts)
        {
            totalMessage.AddMessage(part.Message);
            if (part.DoNewLine && parts.Last() != part)
                totalMessage.PushNewline();
        }

        totalMessage.TrimEnd();

        return totalMessage;
    }

    /// <summary>
    ///     Message group handling. Call this if you want the next set of examine messages that you're adding to have
    ///     a consistent order with regards to each other. This is done so that client & server will always
    ///     sort messages the same as well as grouped together properly, even if subscriptions are different.
    ///     You should wrap it in a using() block so popping automatically occurs.
    /// </summary>
    public ExamineGroupDisposable PushGroup(string groupName, int priority = 0)
    {
        // Ensure that other examine events correctly ended their groups.
        DebugTools.Assert(_currentGroupPart == null);
        _currentGroupPart = new ExamineMessagePart(new FormattedMessage(), priority, false, groupName);
        return new ExamineGroupDisposable(this);
    }

    /// <summary>
    ///     Ends the current group and pushes its groups contents to the message.
    ///     This will be called automatically if in using a `using` block with <see cref="PushGroup"/>.
    /// </summary>
    private void PopGroup()
    {
        DebugTools.Assert(_currentGroupPart != null);
        if (_currentGroupPart != null && !_currentGroupPart.Message.IsEmpty)
        {
            Parts.Add(_currentGroupPart);
        }

        _currentGroupPart = null;
    }

    /// <summary>
    /// Push another message into this examine result, on its own line.
    /// End message will be grouped by <see cref="priority"/>, then by group if one was started
    /// then by ordinal comparison.
    /// </summary>
    /// <seealso cref="PushMarkup"/>
    /// <seealso cref="PushText"/>
    public void PushMessage(FormattedMessage message, int priority = 0)
    {
        if (message.Nodes.Count == 0)
            return;

        if (_currentGroupPart != null)
        {
            message.PushNewline();
            _currentGroupPart.Message.AddMessage(message);
        }
        else
        {
            Parts.Add(new ExamineMessagePart(message, priority, true, null));
        }
    }

    /// <summary>
    /// Push another message parsed from markup into this examine result, on its own line.
    /// End message will be grouped by <see cref="priority"/>, then by group if one was started
    /// then by ordinal comparison.
    /// </summary>
    /// <seealso cref="PushText"/>
    /// <seealso cref="PushMessage"/>
    public void PushMarkup(string markup, int priority = 0)
    {
        PushMessage(FormattedMessage.FromMarkupOrThrow(markup), priority);
    }

    /// <summary>
    /// Push another message containing raw text into this examine result, on its own line.
    /// End message will be grouped by <see cref="priority"/>, then by group if one was started
    /// then by ordinal comparison.
    /// </summary>
    /// <seealso cref="PushMarkup"/>
    /// <seealso cref="PushMessage"/>
    public void PushText(string text, int priority = 0)
    {
        var msg = new FormattedMessage();
        msg.AddText(text);
        PushMessage(msg, priority);
    }

    /// <summary>
    /// Adds a message directly without starting a newline after.
    /// End message will be grouped by <see cref="priority"/>, then by group if one was started
    /// then by ordinal comparison.
    /// </summary>
    /// <seealso cref="AddMarkup"/>
    /// <seealso cref="AddText"/>
    public void AddMessage(FormattedMessage message, int priority = 0)
    {
        if (message.Nodes.Count == 0)
            return;

        if (_currentGroupPart != null)
        {
            _currentGroupPart.Message.AddMessage(message);
        }
        else
        {
            Parts.Add(new ExamineMessagePart(message, priority, false, null));
        }
    }

    /// <summary>
    /// Adds markup directly without starting a newline after.
    /// End message will be grouped by <see cref="priority"/>, then by group if one was started
    /// then by ordinal comparison.
    /// </summary>
    /// <seealso cref="AddText"/>
    /// <seealso cref="AddMessage"/>
    public void AddMarkup(string markup, int priority = 0)
    {
        AddMessage(FormattedMessage.FromMarkupOrThrow(markup), priority);
    }

    /// <summary>
    /// Adds text directly without starting a newline after.
    /// End message will be grouped by <see cref="priority"/>, then by group if one was started
    /// then by ordinal comparison.
    /// </summary>
    /// <seealso cref="AddMarkup"/>
    /// <seealso cref="AddMessage"/>
    public void AddText(string text, int priority = 0)
    {
        var msg = new FormattedMessage();
        msg.AddText(text);
        AddMessage(msg, priority);
    }

    public struct ExamineGroupDisposable : IDisposable
    {
        private PartStatusExaminedEvent _event;

        public ExamineGroupDisposable(PartStatusExaminedEvent @event)
        {
            _event = @event;
        }

        public void Dispose()
        {
            _event.PopGroup();
        }
    }

    private record ExamineMessagePart(FormattedMessage Message, int Priority, bool DoNewLine, string? Group);
}