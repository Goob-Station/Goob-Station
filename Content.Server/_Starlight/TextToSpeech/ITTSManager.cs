using System.Threading.Tasks;

namespace Content.Server.Starlight.TextToSpeech;
public interface ITTSManager
{
    Task<byte[]?> ConvertTextToSpeechAnnounce(string elevenId, string text);
    Task<byte[]?> ConvertTextToSpeechRadio(string elevenId, string text);
    Task<byte[]?> ConvertTextToSpeechStandard(string elevenId, string text);
    void Initialize();
}
