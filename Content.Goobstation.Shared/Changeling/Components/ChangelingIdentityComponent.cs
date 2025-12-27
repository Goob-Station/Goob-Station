// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 TGRCDev <tgrc@tgrc.dev>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Marcus F <marcus2008stoke@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <marcus2008stoke@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ChangelingIdentityComponent : Component
{
    #region Prototypes

    [DataField("soundMeatPool")]
    public List<SoundSpecifier?> SoundPool = new()
    {
        new SoundPathSpecifier("/Audio/Effects/gib1.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib2.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib3.ogg"),
    };

    [DataField("soundShriek")]
    public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/changeling_shriek.ogg");

    [DataField("shriekPower")]
    public float ShriekPower = 2.5f;

    [DataField("armorTransform")]
    public SoundSpecifier ArmourSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/armour_transform.ogg");
    [DataField("armorStrip")]
    public SoundSpecifier ArmourStripSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/armour_strip.ogg");

    public readonly List<EntProtoId> BaseChangelingActions = new()
    {
        "ActionEvolutionMenu",
        "ActionAbsorbDNA",
        "ActionStingExtractDNA",
        "ActionChangelingTransformCycle",
        "ActionChangelingTransform",
        "ActionEnterStasis",
        "ActionExitStasis"
    };

    /// <summary>
    ///     The status icon corresponding to the Changlings.
    /// </summary>

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "HivemindFaction";

    #endregion

    [DataField]
    public bool IsInStasis = false;

    [DataField]
    public bool StrainedMusclesActive = false;

    [DataField]
    public bool IsInLesserForm = false;

    [DataField]
    public bool IsInLastResort = false;

    [DataField]
    public bool ChameleonActive = false;

    public List<EntityUid>? ActiveArmor = null;

    public Dictionary<string, EntityUid?> Equipment = new();

    /// <summary>
    ///     The default stasis time (in s).
    /// </summary>
    public readonly int DefaultStasisTime = 15;

    /// <summary>
    ///     The typical longest time that stasis can last (in s).
    /// </summary>
    public readonly int MaxStasisTime = 45;

    /// <summary>
    ///     The time a changeling must stay in stasis upon taking catastrophic damage (in s).
    /// </summary>
    public readonly int CatastrophicStasisTime = 60;

    /// <summary>
    ///     Time in seconds the changeling must spend in stasis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StasisTime;

    /// <summary>
    ///     Current amount of chemicals changeling currently has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Chemicals = 100.0f;

    /// <summary>
    ///     Maximum amount of chemicals changeling can have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxChemicals = 100.0f;

    /// <summary>
    ///     Total evolution points gained by the changeling.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TotalEvolutionPoints;

    /// <summary>
    ///     Bonus chemicals regeneration. In case
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BonusChemicalRegen = 0.0f;

    /// <summary>
    ///     Chemicals regeneration rate multiplier from certain abilities.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChemicalRegenMultiplier = 1.0f;

    /// <summary>
    ///     Cooldown between chem regen events.
    /// </summary>
    public TimeSpan UpdateTimer = TimeSpan.Zero;
    public float UpdateCooldown = 1f;

    /// <summary>
    ///     All of the DNA that the changeling had extracted in their lifetime.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<TransformData> AbsorbedHistory = new();

    /// <summary>
    ///     The DNA that the changeling has stored up.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<TransformData> AbsorbedDNA = new();

    /// <summary>
    ///     Index of <see cref="AbsorbedDNA"/>. Used for switching forms.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public int AbsorbedDNAIndex = 0;

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    [DataField]
    public int MaxAbsorbedDNA = 5;

    /// <summary>
    ///     Total absorbed DNA. Counts towards objectives.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int TotalAbsorbedEntities = 0;

    /// <summary>
    ///     Total absorbed changelings. Used as a 'bonus' for its respective objective.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TotalChangelingsAbsorbed = 0;

    /// <summary>
    ///     Total stolen DNA. Counts towards objectives.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int TotalStolenDNA = 0;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public TransformData? CurrentForm;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? SelectedForm;
}

[DataDefinition]
public sealed partial class TransformData
{
    /// <summary>
    ///     Entity's name.
    /// </summary>
    [DataField]
    public string Name;

    /// <summary>
    ///     Entity's fingerprint, if it exists.
    /// </summary>
    [DataField]
    public string? Fingerprint;

    /// <summary>
    ///     Entity's DNA.
    /// </summary>
    [DataField("dna")]
    public string DNA;

    /// <summary>
    ///     Entity's humanoid appearance component.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly), NonSerialized]
    public HumanoidAppearanceComponent Appearance;
}
