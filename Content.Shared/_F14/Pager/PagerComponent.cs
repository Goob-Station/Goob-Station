using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared._F14.SCPOS;

namespace Content.Shared._F14.Pager;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PagerComponent : Component
{
    [DataField, AutoNetworkedField]
    public string OwnerName = "Unknown Operator";

    [DataField, AutoNetworkedField]
    public SoundSpecifier? RingSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? ClickSound;

    [DataField, AutoNetworkedField]
    public EntityUid? ActiveCallTarget;

    [DataField, AutoNetworkedField]
    public PagerCallState CallState = PagerCallState.Idle;

    [DataField, AutoNetworkedField]
    public List<PagerMessage> Messages = new();

    [DataField, AutoNetworkedField]
    public List<string> Notes = new();

    [DataField, AutoNetworkedField]
    public int SupplyBudget = 1500;

    [DataField, AutoNetworkedField]
    public int DiskCapacity = 64;

    [DataField, AutoNetworkedField]
    public List<string> InstalledSoftware = new()
    {
        "scpos_relay_chat_client",
        "scpos_dispatch",
        "scpos_nanoword",
        "scpos_personnel_manifest"
    };

    public const string IdCardSlotId = "PagerIdCard";
}

[Serializable, NetSerializable]
public struct PagerMessage
{
    public string Sender;
    public string? Recipient;
    public string Text;
    public TimeSpan Timestamp;

    public PagerMessage(string sender, string? recipient, string text, TimeSpan timestamp)
    {
        Sender = sender;
        Recipient = recipient;
        Text = text;
        Timestamp = timestamp;
    }
}

[Serializable, NetSerializable]
public struct PagerCrewEntry
{
    public string Name;
    public string JobTitle;

    public PagerCrewEntry(string name, string jobTitle)
    {
        Name = name;
        JobTitle = jobTitle;
    }
}

[Serializable, NetSerializable]
public struct PagerScpEntry
{
    public string Id;
    public string Name;
    public string ObjectClass;
    public string Containment;
    public string Description;

    public PagerScpEntry(string id, string name, string objectClass, string containment, string description)
    {
        Id = id;
        Name = name;
        ObjectClass = objectClass;
        Containment = containment;
        Description = description;
    }
}

[Serializable, NetSerializable]
public struct PagerShieldEntry
{
    public string Name;
    public int Integrity;
    public int PowerKw;
    public string Status;

    public PagerShieldEntry(string name, int integrity, int powerKw, string status)
    {
        Name = name;
        Integrity = integrity;
        PowerKw = powerKw;
        Status = status;
    }
}

[Serializable, NetSerializable]
public struct PagerCrewRecord
{
    public string Name;
    public string Clearance;
    public string Status;
    public string Log;

    public PagerCrewRecord(string name, string clearance, string status, string log)
    {
        Name = name;
        Clearance = clearance;
        Status = status;
        Log = log;
    }
}

[Serializable, NetSerializable]
public struct PagerSupplyItem
{
    public string Id;
    public string Name;
    public int Cost;

    public PagerSupplyItem(string id, string name, int cost)
    {
        Id = id;
        Name = name;
        Cost = cost;
    }
}

[Serializable, NetSerializable]
public struct PagerScannerData
{
    public float HumeLevel;
    public float AmbientTemp;
    public float Radiation;
    public int HeartRate;
    public string ThreatStatus;

    public PagerScannerData(float humeLevel, float ambientTemp, float radiation, int heartRate, string threatStatus)
    {
        HumeLevel = humeLevel;
        AmbientTemp = ambientTemp;
        Radiation = radiation;
        HeartRate = heartRate;
        ThreatStatus = threatStatus;
    }
}

[Serializable, NetSerializable]
public enum PagerCallState : byte
{
    Idle,
    OutgoingRinging,
    IncomingRinging,
    InCall
}

[Serializable, NetSerializable]
public enum PagerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PagerBoundUserInterfaceState : BoundUserInterfaceState
{
    public string OwnerName;
    public PagerCallState CallState;
    public string? CallPartnerName;
    public List<PagerMessage> Messages;
    public List<(NetEntity PagerNetUid, string Name)> AvailableContacts;
    public List<PagerCrewEntry> Manifest;

    public int DiskUsed;
    public int DiskCapacity;
    public List<string> InstalledSoftware;
    public List<string> CurrentAccessTags;
    public string? InsertedIdName;

    public List<string> Notes;
    public int? BankBalance;
    public string? BankAccountNum;
    public List<PagerScpEntry> Database;
    public List<PagerShieldEntry> Shields;

    public List<PagerCrewRecord> Records;
    public List<PagerSupplyItem> SupplyCatalog;
    public int SupplyBudget;
    public PagerScannerData Scanner;
    public bool EarpieceLinked;

    public PagerBoundUserInterfaceState(
        string ownerName,
        PagerCallState callState,
        string? callPartnerName,
        List<PagerMessage> messages,
        List<(NetEntity, string)> availableContacts,
        List<PagerCrewEntry> manifest,
        int diskUsed,
        int diskCapacity,
        List<string> installedSoftware,
        List<string> currentAccessTags,
        string? insertedIdName,
        List<string> notes,
        int? bankBalance,
        string? bankAccountNum,
        List<PagerScpEntry> database,
        List<PagerShieldEntry> shields,
        List<PagerCrewRecord> records,
        List<PagerSupplyItem> supplyCatalog,
        int supplyBudget,
        PagerScannerData scanner,
        bool earpieceLinked)
    {
        OwnerName = ownerName;
        CallState = callState;
        CallPartnerName = callPartnerName;
        Messages = messages;
        AvailableContacts = availableContacts;
        Manifest = manifest;
        DiskUsed = diskUsed;
        DiskCapacity = diskCapacity;
        InstalledSoftware = installedSoftware;
        CurrentAccessTags = currentAccessTags;
        InsertedIdName = insertedIdName;
        Notes = notes;
        BankBalance = bankBalance;
        BankAccountNum = bankAccountNum;
        Database = database;
        Shields = shields;
        Records = records;
        SupplyCatalog = supplyCatalog;
        SupplyBudget = supplyBudget;
        Scanner = scanner;
        EarpieceLinked = earpieceLinked;
    }
}

[Serializable, NetSerializable]
public enum PagerButton : byte
{
    Up,
    Down,
    Left,
    Right,
    Green,
    Red,
    Space,
    WheelUp,
    WheelDown
}

[Serializable, NetSerializable]
public sealed class PagerButtonPressedMessage : BoundUserInterfaceMessage
{
    public PagerButton Button;
    public PagerButtonPressedMessage(PagerButton button) => Button = button;
}

[Serializable, NetSerializable]
public sealed class PagerSendMessageAlert : BoundUserInterfaceMessage
{
    public string? Recipient;
    public string Content;

    public PagerSendMessageAlert(string? recipient, string content)
    {
        Recipient = recipient;
        Content = content;
    }
}

[Serializable, NetSerializable]
public sealed class PagerCallRequestMessage : BoundUserInterfaceMessage
{
    public NetEntity TargetPager;
    public PagerCallRequestMessage(NetEntity targetPager) => TargetPager = targetPager;
}

[Serializable, NetSerializable]
public sealed class PagerDownloadSoftwareMessage : BoundUserInterfaceMessage
{
    public string SoftwareId;
    public PagerDownloadSoftwareMessage(string softwareId) => SoftwareId = softwareId;
}

[Serializable, NetSerializable]
public sealed class PagerUninstallSoftwareMessage : BoundUserInterfaceMessage
{
    public string SoftwareId;
    public PagerUninstallSoftwareMessage(string softwareId) => SoftwareId = softwareId;
}

[Serializable, NetSerializable]
public sealed class PagerEjectIdMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PagerAddNoteMessage : BoundUserInterfaceMessage
{
    public string Text;
    public PagerAddNoteMessage(string text) => Text = text;
}

[Serializable, NetSerializable]
public sealed class PagerDeleteNoteMessage : BoundUserInterfaceMessage
{
    public int Index;
    public PagerDeleteNoteMessage(int index) => Index = index;
}

[Serializable, NetSerializable]
public sealed class PagerCycleShieldsMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PagerOrderSupplyMessage : BoundUserInterfaceMessage
{
    public string ItemId;
    public PagerOrderSupplyMessage(string itemId) => ItemId = itemId;
}

[Serializable, NetSerializable]
public sealed class PagerTriggerScanMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PagerSetIdInfoMessage : BoundUserInterfaceMessage
{
    public string Name;
    public string JobTitle;

    public PagerSetIdInfoMessage(string name, string jobTitle)
    {
        Name = name;
        JobTitle = jobTitle;
    }
}

[RegisterComponent, NetworkedComponent]
public sealed partial class PagerEarpieceComponent : Component
{
}
