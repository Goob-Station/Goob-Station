using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Autodoc;

[Serializable, NetSerializable]
public enum AutodocUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class AutodocCreateProgramMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class AutodocToggleProgramSafetyMessage(int program) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
}

[Serializable, NetSerializable]
public sealed class AutodocRemoveProgramMessage(int program) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
}

[Serializable, NetSerializable]
public sealed class AutodocAddStepMessage(int program, IAutodocStep step) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
    public readonly IAutodocStep Step = step;
}

[Serializable, NetSerializable]
public sealed class AutodocRemoveStepMessage(int program, int step) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
    public readonly int Step = step;
}

[Serializable, NetSerializable]
public sealed class AutodocStartMessage(int program) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
}

[Serializable, NetSerializable]
public sealed class AutodocStopMessage : BoundUserInterfaceMessage;
