namespace Content.Shared._EinsteinEngines.Silicon.Bots;

[RegisterComponent]
[Access(typeof(FillbotSystem))]
public sealed partial class FillbotComponent : Component
{
    [ViewVariables]
    public EntityUid? LinkedSinkEntity { get; set; }
}
