namespace Content.Shared._Goobstation.RecoveryPassword;
public enum RecoveryPasswordSetResult : byte
{
    Success,
    Disabled,
    AlreadySet,
    TooShort,
    TooLong,
    Error,
}
