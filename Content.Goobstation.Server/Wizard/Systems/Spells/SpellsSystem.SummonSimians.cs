using Content.Goobstation.CommonShared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Shared.Actions.Components;
using Content.Shared.Chat;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Physics;
using Content.Shared.Random.Helpers;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class SpellsSystem
{
    protected override void SpawnMonkeysRelay(SummonSimiansEvent ev)
    {
        if (!_prototypeManager.TryIndex(ev.Mobs, out var mobs) || !_prototypeManager.TryIndex(ev.Weapons, out var weapons))
            return;

        if (mobs.Weights.Count == 0)
            return;

        var positions = GetSpawnCoordinatesAroundPerformer(ev.Performer,
            ev.Range,
            ev.Amount,
            ev.SpawnAngle,
            (int) CollisionGroup.MobMask);
        foreach (var pos in positions)
        {
            var mob = Spawn(mobs.Pick(_random), pos);

            if (!_handsQuery.TryComp(mob, out var hands) || hands.Count == 0 || weapons.Weights.Count == 0)
                continue;

            var weapon = Spawn(weapons.Pick(_random), pos);

            if (!_hands.TryPickupAnyHand(mob, weapon, true, false, false, hands))
            {
                QueueDel(weapon);
                continue;
            }

            FadingTimedDespawnComponent? weaponDespawn;
            if (_timedDespawnQuery.TryComp(mob, out var despawn))
            {
                weaponDespawn = EnsureComp<FadingTimedDespawnComponent>(weapon);
                weaponDespawn.Lifetime = despawn.Lifetime + 30f;
                weaponDespawn.FadeOutTime = 4f;
                Dirty(weapon, weaponDespawn);
            }
            else if (_fadingTimedDespawnQuery.TryComp(mob, out var fading))
            {
                weaponDespawn = EnsureComp<FadingTimedDespawnComponent>(weapon);
                weaponDespawn.Lifetime = fading.Lifetime + 30f;
                weaponDespawn.FadeOutTime = 4f;
                Dirty(weapon, weaponDespawn);
            }
        }
    }

    protected override void OnMonkeyAscensionRelay(Entity<MindContainerComponent> ent, ref SummonSimiansMaxedOutEvent args)
    {
        var (uid, comp) = ent;
        if (!TryComp(comp.Mind, out MindComponent? mindComp) ||
            !TryComp(comp.Mind.Value, out ActionsContainerComponent? container))
            return;

        var hasMaxLevelSimians = false;
        var hasGorillaForm = false;
        foreach (var (action, _) in _actions.GetActions(uid))
        {
            if (!hasGorillaForm && _tag.HasTag(action, args.GorillaFormTag))
                hasGorillaForm = true;

            if (!_tag.HasTag(action, args.MaxLevelTag))
                continue;

            if (TryComp(action, out StoreRefundComponent? refund))
                StoreSystem.DisableListingRefund(refund.Data);

            hasMaxLevelSimians = true;
        }

        if (hasGorillaForm || !hasMaxLevelSimians)
            return;

        _actions.AddAction(comp.Mind.Value, args.Action);

        if (!_playerManager.TryGetSessionById(mindComp.UserId, out var session))
            return;

        var message = Loc.GetString("spell-summon-simians-maxed-out-message");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToOne(ChatChannel.Server,
            message,
            wrappedMessage,
            default,
            false,
            session.Channel,
            args.MessageColor);
    }
}