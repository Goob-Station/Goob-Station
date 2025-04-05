namespace Content.Goobstation.Server.Contract.Revival;

[RegisterComponent]
public sealed partial class RevivalContractComponent : Component
{
    /// <summary>
    /// The entity who signed the paper, AKA, the entity who has the effects applied.
    /// </summary>
    [DataField]
    public EntityUid Signer;

    /// <summary>
    /// The entity who created the contract, AKA, the entity who gains the soul.
    /// </summary>
    [DataField]
    public EntityUid ContractOwner;

}
