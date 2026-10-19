using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Actions.Events;

[DataDefinition]
public sealed partial class ActionPurchaseCompanionEvent : EntityEventArgs
{
    [DataField]
    public NetEntity Buyer = default!;
    
    [DataField("companionActions")]
    public List<string> CompanionActions = new();
}
