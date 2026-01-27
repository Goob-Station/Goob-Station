using Content.Goobstation.Common.Charges;
using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Shared.Body.Organ;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Cyberdeck;

public abstract partial class SharedCyberdeckSystem
{
    private void InitializeCharges()
    {
        SubscribeLocalEvent<CyberdeckSourceComponent, ChargesChangedEvent>(OnChargesChanged);
    }

    private bool TryHackDevice(EntityUid user, EntityUid device)
    {
        if (!_hackableQuery.TryComp(device, out var hackable)
            || !_power.IsPowered(device))
            return false;

        return UseCharges(user, hackable.Cost);
    }

    /// <summary>
    /// Checks and then uses some cyberdeck charges. If cyberdeck provider entity is null,
    /// will just ignore charges and always return true.
    /// </summary>
    private bool UseCharges(EntityUid user, int amount, EntityUid? target = null)
    {
        if (!_cyberdeckUserQuery.TryComp(user, out var cyberDeck))
            return false;

        if (cyberDeck.ProviderEntity == null)
            return true; // We don't care if nowhere to take charges from at this point

        if (!CheckCharges(user, cyberDeck.ProviderEntity.Value, amount, target))
            return false;

        Charges.TryUseCharges(cyberDeck.ProviderEntity.Value, amount);
        return true;
    }

    /// <summary>
    /// Checks if the user has enough charges to use ability or hack something
    /// using cyberdeck charges, also handles all related popups.
    /// </summary>
    private bool CheckCharges(EntityUid user, EntityUid provider, int amount, EntityUid? target = null)
    {
        // If we don't have a provider, we also return true so it will give infinite charges (feature)
        if (!_chargesQuery.TryComp(provider, out var chargesComp)
            || Charges.HasCharges((provider, chargesComp), amount))
            return true;

        // Tell user that he doesn't have enough charges
        string message;
        var charges = chargesComp.LastCharges;
        var chargesForm = amount - charges;

        // SHUT UP C# I HATE BRACES!!!!!!!!!
        // ReSharper disable once EnforceIfStatementBraces
        if (target != null)
            message = Loc.GetString("cyberdeck-insufficient-charges-with-target",
                ("amount", chargesForm),
                ("target", Identity.Entity(target.Value, EntityManager, user)));
        // ReSharper disable once EnforceIfStatementBraces
        else
            message = Loc.GetString("cyberdeck-insufficient-charges",
                ("amount", chargesForm));

        Popup.PopupClient(message, user, user, PopupType.Medium);
        return false;
    }

    private void OnChargesChanged(Entity<CyberdeckSourceComponent> ent, ref ChargesChangedEvent args)
    {
        if (!TryComp(ent.Owner, out OrganComponent? organ)
            || !_cyberdeckUserQuery.TryComp(organ.Body, out var userComp))
            return;

        var user = organ.Body.Value;
        ent.Comp.Accumulator = 0f;
        UpdateAlert((user, userComp));
    }
}
