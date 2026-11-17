using Content.Shared.Forensics.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModDnaLockModuleSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    [SubscribeLocalEvent]
    private void OnAccessAttempt(Entity<ModDnaLockModuleComponent> ent, ref ModSuitAccessAttemptEvent args)
    {
        if (ent.Comp.LockedDna is { } locked && GetDna(args.User) != locked)
            args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnUsed(Entity<ModDnaLockModuleComponent> ent, ref ModModuleUsedEvent args)
    {
        args.Handled = true;

        if (ent.Comp.LockedDna != null)
        {
            ent.Comp.LockedDna = null;
            Dirty(ent);
            _audio.PlayPredicted(ent.Comp.Sound, args.User, args.User);
            _popup.PopupClient(Loc.GetString("modsuit-dna-unlocked"), args.User, args.User);
            return;
        }

        if (GetDna(args.User) is not { } dna)
        {
            _popup.PopupClient(Loc.GetString("modsuit-dna-none"), args.User, args.User);
            return;
        }

        ent.Comp.LockedDna = dna;
        Dirty(ent);
        _audio.PlayPredicted(ent.Comp.Sound, args.User, args.User);
        _popup.PopupClient(Loc.GetString("modsuit-dna-locked"), args.User, args.User);
    }

    private string? GetDna(EntityUid user)
    {
        return TryComp<DnaComponent>(user, out var dna) ? dna.DNA : null;
    }
}
