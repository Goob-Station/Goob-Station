namespace Content.Goobstation.Shared.Sandevistan;

// Well this is the last thing I actually need to do
public sealed class SandevistanSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanUserComponent, ToggleSandevistanEvent>(OnToggle);
    }

    private void OnToggle(Entity<SandevistanUserComponent> ent, ref ToggleSandevistanEvent args)
    {
        if (ent.Comp.Enabled)
        {

        }
    }

}
