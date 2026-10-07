using Content.Shared._Shitmed.Medical.Surgery.Tools;

namespace Content.Goobstation.Shared.Surgery.Steps.Parts;

public sealed partial class TissueSampleSystem : EntitySystem
{
    [Dependency] private SurgeryToolExamineSystem _toolExamine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TissueSampleComponent, SurgeryToolExaminedEvent>(_toolExamine.OnExamined);
    }
}
