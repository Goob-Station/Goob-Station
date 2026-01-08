using Content.Goobstation.Shared.Wraith.Curses;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Administration.Logs;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Wraith.Revenant;

/// <summary>
/// The target immediately collapses and begins to take a huge amount of brute damage over time
/// as their bones crack and their body implodes. The victim explodes into gibs once this damage becomes lethal,
/// but the process is interrupted if they are removed from your line of sight or you move (or are moved).
/// </summary>
public sealed class RevenantCrushSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevenantCrushComponent, RevenantCrushEvent>(OnRevenantCrush);
        SubscribeLocalEvent<RevenantCrushComponent, RevenantCrushDoAfterEvent>(OnRevenantCrushDoAfter);
    }

    private void OnRevenantCrush(Entity<RevenantCrushComponent> ent, ref RevenantCrushEvent args)
    {
        if (ent.Comp.InitialDamage == null)
            return;

        if (HasComp<CurseImmuneComponent>(args.Target))
        {
            _popup.PopupClient(Loc.GetString("revenant-crush-chaplain"), ent.Owner, ent.Owner);
            return;
        }

        var doAftersArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.AbilityDuration,
            new RevenantCrushDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            // technically, in order for them to be removed from our line of sight, they need to move...
            BreakOnMove = true,
            MovementThreshold = 0.3f,
            DistanceThreshold = 15f,
        };

        _popup.PopupClient(Loc.GetString("revenant-crush-start"), args.Target, args.Target, PopupType.MediumCaution);
        _doAfter.TryStartDoAfter(doAftersArgs);
        _audio.PlayPredicted(ent.Comp.CrushSound, args.Target, args.Target);

        _stunSystem.KnockdownOrStun(args.Target, ent.Comp.KnockdownDuration, true);
        _damageableSystem.TryChangeDamage(args.Target, ent.Comp.InitialDamage, true);

        args.Handled = true;
    }

    // TO DO: Make it so there's a 25% each second that it plays Flesh_Tear1.ogg and picks one of these three pop-ups revenant-crush-crack1, 2 or 3.
    //TO DO: Deal damage to their chest every second. Theoretically 5 damage, but who knows. Doesn't have to be the chest if you don't wanna deal with that.
    private void OnRevenantCrushDoAfter(Entity<RevenantCrushComponent> ent, ref RevenantCrushDoAfterEvent args)
    {
        var target = args.Target;

        if (args.Cancelled || target == null)
            return;

        _popup.PopupClient(Loc.GetString("revenant-crush-you"), target.Value, target.Value);
        _admin.Add(LogType.Gib, LogImpact.High, $"{ent.Owner} gibbed {target.Value} via Crush");
        if (_netManager.IsServer) // this shit mispredicts, requires upstream prediction fix
            _body.GibBody(target.Value, splatModifier: 5f);

        args.Handled = true;
    }
}
