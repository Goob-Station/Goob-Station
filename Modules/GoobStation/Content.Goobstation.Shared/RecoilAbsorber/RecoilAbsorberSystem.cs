using System.Linq;
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Goobstation.Shared.RecoilAbsorber;

public sealed class RecoilAbsorberSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RecoilAbsorberArmComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RecoilAbsorberArmComponent, BodyPartAddedEvent>(OnAttach);
        SubscribeLocalEvent<RecoilAbsorberArmComponent, BodyPartRemovedEvent>(OnRemove);

        SubscribeLocalEvent<RecoilAbsorberComponent, GetRecoilModifiersEvent>(OnShot);
    }

    private void OnInit(Entity<RecoilAbsorberArmComponent> ent, ref ComponentInit args) => UpdateComp(ent);

    private void OnAttach(Entity<RecoilAbsorberArmComponent> ent, ref BodyPartAddedEvent args) => UpdateComp(ent);

    private void OnRemove(Entity<RecoilAbsorberArmComponent> ent, ref BodyPartRemovedEvent args) => UpdateComp(ent);

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
            if (!TryComp<RecoilAbsorberArmComponent>(arm.Id, out var absorber))
                // If any arm is not an absorber arm, just return without the component
                return;

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
