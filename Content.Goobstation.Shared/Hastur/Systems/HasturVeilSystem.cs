using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;

namespace Content.Goobstation.Shared.Hastur.Systems;
public sealed class HasturVeilSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HasturVeilComponent, VeilOfTheVoidEvent>(OnVeil);
        SubscribeLocalEvent<HasturVeilComponent, HasturDevourEvent>(OnDevour);
    }

    private void OnVeil(Entity<HasturVeilComponent> ent, ref VeilOfTheVoidEvent args)
    {
        if (args.Handled)
            return;

        if (!ent.Comp.IsActive)
        {
            var stealth = EnsureComp<StealthComponent>(ent.Owner);
            _stealth.SetVisibility(ent.Owner, 0f, stealth);

            _popup.PopupPredicted(
                Loc.GetString("hastur-hide1", ("user", ent.Owner)),
                ent.Owner, ent.Owner, PopupType.Medium);
            ent.Comp.IsActive = true;
            Dirty(ent);
        }
        else
        {
            RemComp<StealthComponent>(ent.Owner);

            _popup.PopupPredicted(
                Loc.GetString("hastur-reveal1", ("user", ent.Owner)),
                ent.Owner, ent.Owner, PopupType.Medium);
            ent.Comp.IsActive = false;
            Dirty(ent);

            args.Handled = true; // Only set the event as handled if he deactivates the stealth. Then the cooldown kicks in.
        }
    }

    private void OnDevour(Entity<HasturVeilComponent> ent, ref HasturDevourEvent args) // Stealth gets broken on devour as well.
    {
        RemComp<StealthComponent>(ent.Owner);

        _popup.PopupPredicted(
            Loc.GetString("hastur-reveal1", ("user", ent.Owner)),
            ent.Owner, ent.Owner, PopupType.Medium);
        ent.Comp.IsActive = false;
        Dirty(ent);
    }
}
