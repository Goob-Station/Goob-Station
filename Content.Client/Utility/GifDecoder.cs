// SPDX-FileCopyrightText: 2026 Goob-Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client.Utility;

/// <summary>
///     A small, fully self-contained GIF87a/89a decoder.
/// </summary>
/// <remarks>
///     RobustToolbox's content sandbox does not expose ImageSharp's GIF frame/metadata APIs
///     (<c>Image.Load</c> only ever returns the first frame). This decoder is written purely on top
///     of whitelisted primitives: raw byte parsing plus <see cref="Image{TPixel}"/> construction,
///     <c>Clone()</c> and pixel <i>writes</i> (the <c>this[x, y]</c> setter). It never reads individual
///     pixels, since pixel <i>reads</i> are not whitelisted.
///
///     Each returned frame is a full composite of the logical screen, so callers can treat them as a
///     flat list of equally sized images. GPU upload is intentionally left to the caller so this class
///     stays thread-agnostic and testable.
/// </remarks>
public static class GifDecoder
{
    // Safety caps. A malformed or hostile gif could otherwise exhaust GPU/host memory.
    private const int MaxDimension = 2048;
    private const int MaxFrames = 256;

    // Browser-compatible delay clamping. A 0/1cs delay is treated as 100ms, everything else floored
    // at 20ms so the per-frame animation timer can never spin on a zero delay.
    private const float MinFastDelaySeconds = 0.1f;
    private const float MinDelaySeconds = 0.02f;

    public sealed class GifData
    {
        public required Image<Rgba32>[] Frames;
        public required float[] Delays;
    }

    /// <summary>
    ///     Decodes every frame of an animated (or static) gif into full-canvas RGBA images plus their
    ///     per-frame display durations in seconds. Throws on malformed data or when a safety cap is hit.
    /// </summary>
    public static GifData Decode(Stream stream)
    {
        byte[] data;
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            data = ms.ToArray();
        }

        var r = new Reader(data);

        // --- Header ---
        // "GIF87a" or "GIF89a"
        if (r.Length < 13 || r.ReadByte() != 'G' || r.ReadByte() != 'I' || r.ReadByte() != 'F')
            throw new InvalidDataException("Not a GIF file.");
        r.Skip(3); // version "87a"/"89a"

        // --- Logical Screen Descriptor ---
        var logicalWidth = r.ReadUInt16();
        var logicalHeight = r.ReadUInt16();
        if (logicalWidth <= 0 || logicalHeight <= 0 || logicalWidth > MaxDimension || logicalHeight > MaxDimension)
            throw new InvalidDataException($"GIF dimensions {logicalWidth}x{logicalHeight} out of range.");

        var packed = r.ReadByte();
        r.ReadByte(); // background color index (unused; we treat background as transparent)
        r.ReadByte(); // pixel aspect ratio (ignored)

        var hasGlobalTable = (packed & 0x80) != 0;
        var globalTableSize = 2 << (packed & 0x07);
        Rgba32[]? globalTable = hasGlobalTable ? ReadColorTable(r, globalTableSize) : null;

        var frames = new List<Image<Rgba32>>();
        var delays = new List<float>();

        // Persistent canvas the frames are composited onto, plus disposal bookkeeping.
        var canvas = new Image<Rgba32>(Configuration.Default, logicalWidth, logicalHeight, new Rgba32(0, 0, 0, 0));
        Image<Rgba32>? previousCanvas = null;

        // Graphic Control Extension state for the next image.
        var delayCs = 0;
        var transparentIndex = -1;
        var disposalMethod = 0;

        try
        {
            var done = false;
            while (!done)
            {
                var block = r.ReadByte();
                switch (block)
                {
                    case 0x3B: // Trailer
                        done = true;
                        break;

                    case 0x21: // Extension
                    {
                        var label = r.ReadByte();
                        if (label == 0xF9) // Graphic Control Extension
                        {
                            r.ReadByte(); // block size (4)
                            var gcePacked = r.ReadByte();
                            delayCs = r.ReadUInt16();
                            var tIndex = r.ReadByte();
                            transparentIndex = (gcePacked & 0x01) != 0 ? tIndex : -1;
                            disposalMethod = (gcePacked >> 2) & 0x07;
                            r.SkipSubBlocks();
                        }
                        else
                        {
                            // Application / Comment / Plain Text — irrelevant to rendering.
                            r.SkipSubBlocks();
                        }

                        break;
                    }

                    case 0x2C: // Image Descriptor
                    {
                        if (frames.Count >= MaxFrames)
                            throw new InvalidDataException($"GIF has more than {MaxFrames} frames.");

                        var left = r.ReadUInt16();
                        var top = r.ReadUInt16();
                        var width = r.ReadUInt16();
                        var height = r.ReadUInt16();
                        var imgPacked = r.ReadByte();

                        var hasLocalTable = (imgPacked & 0x80) != 0;
                        var interlaced = (imgPacked & 0x40) != 0;
                        var localTableSize = 2 << (imgPacked & 0x07);
                        var colorTable = hasLocalTable ? ReadColorTable(r, localTableSize) : globalTable;
                        if (colorTable == null)
                            throw new InvalidDataException("GIF frame has no color table.");

                        // Save canvas for disposal method 3 (restore to previous) before drawing.
                        if (disposalMethod == 3)
                            previousCanvas = canvas.Clone();

                        var indices = DecodeLzw(r, width * height);
                        Composite(canvas, indices, colorTable, left, top, width, height, interlaced, transparentIndex);

                        // Emit a snapshot of the fully composited canvas.
                        frames.Add(canvas.Clone());
                        delays.Add(ClampDelay(delayCs));

                        // Apply this frame's disposal in preparation for the next frame.
                        switch (disposalMethod)
                        {
                            case 2: // Restore to background (transparent).
                                ClearRect(canvas, left, top, width, height);
                                break;
                            case 3: // Restore to previous.
                                if (previousCanvas != null)
                                {
                                    canvas.Dispose();
                                    canvas = previousCanvas;
                                    previousCanvas = null;
                                }

                                break;
                            // 0/1: leave the canvas as drawn.
                        }

                        // Reset per-image GCE state.
                        delayCs = 0;
                        transparentIndex = -1;
                        disposalMethod = 0;
                        break;
                    }

                    case 0x00: // Stray block terminator; skip.
                        break;

                    default:
                        throw new InvalidDataException($"Unknown GIF block 0x{block:X2}.");
                }
            }
        }
        catch
        {
            foreach (var f in frames)
                f.Dispose();
            throw;
        }
        finally
        {
            canvas.Dispose();
            previousCanvas?.Dispose();
        }

        if (frames.Count == 0)
            throw new InvalidDataException("GIF contained no frames.");

        return new GifData { Frames = frames.ToArray(), Delays = delays.ToArray() };
    }

    private static float ClampDelay(int delayCs)
    {
        if (delayCs <= 1)
            return MinFastDelaySeconds;

        return Math.Max(delayCs / 100f, MinDelaySeconds);
    }

    private static Rgba32[] ReadColorTable(Reader r, int size)
    {
        var table = new Rgba32[size];
        for (var i = 0; i < size; i++)
        {
            var cr = r.ReadByte();
            var cg = r.ReadByte();
            var cb = r.ReadByte();
            table[i] = new Rgba32((byte) cr, (byte) cg, (byte) cb, 255);
        }

        return table;
    }

    /// <summary>
    ///     Writes a decoded sub-image's pixels onto the canvas, honoring interlacing, transparency and
    ///     frame offset. Only writes pixels (never reads), and clips to the canvas bounds.
    /// </summary>
    private static void Composite(
        Image<Rgba32> canvas,
        byte[] indices,
        Rgba32[] colorTable,
        int left, int top, int width, int height,
        bool interlaced, int transparentIndex)
    {
        var logicalWidth = canvas.Width;
        var logicalHeight = canvas.Height;

        for (var row = 0; row < height; row++)
        {
            var destRow = interlaced ? InterlacedRow(row, height) : row;
            var canvasY = top + destRow;
            if (canvasY < 0 || canvasY >= logicalHeight)
                continue;

            var srcBase = row * width;
            for (var x = 0; x < width; x++)
            {
                var canvasX = left + x;
                if (canvasX < 0 || canvasX >= logicalWidth)
                    continue;

                int idx = indices[srcBase + x];
                if (idx == transparentIndex || idx >= colorTable.Length)
                    continue;

                canvas[canvasX, canvasY] = colorTable[idx];
            }
        }
    }

    /// <summary>Maps the k-th stored row of an interlaced image to its real destination row.</summary>
    private static int InterlacedRow(int k, int height)
    {
        // Pass 1: rows 0,8,16...  Pass 2: 4,12...  Pass 3: 2,6,10...  Pass 4: 1,3,5...
        var pass1 = (height + 7) / 8;
        var pass2 = (height + 3) / 8;
        var pass3 = (height + 1) / 4;

        if (k < pass1)
            return k * 8;
        k -= pass1;
        if (k < pass2)
            return k * 8 + 4;
        k -= pass2;
        if (k < pass3)
            return k * 4 + 2;
        k -= pass3;
        return k * 2 + 1;
    }

    private static void ClearRect(Image<Rgba32> canvas, int left, int top, int width, int height)
    {
        var logicalWidth = canvas.Width;
        var logicalHeight = canvas.Height;
        var transparent = new Rgba32(0, 0, 0, 0);

        for (var y = top; y < top + height && y < logicalHeight; y++)
        {
            if (y < 0)
                continue;

            for (var x = left; x < left + width && x < logicalWidth; x++)
            {
                if (x < 0)
                    continue;

                canvas[x, y] = transparent;
            }
        }
    }

    /// <summary>Standard variable-width GIF LZW decompression into an index array of <paramref name="pixelCount"/> bytes.</summary>
    private static byte[] DecodeLzw(Reader r, int pixelCount)
    {
        var minCodeSize = r.ReadByte();
        if (minCodeSize < 1 || minCodeSize > 8)
            throw new InvalidDataException($"Invalid LZW minimum code size {minCodeSize}.");

        // Gather the image data sub-blocks into one contiguous buffer.
        var lzw = r.ReadSubBlocks();

        var output = new byte[pixelCount];
        var outPos = 0;

        var clearCode = 1 << minCodeSize;
        var endCode = clearCode + 1;

        var prefix = new int[4096];
        var suffix = new byte[4096];
        var stack = new byte[4096];

        for (var i = 0; i < clearCode; i++)
        {
            prefix[i] = -1;
            suffix[i] = (byte) i;
        }

        var codeSize = minCodeSize + 1;
        var dictSize = clearCode + 2;
        var oldCode = -1;
        var firstByte = 0;

        var bitPos = 0;
        var totalBits = lzw.Length * 8;

        while (outPos < pixelCount)
        {
            if (bitPos + codeSize > totalBits)
                break;

            // Read one LSB-first variable-width code.
            var code = 0;
            for (var i = 0; i < codeSize; i++)
            {
                var bit = (lzw[(bitPos + i) >> 3] >> ((bitPos + i) & 7)) & 1;
                code |= bit << i;
            }

            bitPos += codeSize;

            if (code == endCode)
                break;

            if (code == clearCode)
            {
                codeSize = minCodeSize + 1;
                dictSize = clearCode + 2;
                oldCode = -1;
                continue;
            }

            if (oldCode == -1)
            {
                // First code after a clear is always a root.
                firstByte = code;
                oldCode = code;
                if (outPos < pixelCount)
                    output[outPos++] = (byte) code;
                continue;
            }

            var inCode = code;
            var sp = 0;

            if (code >= dictSize)
            {
                // Special "KwKwK" case.
                stack[sp++] = (byte) firstByte;
                code = oldCode;
            }

            while (code >= clearCode)
            {
                stack[sp++] = suffix[code];
                code = prefix[code];
            }

            firstByte = suffix[code];
            stack[sp++] = (byte) firstByte;

            if (dictSize < 4096)
            {
                prefix[dictSize] = oldCode;
                suffix[dictSize] = (byte) firstByte;
                dictSize++;
                if (dictSize == (1 << codeSize) && codeSize < 12)
                    codeSize++;
            }

            oldCode = inCode;

            while (sp > 0 && outPos < pixelCount)
                output[outPos++] = stack[--sp];
        }

        return output;
    }

    /// <summary>Tiny forward byte cursor over the gif buffer with the helpers the parser needs.</summary>
    private sealed class Reader
    {
        private readonly byte[] _data;
        private int _pos;

        public Reader(byte[] data)
        {
            _data = data;
        }

        public int Length => _data.Length;

        public int ReadByte()
        {
            if (_pos >= _data.Length)
                throw new InvalidDataException("Unexpected end of GIF data.");

            return _data[_pos++];
        }

        public int ReadUInt16()
        {
            var lo = ReadByte();
            var hi = ReadByte();
            return lo | (hi << 8);
        }

        public void Skip(int count)
        {
            _pos += count;
            if (_pos > _data.Length)
                throw new InvalidDataException("Unexpected end of GIF data.");
        }

        /// <summary>Skips a chain of length-prefixed sub-blocks terminated by a zero-length block.</summary>
        public void SkipSubBlocks()
        {
            int size;
            while ((size = ReadByte()) != 0)
                Skip(size);
        }

        /// <summary>Reads a chain of length-prefixed sub-blocks into a single contiguous buffer.</summary>
        public byte[] ReadSubBlocks()
        {
            using var ms = new MemoryStream();
            int size;
            while ((size = ReadByte()) != 0)
            {
                if (_pos + size > _data.Length)
                    throw new InvalidDataException("Unexpected end of GIF sub-block.");

                ms.Write(_data, _pos, size);
                _pos += size;
            }

            return ms.ToArray();
        }
    }
}
