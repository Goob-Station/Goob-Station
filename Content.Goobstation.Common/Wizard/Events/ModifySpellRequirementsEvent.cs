namespace Content.Goobstation.Common.Wizard.Events;

[ByRefEvent]
public record struct ModifySpellRequirementsEvent(int SlotFlags, int RequiredSlots, bool RequiresSpeech, EntityUid Performer);