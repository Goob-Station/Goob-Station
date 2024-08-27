using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Magic;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Store.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly PolymorphSystem _poly = default!;
    [Dependency] private readonly ChainFireballSystem _splitball = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobstate = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly StaminaSystem _stam = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticMagicItemComponent, InventoryRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);

        SubscribeLocalEvent<HereticComponent, EventHereticOpenStore>(OnStore);
        SubscribeLocalEvent<HereticComponent, EventHereticMansusGrasp>(OnMansusGrasp);

        SubscribeAsh();
        SubscribeFlesh();
    }

    private bool TryUseAbility(Entity<HereticComponent> ent, BaseActionEvent args)
    {
        if (args.Handled)
            return false;

        if (!TryComp<HereticActionComponent>(args.Action, out var actionComp))
            return false;

        // check if any magic items are worn
        if (actionComp.RequireMagicItem && !ent.Comp.Ascended)
        {
            if (ent.Comp.CodexActive)
                return true;

            var ev = new CheckMagicItemEvent();
            RaiseLocalEvent(ent, ev);

            if (!ev.Handled)
            {
                _popup.PopupEntity(Loc.GetString("heretic-ability-fail-magicitem"), ent, ent);
                return false;
            }
        }

        // shout the spell out
        if (!string.IsNullOrWhiteSpace(actionComp.MessageLoc))
            _chat.TrySendInGameICMessage(ent, Loc.GetString(actionComp.MessageLoc!), InGameICChatType.Speak, false);

        return true;
    }

    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref InventoryRelayedEvent<CheckMagicItemEvent> args)
    {
        // no need to check fo anythign because the event gets processed only by magic items
        args.Args.Handled = true;
    }

    private void OnStore(Entity<HereticComponent> ent, ref EventHereticOpenStore args)
    {
        if (!TryComp<StoreComponent>(ent, out var store))
            return;

        _store.ToggleUi(ent, ent, store);
    }
    private void OnMansusGrasp(Entity<HereticComponent> ent, ref EventHereticMansusGrasp args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (ent.Comp.MansusGraspActive)
        {
            _popup.PopupEntity(Loc.GetString("heretic-ability-fail"), ent, ent);
            return;
        }

        var st = Spawn("TouchSpellMansus", Transform(ent).Coordinates);

        if (!_hands.TryForcePickupAnyHand(ent, st))
        {
            _popup.PopupEntity(Loc.GetString("heretic-ability-fail"), ent, ent);
            QueueDel(st);
            return;
        }

        ent.Comp.MansusGraspActive = true;
        args.Handled = true;
    }
}
