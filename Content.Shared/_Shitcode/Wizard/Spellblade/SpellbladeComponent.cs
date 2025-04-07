// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.Spellblade;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpellbladeComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? EnchantmentName;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EnchantSound = new SoundPathSpecifier("/Audio/Magic/forcewall.ogg");

    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<SpellbladeEnchantmentPrototype>> Prototypes = new();
}

[DataDefinition]
public sealed partial class LightningSpellbladeEnchantmentEvent : EntityEventArgs
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(3);

    [DataField]
    public float ShockDamage = 30f;

    [DataField]
    public float ShockTime = 0.5f;

    [DataField]
    public float Range = 4f;

    [DataField]
    public int BoltCount = 3;

    [DataField]
    public int ArcDepth = 1;

    [DataField]
    public float Siemens = 1f;

    [DataField]
    public EntProtoId LightningPrototype = "HyperchargedLightning";
}

[DataDefinition]
public sealed partial class BluespaceSpellbladeEnchantmentEvent : EntityEventArgs
{
    [DataField]
    public float Distance = 10f;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(1);

    [DataField]
    public float KnockdownRadius = 0.5f;

    [DataField]
    public TimeSpan BlinkDelay = TimeSpan.FromSeconds(2.5);

    [DataField]
    public TimeSpan ToggleDelay = TimeSpan.FromSeconds(0.2);
}

[DataDefinition]
public sealed partial class FireSpellbladeEnchantmentEvent : EntityEventArgs
{
    [DataField]
    public float Range = 4f;

    [DataField]
    public float FireStacks = 10f;

    [DataField]
    public float FireStacksOnHit = 2f;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(8);
}

[DataDefinition]
public sealed partial class SpacetimeSpellbladeEnchantmentEvent : EntityEventArgs
{
    [DataField]
    public float MeleeMultiplier = 2f;
}

[DataDefinition]
public sealed partial class ForceshieldSpellbladeEnchantmentEvent : EntityEventArgs
{
    [DataField]
    public float ShieldLifetime = 5f;
}

[Serializable, NetSerializable]
public sealed class SpellbladeEnchantMessage(ProtoId<SpellbladeEnchantmentPrototype> protoId)
    : BoundUserInterfaceMessage
{
    public ProtoId<SpellbladeEnchantmentPrototype> ProtoId { get; } = protoId;
}

[Serializable, NetSerializable]
public enum SpellbladeUiKey : byte
{
    Key
}