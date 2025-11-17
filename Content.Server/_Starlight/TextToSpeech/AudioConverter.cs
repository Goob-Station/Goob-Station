using System;
using System.Diagnostics;
using System.IO;

namespace Content.Server.Starlight.TextToSpeech
{
    public static class AudioConverter
    {
        public static byte[] ConvertToOgg(byte[] inputAudioBytes)
        {
            var inputPath = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(Path.GetTempFileName(), ".ogg");

            File.WriteAllBytes(inputPath, inputAudioBytes);

            try
            {
                var ffmpeg = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-y -i \"{inputPath}\" " + "-filter:a loudnorm=I=-16:TP=-1.5:LRA=11 " + $" -c:a libvorbis \"{outputPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                using var process = Process.Start(ffmpeg)!;
                process.WaitForExit();

                if (!File.Exists(outputPath))
                    throw new FileNotFoundException("FFmpeg did not produce an output file.");

                return File.ReadAllBytes(outputPath);
            }
            finally
            {
                try { File.Delete(inputPath); } catch { }
                try { File.Delete(outputPath); } catch { }
            }
        }
    }
}
