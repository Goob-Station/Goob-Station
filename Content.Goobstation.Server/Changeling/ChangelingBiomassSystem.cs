using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Chat.Systems;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingBiomassSystem : SharedChangelingBiomassSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

    }

    protected override void DoCough(Entity<ChangelingBiomassComponent> ent)
    {
        _chat.TryEmoteWithChat(ent, ent.Comp.CoughEmote, ignoreActionBlocker: true, forceEmote: true);
    }
}
