using System.Linq;
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Goobstation.Shared.Cyberware;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Goobstation.Shared.RecoilAbsorber;

public sealed class RecoilAbsorberSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly CyberneticsSystem _cybernetics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RecoilAbsorberArmComponent, ComponentInit>(OnAbsorberChanged);
        SubscribeLocalEvent<RecoilAbsorberArmComponent, BodyPartAddedEvent>(OnAbsorberChanged);
        SubscribeLocalEvent<RecoilAbsorberArmComponent, BodyPartRemovedEvent>(OnAbsorberChanged);
        SubscribeLocalEvent<RecoilAbsorberArmComponent, CyberwareChangedEvent>(OnAbsorberChanged);

        SubscribeLocalEvent<RecoilAbsorberComponent, GetRecoilModifiersEvent>(OnShot);
    }

    private void OnAbsorberChanged<T>(Entity<RecoilAbsorberArmComponent> ent, ref T args) => UpdateComp(ent);

    private void UpdateComp(Entity<RecoilAbsorberArmComponent> ent)
    {
        if (!TryComp<BodyPartComponent>(ent, out var part)
            || part.Body == null)
            return;

        var arms = _body.GetBodyChildrenOfType(part.Body.Value, BodyPartType.Arm).ToList();
        if (arms.Count == 0)
        {
            RemComp<RecoilAbsorberComponent>(part.Body.Value);
            return;
        }

        // Check if all arms are absorber arms and collect their modifiers
        var modifiers = new List<float>();
        foreach (var arm in arms)
        {
            if (!TryComp<RecoilAbsorberArmComponent>(arm.Id, out var absorber)
                || !_cybernetics.IsEnabled(arm.Id))
            {
                RemCompDeferred<RecoilAbsorberComponent>(part.Body.Value);
                return;
            }

            modifiers.Add(absorber.Modifier);
        }

        // Only if we have valid modifiers from all arms, add/update the component
        if (modifiers.Count > 0)
        {
            var comp = EnsureComp<RecoilAbsorberComponent>(part.Body.Value);
            comp.Modifier = modifiers.Min();
            Dirty(part.Body.Value, comp);
        }
    }

    private void OnShot(Entity<RecoilAbsorberComponent> ent, ref GetRecoilModifiersEvent args)
    {
        if (args.User != ent.Owner)
            return;

        args.Modifier *= ent.Comp.Modifier;
    }
}
