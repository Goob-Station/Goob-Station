using System.IO;
using System.Threading;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.ContentPack;
using Robust.Shared.Graphics;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using YamlDotNet.RepresentationModel;

namespace Content.Client._Goobstation.ResourceManagement.ResourceTypes
{
    public sealed class MetadataTextureResource : BaseResource
    {
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        private TextureResource _texture = default!;
        public TextureResource Texture => _texture;
        public override ResPath? Fallback => new("/Textures/noSprite.png");

        public override void Load(IDependencyCollection dependencies, ResPath path)
        {
            if (path.Directory.Filename.EndsWith(".rsi"))
            {
                Logger.WarningS(
                    "res",
                    "Loading raw texture inside RSI: {Path}. Refer to the RSI state instead of the raw PNG.",
                    path);
            }

            var data = new LoadStepData {Path = path};

            LoadPreTexture(dependencies.Resolve<IResourceManager>(), data);
            LoadTexture(dependencies.Resolve<IClyde>(), data);
            LoadFinish(dependencies.Resolve<IResourceCache>(), data);
        }

        internal static void LoadPreTexture(IResourceManager cache, LoadStepData data)
        {
            using (var stream = cache.ContentFileRead(data.Path))
            {
                data.Image = Image.Load<Rgba32>(stream);
            }

            data.LoadParameters = TryLoadTextureParameters(cache, data.Path) ?? TextureLoadParameters.Default;
        }

        internal static void LoadTexture(IClyde clyde, LoadStepData data)
        {
            data.Texture = _resourceCache.GetResource<TextureResource>(data.Path);
        }

        internal void LoadFinish(IResourceCache cache, LoadStepData data)
        {
            _texture = data.Texture;
        }

        public override void Reload(IDependencyCollection dependencies, ResPath path, CancellationToken ct = default)
        {
            var data = new LoadStepData {Path = path};

            data.Texture = _resourceCache.GetResource<TextureResource>(path);
        }

        internal sealed class LoadStepData
        {
            public ResPath Path = default!;
            public string MetaData = default!;
            public TextureResource Texture = default!;
            public bool Bad;
        }

        // TODO: Due to a bug in Roslyn, NotNullIfNotNullAttribute doesn't work.
        // So this can't work with both nullables and non-nullables at the same time.
        // I decided to only have it work with non-nullables as such.
        public static implicit operator TextureResource(MetadataTextureResource res)
        {
            return res.Texture;
        }
    }
}
