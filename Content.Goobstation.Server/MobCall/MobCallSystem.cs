using Content.Goobstation.Shared.MobCall;
using Content.Server.Chat.Systems;
using Content.Server.NPC.Systems;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.MobCall;

public sealed partial class MobCallSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobCallSourceComponent, MobCallActionEvent>(OnMobCall);
    }

    private void OnMobCall(Entity<MobCallSourceComponent> ent, ref MobCallActionEvent args)
    {
        _chat.TryEmoteWithChat(ent, ent.Comp.Emote, forceEmote: false);
        var mapCoord = _transform.GetMapCoordinates(ent);
        var entCoord = Transform(ent).Coordinates;
        var ents = _lookup.GetEntitiesInRange<MobCallableComponent>(mapCoord, ent.Comp.Range);
        foreach (var (uid, comp) in ents)
        {
            if (_whitelist.IsWhitelistPass(ent.Comp.Whitelist, uid))
                _npc.SetBlackboard(uid, ent.Comp.Key, entCoord);
        }
    }
}
