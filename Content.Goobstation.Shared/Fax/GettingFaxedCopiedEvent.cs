using Content.Shared.Fax;
using Content.Shared.Fax.Components;

namespace Content.Goobstation.Shared.Fax;

[ByRefEvent]
public record struct GettingFaxedCopiedEvent(Entity<FaxMachineComponent> Fax, FaxCopyMessage Args, bool Handled = false);
