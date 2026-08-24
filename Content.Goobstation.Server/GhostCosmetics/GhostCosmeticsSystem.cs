using Content.Goobstation.Shared.GhostCosmetics;
using Content.Server._RMC14.LinkAccount;
using Content.Shared._RMC14.LinkAccount;
using Content.Shared.Ghost;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.GhostCosmetics;

public sealed class GhostCosmeticsSystem : EntitySystem
{
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<ChangeGhostCosmeticsEvent>(OnChangeCosmetics);
        SubscribeLocalEvent<GhostComponent, PlayerAttachedEvent>(OnGhostPlayerAttached);

        _linkAccount.PatronUpdated += OnPatronUpdated;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _linkAccount.PatronUpdated -= OnPatronUpdated;
    }

    private void OnChangeCosmetics(ChangeGhostCosmeticsEvent ev, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;
        if (_linkAccount.GetPatron(session.UserId)?.Tier is not { } tier ||
            !tier.GhostCosmetics && !tier.GhostParticles)
        {
            return;
        }

        var particles = tier.GhostParticles ? Validate(ev.Particles?.Id, GhostCosmeticCategory.Particles) : null;
        var hat = tier.GhostCosmetics ? Validate(ev.Hat?.Id, GhostCosmeticCategory.Hat) : null;
        var mask = tier.GhostCosmetics ? Validate(ev.Mask?.Id, GhostCosmeticCategory.Mask) : null;

        _linkAccount.SetGhostCosmetics(session.UserId, particles?.ID, hat?.ID, mask?.ID);
    }

    private void OnGhostPlayerAttached(Entity<GhostComponent> ent, ref PlayerAttachedEvent args)
    {
        Apply(ent, _linkAccount.GetPatron(args.Player.UserId));
    }

    private void OnPatronUpdated((NetUserId Id, SharedRMCPatronFull Patron) tuple)
    {
        if (_player.TryGetSessionById(tuple.Id, out var session) &&
            session.AttachedEntity is { } uid &&
            HasComp<GhostComponent>(uid))
        {
            Apply(uid, tuple.Patron);
        }
    }

    private GhostCosmeticPrototype? Validate(string? id, GhostCosmeticCategory category)
    {
        if (string.IsNullOrEmpty(id) ||
            !_prototypes.TryIndex<GhostCosmeticPrototype>(id, out var proto) ||
            proto.Category != category)
        {
            return null;
        }

        return proto;
    }

    private void Apply(EntityUid ghost, SharedRMCPatronFull? patron)
    {
        var tier = patron?.Tier;
        var saved = patron?.GhostCosmetics;

        var particles = tier is { GhostParticles: true } ? Validate(saved?.Particles, GhostCosmeticCategory.Particles) : null;
        var hat = tier is { GhostCosmetics: true } ? Validate(saved?.Hat, GhostCosmeticCategory.Hat) : null;
        var mask = tier is { GhostCosmetics: true } ? Validate(saved?.Mask, GhostCosmeticCategory.Mask) : null;

        if (particles == null && hat == null && mask == null)
        {
            RemCompDeferred<GhostCosmeticsComponent>(ghost);
            return;
        }

        var comp = EnsureComp<GhostCosmeticsComponent>(ghost);
        comp.Particles = ToProtoId(particles);
        comp.Hat = ToProtoId(hat);
        comp.Mask = ToProtoId(mask);
        Dirty(ghost, comp);
    }

    private static ProtoId<GhostCosmeticPrototype>? ToProtoId(GhostCosmeticPrototype? proto)
    {
        if (proto == null)
            return null;

        return new ProtoId<GhostCosmeticPrototype>(proto.ID);
    }
}
