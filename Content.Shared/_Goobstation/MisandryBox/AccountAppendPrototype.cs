using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MisandryBox;

[Prototype("AccountAppend")]
public sealed class AccountAppendPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = null!;

    // TODO: Harmony monkeypatch to serialize GUID's in prototypes
    // I will probably make a separate server module for this.
    [DataField("userid")]
    public string Userid { get; private init; } = "";

    // I am not dragging the whole compReg for this
    [DataField("AppendComps")]
    public List<string> Components { get; init; } = [];
}
