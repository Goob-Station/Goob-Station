using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Antags.Abductor;

[Serializable, NetSerializable]
public enum AbductorExperimentatorVisuals : byte
{
    Full
}

[Serializable, NetSerializable]
public enum AbductorCameraConsoleUIKey
{
    Key
}

[Serializable, NetSerializable]
public enum AbductorConsoleUIKey
{
    Key
}

[Serializable, NetSerializable]
public enum AbductorArmorModeType : byte
{
    Combat,
    Stealth
}
