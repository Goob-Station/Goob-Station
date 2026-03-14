namespace Content.Goobstation.Shared.Cult.Events;

public sealed class NameSelectedMessage : BoundUserInterfaceMessage
{
    private string _text;

    public NameSelectedMessage(string text)
    {
        _text = text;
    }
}
