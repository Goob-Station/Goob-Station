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

        var arms = _body.GetBodyChildrenOfType(part.Body.Value, BodyPartType.Arm);
        var absorberArms = arms.Where(x => HasComp<RecoilAbsorberArmComponent>(x.Id));
        if (arms.Count() != absorberArms.Count())
        {
            RemComp<RecoilAbsorberComponent>(part.Body.Value);
            return;
        }
        else
        {
            var comp = EnsureComp<RecoilAbsorberComponent>(part.Body.Value);
            comp.Modifier = absorberArms.Select(x => Comp<RecoilAbsorberArmComponent>(x.Id).Modifier).Min();
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
