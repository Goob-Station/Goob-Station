using Content.Shared.DeviceLinking;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Lathe;

/// <summary>
/// Lets a lathe produce the last made recipe, controlled by signal port.
/// The port must be added by something else e.g. AutomationSlots
/// </summary>
[RegisterComponent, Access(typeof(LatheAutomationSystem))]
public sealed partial class LatheAutomationComponent : Component
{
    [ViewVariables]
    public LatheRecipePrototype? LastRecipe;

    [DataField]
    public ProtoId<SinkPortPrototype> PrintPort = "LathePrint";
}
