using Content.Goobstation.Shared.Shizophrenia;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Shizophrenia;

public sealed class ShizophreniaSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SchizophreniaComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(Entity<SchizophreniaComponent> ent, ref GetStatusIconsEvent args)
    {
        if (TryComp<HallucinationComponent>(_player.LocalEntity, out var hallucination) && hallucination.Idx == ent.Comp.Idx)
            args.StatusIcons.Add(_prototypeManager.Index<FactionIconPrototype>("ShizophrenicIcon"));
    }
}
