using Content.Shared.Damage;
using Content.Shared.Heretic;

namespace Content.Server.Heretic;

public sealed partial class HereticCombatMarkSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChange);
    }

    public void AddCombatMark(EntityUid target, EntityUid performer)
    {
        if (!TryComp<HereticComponent>(performer, out var hereticComp))
            return;

        var mark = EnsureComp<HereticCombatMarkComponent>(target);
        mark.AttachedEntity = SpawnAttachedTo($"HereticCombatMark{hereticComp.CurrentPath}", Transform(target).Coordinates);
        // failed to attach tha mark, get sad and remove it
        if (mark.AttachedEntity == null)
            RemComp(target, mark);
    }

    public bool ApplyMarkEffect(EntityUid target, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        switch (path)
        {
            case "Ash":
                break;

            case "Blade":
                break;

            case "Flesh":
                break;

            case "Lock":
                break;

            case "Rust":
                break;

            case "Void":
                break;

            default:
                return false;
        }

        return true;
    }

    public void RemoveCombatMark(EntityUid target)
    {
        if (!TryComp<HereticCombatMarkComponent>(target, out var mark))
            return;

        if (mark.AttachedEntity != null)
            QueueDel(mark.AttachedEntity);
        RemComp(target, mark);
    }

    private void OnDamageChange(Entity<DamageableComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta is null || !args.DamageIncreased)
            return;

        if (!args.Origin.HasValue)
            return;

        if (!TryComp<HereticComponent>(args.Origin.Value, out var hereticComp))
            return;

        if (!TryComp<HereticCombatMarkComponent>(ent, out var mark))
            return;


        if (ApplyMarkEffect(ent, hereticComp.CurrentPath))
        {

        }
    }
}
