namespace Content.Goobstation.Common.Wizard.Chuuni;

public struct HandleSpellInvocationEvent(int school, EntityUid performer)
{
    public int MagicSchool = school;
    public EntityUid Performer = performer;
    public LocId? Invocation = null;
}
