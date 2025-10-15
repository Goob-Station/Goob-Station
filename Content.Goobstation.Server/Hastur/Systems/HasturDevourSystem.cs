using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Server.Body.Systems;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Hastur.Systems;

public sealed class HasturDevourSystem : EntitySystem
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HasturDevourComponent, HasturDevourEvent>(OnTryDevour);
    }

    private void OnTryDevour(Entity<HasturDevourComponent> ent, ref HasturDevourEvent args)
    {
        _popup.PopupPredicted(
            Loc.GetString("hastur-devour", ("user", ent.Owner), ("target", args.Target)),
            ent.Owner, args.Target, PopupType.LargeCaution);
        ent.Owner, args.Target, PopupType.MediumCaution);
        _audio.PlayPredicted(ent.Comp.DevourSound, ent.Owner, args.Target);
        _stun.TryStun(args.Target, ent.Comp.StunDuration, false);

        _bodySystem.GibBody(args.Target);

        args.Handled = true;
    }
}
