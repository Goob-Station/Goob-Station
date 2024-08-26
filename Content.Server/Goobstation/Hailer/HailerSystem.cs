using Content.Shared.Hailer.Components;
using Content.Shared.Actions;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Audio.Systems;
using Ra = System.Random;
using Robust.Shared.Timing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared.Hailer.EntitySystems;

public sealed class HailerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionsComponent, HailerActionEvent>(OnHail);
        SubscribeLocalEvent<HailerComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<HailerComponent, GotUnequippedEvent>(OnGotUnequipped);
    }
    private void OnGetItemActions(EntityUid uid, HailerComponent component, GetItemActionsEvent args)
    {
        if (args.SlotFlags == SlotFlags.MASK)
        {
            _actionsSystem.AddAction(args.User, ref component.HailActionEntity, component.HailerAction, args.User);
        }
    }
    private void OnGotUnequipped(EntityUid uid, HailerComponent component, GotUnequippedEvent args)
    {
        if (args.SlotFlags == SlotFlags.MASK)
        {
            _actionsSystem.RemoveAction(args.Equipee, component.HailActionEntity);
        }
    }
    string[] _sounds = [
        "/Audio/Goobstation/Hailer/asshole.ogg",
        "/Audio/Goobstation/Hailer/bash.ogg",
        "/Audio/Goobstation/Hailer/bobby.ogg",
        "/Audio/Goobstation/Hailer/compliance.ogg",
        "/Audio/Goobstation/Hailer/dontmove.ogg",
        "/Audio/Goobstation/Hailer/dredd.ogg",
        "/Audio/Goobstation/Hailer/floor.ogg",
        "/Audio/Goobstation/Hailer/freeze.ogg",
        "/Audio/Goobstation/Hailer/halt.ogg",
    ];
    Dictionary<EntityUid, TimeSpan> _delays = new Dictionary<EntityUid, TimeSpan>();
    TimeSpan _fixed_delay = TimeSpan.FromSeconds(2);
    private void OnHail(EntityUid uid, ActionsComponent component, ref HailerActionEvent args)
    {
        if (args.Handled)
            return;
        // No hail spam check.
        if (_delays.ContainsKey(uid))
        {
            if (_timing.CurTime < _delays[uid])
            {
                return;
            }
        }
        Ra r = new Ra();
        int rInt = r.Next(0, _sounds.Length);
        _audio.PlayPvs(_sounds[rInt], uid);
        _delays[uid] = _timing.CurTime.Add(_fixed_delay);
    }
}
