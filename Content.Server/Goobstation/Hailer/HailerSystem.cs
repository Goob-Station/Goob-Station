using Content.Shared.Hailer.Components;
using Content.Shared.Actions;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Server.Chat.Systems;
using Robust.Shared.Random;

namespace Content.Shared.Hailer.EntitySystems;

public sealed class HailerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionsComponent, HailerActionEvent>(OnHail);
        SubscribeLocalEvent<HailerComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<HailerComponent, GotUnequippedEvent>(OnGotUnequipped);
    }
    private void OnGotEquipped(EntityUid uid, HailerComponent component, GotEquippedEvent args)
    {
        if (args.SlotFlags == SlotFlags.MASK)
        {
            _actionsSystem.AddAction(args.Equipee, ref component.HailActionEntity, component.HailerAction, args.Equipee);
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
        int rInt = (int) _random.NextDouble(0, _sounds.Length);
        _audio.PlayPvs(_sounds[rInt], uid);
        _delays[uid] = _timing.CurTime.Add(_fixed_delay);
        _chat.TrySendInGameICMessage(uid, Loc.GetString("hail-" + rInt), InGameICChatType.Speak, ChatTransmitRange.GhostRangeLimit, nameOverride: Name(uid) + "(SecMask)", checkRadioPrefix: false);
    }
}
