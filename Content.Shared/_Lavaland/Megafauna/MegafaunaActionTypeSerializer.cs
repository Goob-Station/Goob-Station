using Content.Shared._Lavaland.Megafauna.Actions;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared._Lavaland.Megafauna;

[TypeSerializer]
public sealed class MegafaunaActionTypeSerializer :
    ITypeReader<MegafaunaActionSelector, MappingDataNode>
{
    public ValidationNode Validate(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        if (node.Has(ProtoIdMegafaunaAction.IdDataFieldTag))
            return serializationManager.ValidateNode<ProtoIdMegafaunaAction>(node, context);

        return new ErrorNode(node, "Custom validation not supported! Please specify the type manually!");
    }

    public MegafaunaActionSelector Read(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<MegafaunaActionSelector>? instanceProvider = null)
    {
        var type = typeof(MegafaunaActionSelector);
        if (node.Has(ProtoIdMegafaunaAction.IdDataFieldTag))
            type = typeof(ProtoIdMegafaunaAction);

        return (MegafaunaActionSelector) serializationManager.Read(type, node, context)!;
    }
}
