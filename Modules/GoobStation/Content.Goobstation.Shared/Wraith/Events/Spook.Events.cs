using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Wraith.Events;

public sealed partial class FlipLightsEvent : InstantActionEvent;

public sealed partial class BurnLightsEvent : InstantActionEvent;
public sealed partial class OpenDoorsSpookEvent : InstantActionEvent;
public sealed partial class CreateSmokeSpookEvent : InstantActionEvent;
public sealed partial class CreateEctoplasmEvent : InstantActionEvent;
public sealed partial class SapApcEvent : InstantActionEvent;
public sealed partial class RandomSpookEvent : InstantActionEvent;
