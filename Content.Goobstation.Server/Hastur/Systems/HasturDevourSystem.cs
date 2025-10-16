using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Server.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Hastur.Systems;

public sealed class HasturDevourSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HasturDevourComponent, HasturDevourEvent>(OnTryDevour);

        SubscribeLocalEvent<HasturDevourComponent, HasturDevourDoAfterEvent>(OnDevourDoAfter);

    }

    private void OnTryDevour(Entity<HasturDevourComponent> ent, ref HasturDevourEvent args)
    {
        if (_mobState.IsAlive(args.Target))
        {
            _popup.PopupPredicted(Loc.GetString("hastur-devour-fail"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        // Stun the target first
        _stun.TryStun(args.Target, ent.Comp.StunDuration, false);

        _popup.PopupPredicted(Loc.GetString("hastur-devour", ("user", ent.Owner), ("target", args.Target)),ent.Owner, args.Target, PopupType.LargeCaution);

        _audio.PlayPredicted(ent.Comp.DevourSound, ent.Owner, args.Target);

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
            return;

        _bodySystem.GibBody(target); // Actually devour the target

        args.Handled = true;
    }

}
