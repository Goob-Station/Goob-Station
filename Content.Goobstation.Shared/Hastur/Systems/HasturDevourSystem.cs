using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Administration.Logs;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Hastur.Systems;

public sealed class HasturDevourSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HasturDevourComponent, HasturDevourEvent>(OnTryDevour);

        SubscribeLocalEvent<HasturDevourComponent, HasturDevourDoAfterEvent>(OnDevourDoAfter);

    }

    private void OnTryDevour(Entity<HasturDevourComponent> ent, ref HasturDevourEvent args)
    {
        // Stun the target first
        _stun.TryStun(args.Target, ent.Comp.StunDuration, false);

        _popup.PopupPredicted(Loc.GetString("hastur-devour", ("user", ent.Owner), ("target", args.Target)),ent.Owner, args.Target, PopupType.LargeCaution);

        _audio.PlayPredicted(ent.Comp.DevourSound, ent.Owner, ent.Owner);

        _appearance.SetData(ent.Owner, DevourVisuals.Devouring, true);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.DevourDuration,
            new HasturDevourDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);

        args.Handled = true;
    }

    private void OnDevourDoAfter(Entity<HasturDevourComponent> ent, ref HasturDevourDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
        {
            _appearance.SetData(ent.Owner, DevourVisuals.Devouring, false); // If cancelled, revert sprite.
            _stun.TryStun(ent.Owner, ent.Comp.StunDuration, false); // If it gets cancelled, Hastur gets stunned instead.
            return;
        }

        _bodySystem.GibBody(target); // Actually devour the target
        _admin.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent.Owner)} devoured {ToPrettyString(target)} as a Hastur, gibbing them in the process.");

        _damage.TryChangeDamage(ent.Owner, ent.Comp.Healing, targetPart: TargetBodyPart.All); // Shitmed Change
        _appearance.SetData(ent.Owner, DevourVisuals.Devouring, false); // Reverts the sprite on completion.
        args.Handled = true;
    }
}
