namespace Content.Goobstation.Common.Charges;

public sealed class ChargesChangedEvent : EntityEventArgs
{
    public readonly int CurrentCharges;
    public readonly int LastCharges;

    public ChargesChangedEvent(int current, int last)
    {
        CurrentCharges = current;
        LastCharges = last;
    }
}
