using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingBiomassSystem : SharedChangelingBiomassSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingBiomassComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnPolymorphed(Entity<ChangelingBiomassComponent> ent, ref PolymorphedEvent args)
        => _polymorph.CopyPolymorphComponent<ChangelingBiomassComponent>(ent, args.NewEntity);

    protected override void DoCough(Entity<ChangelingBiomassComponent> ent)
    {
        _chat.TryEmoteWithChat(ent, ent.Comp.CoughEmote, ignoreActionBlocker: true, forceEmote: true);
    }
}
