using Content.Goobstation.Shared.Cult;

namespace Content.Goobstation.Server.Cult.Systems;
public sealed partial class BloodCultistSystem : EntitySystem
{


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultistComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<BloodCultistComponent> ent, ref ComponentStartup args)
    {

    }
}
