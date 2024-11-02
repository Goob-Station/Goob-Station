using Robust.Shared.GameStates;

namespace Content.Shared.Stunnable;

[RegisterComponent, NetworkedComponent]
public sealed partial class OvertimeStaminaDamageComponent : Component
{
    [DataField] public float Delay = 1f;
    [ViewVariables(VVAccess.ReadWrite)] public float Timer = 1f;

    /// <summary>
    ///     Total amount of stamina damage a person is about to get
    /// </summary>
    [DataField] public float Amount = 10f;

    [ViewVariables(VVAccess.ReadWrite)] public float Damage = 10f;


    /// <summary>
    ///     Divisor. How much damage should we add overtime.
    /// </summary>
    /// <remarks> For example, if the divisor is 5, out entity will get the entire overtime stam damage only after 5 seconds. </remarks>
    [DataField] public float Delta = 5f;
}
