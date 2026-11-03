using Content.Goobstation.Shared.MobCall;
using Content.Server.Chat.Systems;
using Content.Server.NPC.Systems;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.MobCall;

public sealed partial class MobCallSystem : EntitySystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private NPCSystem _npc = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

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
        args.Handled = true;
    }
}
