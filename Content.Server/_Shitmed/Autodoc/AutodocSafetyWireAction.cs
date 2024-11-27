using Content.Server.Wires;
using Content.Shared._Shitmed.Autodoc.Components;
using Content.Shared._Shitmed.Autodoc.Systems;
using Content.Shared.Wires;

namespace Content.Server._Shitmed.Autodoc;

public sealed partial class AutodocSafetyWireAction : ComponentWireAction<AutodocComponent>
{
    public override Color Color { get; set; } = Color.Red;
    public override string Name { get; set; } = "wire-name-autodoc-safety";

    public override StatusLightState? GetLightState(Wire wire, AutodocComponent comp)
        => comp.RequireSleeping ? StatusLightState.On : StatusLightState.Off;

    public override object StatusKey { get; } = AutodocWireStatus.SafetyIndicator;

    public override bool Cut(EntityUid user, Wire wire, AutodocComponent comp)
    {
        var uid = wire.Owner;
        EntityManager.System<SharedAutodocSystem>().SetSafety((uid, comp), false);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, AutodocComponent comp)
    {
        var uid = wire.Owner;
        EntityManager.System<SharedAutodocSystem>().SetSafety((uid, comp), true);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, AutodocComponent comp)
    {
        var uid = wire.Owner;
        EntityManager.System<SharedAutodocSystem>().SetSafety((uid, comp), !comp.RequireSleeping);
    }
}
