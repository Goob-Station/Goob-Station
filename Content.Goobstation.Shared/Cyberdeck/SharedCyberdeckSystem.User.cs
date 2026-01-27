using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Verbs;

namespace Content.Goobstation.Shared.Cyberdeck;

public abstract partial class SharedCyberdeckSystem
{
    private void InitializeUser()
    {
        SubscribeLocalEvent<CyberdeckUserComponent, ComponentStartup>(OnUserInit);
        SubscribeLocalEvent<CyberdeckUserComponent, ComponentShutdown>(OnUserShutdown);
        SubscribeLocalEvent<CyberdeckUserComponent, AccessibleOverrideEvent>(OnCyberdeckAccessible);
        SubscribeLocalEvent<CyberdeckUserComponent, InRangeOverrideEvent>(OnCyberdeckRangeAccessible);
        SubscribeLocalEvent<CyberdeckProjectionComponent, GetVerbsEvent<AlternativeVerb>>(OnProjectionVerbs);
    }

    private void OnUserInit(Entity<CyberdeckUserComponent> ent, ref ComponentStartup args)
    {
        var (uid, component) = ent;

        _actions.AddAction(uid, ref component.HackAction, component.HackActionId);
        _actions.AddAction(uid, ref component.VisionAction, component.VisionActionId);

        if (!_body.TryGetBodyOrganEntityComps<CyberdeckSourceComponent>(uid, out var organs, false)
            || organs.Count == 0)
            return;

        component.ProviderEntity = organs[0].Owner;
        UpdateAlert((uid, component));
    }

    private void OnUserShutdown(Entity<CyberdeckUserComponent> ent, ref ComponentShutdown args)
    {
        var (uid, component) = ent;

        UpdateAlert(ent, true);
        DetachFromProjection(ent);

        _actions.RemoveAction(uid, component.HackAction);
        _actions.RemoveAction(uid, component.VisionAction);
        _actions.RemoveAction(uid, component.ReturnAction);

        PredictedQueueDel(ent.Comp.ProjectionEntity);
    }

    private void OnProjectionVerbs(Entity<CyberdeckProjectionComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!HasComp<StationAiHeldComponent>(user))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("cyberdeck-station-ai-smite-verb"),
            Act = () =>
            {
                if (!_cyberdeckUserQuery.TryComp(ent.Comp.RemoteEntity, out var userComp))
                    return;

                var remote = ent.Comp.RemoteEntity.Value;
                var sound = ent.Comp.CounterHackSound;
                DetachFromProjection((ent.Comp.RemoteEntity.Value, userComp));

                // All components are in common so i can't just put it in YAML. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
                _damage.TryChangeDamage(remote, new DamageSpecifier() { DamageDict = new Dictionary<string, FixedPoint2>() { ["Shock"] = 10, }}, targetPart: TargetBodyPart.Head);
                _stun.KnockdownOrStun(remote, TimeSpan.FromSeconds(5), true);

                Popup.PopupClient("cyberdeck-player-get-hacked", ent.Comp.RemoteEntity.Value, ent.Comp.RemoteEntity, PopupType.LargeCaution);
                _audio.PlayLocal(sound, ent.Owner, ent.Owner);
                _audio.PlayLocal(sound, user, user);
            },
            Impact = LogImpact.High,
        });
    }

    private void OnCyberdeckAccessible(Entity<CyberdeckUserComponent> ent, ref AccessibleOverrideEvent args)
    {
        if (args.Handled)
            return;

        args.Accessible = _aiWhitelistQuery.HasComp(args.Target);
        args.Handled = true;
    }

    private void OnCyberdeckRangeAccessible(Entity<CyberdeckUserComponent> ent, ref InRangeOverrideEvent args)
    {
        if (args.Handled)
            return;

        args.InRange = _aiWhitelistQuery.HasComp(args.Target);
        args.Handled = true;
    }
}
