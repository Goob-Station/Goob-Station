using Content.Shared._Shitmed.Medical.Surgery.Tools;

namespace Content.Goobstation.Shared.Surgery.Steps.Parts;

public sealed class TissueSampleSystem : EntitySystem
{
    [Dependency] private readonly SurgeryToolExamine _toolExamine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TissueSampleComponent, ExaminedEvent>(_toolExamine.OnExamined);
    }
}
