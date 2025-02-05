using Content.Shared.Emag.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Silicon.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;


namespace Content.Shared.Silicon.Systems;

/// <summary>
/// Handles emagging entities to change their factions.
/// </summary>
public sealed class EmagReplaceFactionsSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly NpcFactionSystem _npcFactionSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmagReplaceFactionsComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, EmagReplaceFactionsComponent comp, ref GotEmaggedEvent args)
    {
        if (!TryComp<NpcFactionMemberComponent>(uid, out var npcFactionMemberComponent))
            return;

        _audio.PlayPredicted(comp.SparkSound, uid, args.UserUid);

        HashSet<ProtoId<NpcFactionPrototype>> newFactions = new();

        for (int i = 0, l = comp.Factions.Count; i < l; i++)
        {
            newFactions.Add(comp.Factions[i]);
        }

        _npcFactionSystem.ClearFactions(uid, false);
        _npcFactionSystem.AddFactions(uid, newFactions);

        if(comp.StunSeconds > 0)
            _stunSystem.TryStun(uid, new TimeSpan(0, 0, 0, comp.StunSeconds), true);

        args.Handled = true;
    }
}
