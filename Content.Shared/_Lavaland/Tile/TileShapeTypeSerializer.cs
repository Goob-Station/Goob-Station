using Content.Shared._Lavaland.Tile.Shapes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared._Lavaland.Tile;

[TypeSerializer]
public sealed class TileShapeTypeSerializer :
    ITypeReader<TileShape, MappingDataNode>
{
    public ValidationNode Validate(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        if (node.Has(ProtoIdTileShape.IdDataFieldTag))
            return serializationManager.ValidateNode<ProtoIdTileShape>(node, context);

        return new ErrorNode(node, "Custom validation not supported! Please specify the type manually!");
    }

    public TileShape Read(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<TileShape>? instanceProvider = null)
    {
        var type = typeof(TileShape);
        if (node.Has(ProtoIdTileShape.IdDataFieldTag))
            type = typeof(ProtoIdTileShape);

        return (TileShape) serializationManager.Read(type, node, context)!;
    }
}
