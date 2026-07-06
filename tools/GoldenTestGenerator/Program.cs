// Golden test vector generator for .ithmb decoder formats.
// Uses the C# reference encoder/decoder to produce known BGRA outputs
// for each raw pixel format. Output: .bin (BGRA32), .enc (raw encoded),
// .meta (decoder params in JSON).
//
// Usage:
//   dotnet run --project tools/GoldenTestGenerator [output_dir]
//   Default output: ../../../../Ithmb-Codec/tests/golden/ (relative to project dir)

using IthmbCodec;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GoldenTestGenerator;

unsafe class Program
{
    static string OutputDir = "";

    // ---- Entry point ----
    static void Main(string[] args)
    {
        // Resolve output directory.
        // Project is at: ithmb-codec-csharp/tools/GoldenTestGenerator/
        // Target:         Ithmb-Codec/tests/golden/
        var projectDir = AppContext.BaseDirectory;
        // Walk up from bin/Debug/net10.0/ to find the project root, then compute target path.
        string baseDir;
        if (args.Length > 0)
        {
            baseDir = args[0];
        }
        else
        {
            // Try resolving from project location
            var cwd = Directory.GetCurrentDirectory();
            if (cwd.EndsWith("GoldenTestGenerator"))
                baseDir = Path.GetFullPath(Path.Combine(cwd, "../../../../Ithmb-Codec/tests/golden"));
            else
                baseDir = Path.GetFullPath(Path.Combine(cwd, "../Ithmb-Codec/tests/golden"));
        }
        OutputDir = baseDir;
        Console.WriteLine($"Golden test vectors -> {OutputDir}");
        Directory.CreateDirectory(OutputDir);

        GenerateAll();
        Console.WriteLine("Done.");
    }

    // ---- Master generator ----
    static void GenerateAll()
    {
        // ====== RGB565 (3 files) ======
        Console.Write("rgb565...");
        GenRgb565("rgb565", "solid_white_2x2", 2, 2, MakeSolid(2, 2, 255, 255, 255));
        GenRgb565("rgb565", "solid_red_2x2", 2, 2, MakeSolid(2, 2, 255, 0, 0));
        GenRgb565("rgb565", "gradient_4x4", 4, 4, MakeGradientRgb(4, 4));
        Console.WriteLine(" 3 files");

        // ====== RGB555 (2 files) ======
        Console.Write("rgb555...");
        GenRgb555("rgb555", "solid_white_2x2", 2, 2, MakeSolid(2, 2, 255, 255, 255));
        GenRgb555("rgb555", "gradient_4x4", 4, 4, MakeGradientRgb(4, 4));
        Console.WriteLine(" 2 files");

        // ====== UYVY (2 files) ======
        Console.Write("uyvy...");
        GenUyvy("uyvy", "solid_white_2x2", 2, 2, MakeSolid(2, 2, 255, 255, 255));
        GenUyvyInterlaced("uyvy", "interlaced_4x4", 4, 4, MakeGradientRgb(4, 4));
        Console.WriteLine(" 2 files");

        // ====== YCbCr420 (2 files) ======
        Console.Write("ycbcr420...");
        GenYcbcr420("ycbcr420", "solid_white_4x4", 4, 4, MakeSolid(4, 4, 255, 255, 255));
        GenYcbcr420("ycbcr420", "gradient_4x4", 4, 4, MakeGradientRgb(4, 4));
        Console.WriteLine(" 2 files");

        // ====== CL (1 file) ======
        Console.Write("cl...");
        GenCl("cl", "solid_white_4x4", 4, 4, MakeSolid(4, 4, 255, 255, 255));
        Console.WriteLine(" 1 file");

        // ====== CLCL (1 file) ======
        Console.Write("clcl...");
        GenClcl("clcl", "solid_white_4x4", 4, 4, MakeSolid(4, 4, 255, 255, 255));
        Console.WriteLine(" 1 file");

        // ====== JPEG (1 file) ======
        Console.Write("jpeg...");
        GenJpeg("jpeg", "solid_white_2x2", 2, 2, MakeSolid(2, 2, 255, 255, 255));
        Console.WriteLine(" 1 file");
    }

    // ---- Per-format generators ----

    static void GenRgb565(string dir, string name, int w, int h, byte[] bgra)
    {
        // Encode (little-endian, standard RGB order)
        byte[] encoded = IthmbCodecPlugin.EncodeRgb565(bgra, w, h, bigEndian: false);

        // Decode back to BGRA
        byte[] decoded = new byte[w * h * 4];
        fixed (byte* p = decoded)
        {
            if (!IthmbCodecPlugin.DecodeRgb565(encoded, p, w, h, littleEndian: true, swapRgbChannels: false))
                throw new Exception($"Decode failed: rgb565/{name}");
        }

        var meta = new Dictionary<string, object>
        {
            ["width"] = w,
            ["height"] = h,
            ["encoding"] = "Rgb565",
            ["little_endian"] = true,
            ["swap_rgb_channels"] = false,
        };
        SaveFiles(dir, name, decoded, encoded, meta);
    }

    static void GenRgb555(string dir, string name, int w, int h, byte[] bgra)
    {
        byte[] encoded = IthmbCodecPlugin.EncodeRgb555(bgra, w, h, bigEndian: false, swapRgbChannels: false);

        byte[] decoded = new byte[w * h * 4];
        fixed (byte* p = decoded)
        {
            if (!IthmbCodecPlugin.DecodeRgb555(encoded, p, w, h, littleEndian: true, swapRgbChannels: false))
                throw new Exception($"Decode failed: rgb555/{name}");
        }

        var meta = new Dictionary<string, object>
        {
            ["width"] = w,
            ["height"] = h,
            ["encoding"] = "Rgb555",
            ["little_endian"] = true,
            ["swap_rgb_channels"] = false,
        };
        SaveFiles(dir, name, decoded, encoded, meta);
    }

    static void GenUyvy(string dir, string name, int w, int h, byte[] bgra)
    {
        byte[] encoded = IthmbCodecPlugin.EncodeUyvy(bgra, w, h);

        byte[] decoded = new byte[w * h * 4];
        fixed (byte* p = decoded)
        {
            if (!IthmbCodecPlugin.DecodeYuv422(encoded, p, w, h))
                throw new Exception($"Decode failed: uyvy/{name}");
        }

        var meta = new Dictionary<string, object>
        {
            ["width"] = w,
            ["height"] = h,
            ["encoding"] = "Yuv422",
            ["little_endian"] = true,
        };
        SaveFiles(dir, name, decoded, encoded, meta);
    }

    static void GenUyvyInterlaced(string dir, string name, int w, int h, byte[] bgra)
    {
        // Use BuildIthmbFile with IsInterlaced=true to get correct field reordering.
        // InterlaceFields is private, so we go through the full file builder.
        var profile = new IthmbCodecPlugin.IthmbVariantProfile(
            Prefix: 9999, Width: w, Height: h,
            Encoding: IthmbCodecPlugin.IthmbEncoding.Yuv422,
            FrameByteLength: w * h * 2,
            IsInterlaced: true);

        byte[] ithmbFile = IthmbCodecPlugin.BuildIthmbFile(bgra, w, h, profile);
        // Strip the 4-byte prefix to get raw encoded data
        byte[] encoded = ithmbFile[4..];

        byte[] decoded = new byte[w * h * 4];
        fixed (byte* p = decoded)
        {
            if (!IthmbCodecPlugin.DecodeYuv422Interlaced(encoded, p, w, h))
                throw new Exception($"Decode failed: uyvy_interlaced/{name}");
        }

        var meta = new Dictionary<string, object>
        {
            ["width"] = w,
            ["height"] = h,
            ["encoding"] = "Yuv422",
            ["little_endian"] = true,
            ["is_interlaced"] = true,
        };
        SaveFiles(dir, name, decoded, encoded, meta);
    }

    static void GenYcbcr420(string dir, string name, int w, int h, byte[] bgra)
    {
        byte[] encoded = IthmbCodecPlugin.EncodeYcbcr420(bgra, w, h, swapChromaPlanes: false);

        byte[] decoded = new byte[w * h * 4];
        fixed (byte* p = decoded)
        {
            if (!IthmbCodecPlugin.DecodeYcbcr420(encoded, p, w, h, swapChromaPlanes: false))
                throw new Exception($"Decode failed: ycbcr420/{name}");
        }

        var meta = new Dictionary<string, object>
        {
            ["width"] = w,
            ["height"] = h,
            ["encoding"] = "Ycbcr420",
            ["little_endian"] = true,
            ["swap_chroma_planes"] = false,
        };
        SaveFiles(dir, name, decoded, encoded, meta);
    }

    static void GenCl(string dir, string name, int w, int h, byte[] bgra)
    {
        byte[] encoded = IthmbCodecPlugin.EncodeCl(bgra, w, h);

        byte[] decoded = new byte[w * h * 4];
        fixed (byte* p = decoded)
        {
            if (!IthmbCodecPlugin.DecodeYuv422Cl(encoded, p, w, h))
                throw new Exception($"Decode failed: cl/{name}");
        }

        var meta = new Dictionary<string, object>
        {
            ["width"] = w,
            ["height"] = h,
            ["encoding"] = "Yuv422",
            ["little_endian"] = true,
            ["cl_chroma"] = true,
        };
        SaveFiles(dir, name, decoded, encoded, meta);
    }

    static void GenClcl(string dir, string name, int w, int h, byte[] bgra)
    {
        byte[] encoded = IthmbCodecPlugin.EncodeClcl(bgra, w, h);

        byte[] decoded = new byte[w * h * 4];
        fixed (byte* p = decoded)
        {
            if (!IthmbCodecPlugin.DecodeYuv422Clcl(encoded, p, w, h))
                throw new Exception($"Decode failed: clcl/{name}");
        }

        var meta = new Dictionary<string, object>
        {
            ["width"] = w,
            ["height"] = h,
            ["encoding"] = "Yuv422",
            ["little_endian"] = true,
            ["clcl_chroma"] = true,
        };
        SaveFiles(dir, name, decoded, encoded, meta);
    }

    static void GenJpeg(string dir, string name, int w, int h, byte[] bgra)
    {
        // For JPEG, we create a minimal valid JPEG file using ffmpeg,
        // then decode it to BGRA for the golden .bin.
        // The C# codec uses StbImageSharp for JPEG decode.

        // Write BGRA as raw file for ffmpeg
        string tmpDir = Path.Combine(Path.GetTempPath(), "golden_jpeg_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        try
        {
            string rawFile = Path.Combine(tmpDir, "input.raw");
            string jpegFile = Path.Combine(tmpDir, "output.jpg");
            string ppmFile = Path.Combine(tmpDir, "input.ppm");

            // First, convert BGRA to PPM (simple header + RGB bytes).
            // PPM binary format: P6 header, then width height maxval, then RGB data.
            // We need to convert BGRA to RGB.
            using (var fs = new FileStream(ppmFile, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(System.Text.Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n"));
                for (int i = 0; i < w * h; i++)
                {
                    bw.Write(bgra[i * 4 + 2]); // R
                    bw.Write(bgra[i * 4 + 1]); // G
                    bw.Write(bgra[i * 4]);     // B
                }
            }

            // Run ffmpeg to convert PPM to JPEG (quality 95, no subsampling for accuracy)
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-y -f image2 -i \"{ppmFile}\" -q:v 2 -pix_fmt yuvj444p \"{jpegFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            var proc = System.Diagnostics.Process.Start(psi)!;
            proc.WaitForExit(15000);
            if (proc.ExitCode != 0)
            {
                string err = proc.StandardError.ReadToEnd();
                Console.Error.WriteLine($"ffmpeg warning (exit {proc.ExitCode}): {err}");
            }

            // Read the JPEG file
            byte[] jpegBytes = File.ReadAllBytes(jpegFile);

            // Decode the JPEG using StbImageSharp to get BGRA
            // The C# codec uses StbImageSharp internally for JPEG decode.
            // We call it directly here.
            var result = StbImageSharp.ImageResult.FromMemory(jpegBytes, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            byte[] decoded = new byte[result.Width * result.Height * 4];

            // StbImageSharp returns BGRA data (the library's default is BGRA)
            // Copy to our buffer
            for (int i = 0; i < result.Width * result.Height; i++)
            {
                int srcOff = i * 4;
                decoded[srcOff] = result.Data[srcOff];     // B
                decoded[srcOff + 1] = result.Data[srcOff + 1]; // G
                decoded[srcOff + 2] = result.Data[srcOff + 2]; // R
                decoded[srcOff + 3] = 255;                // A
            }

            // If the JPEG decoder returned different dimensions (rounded up to MCU block),
            // we still use the actual decoded dimensions in meta.
            int actualW = result.Width;
            int actualH = result.Height;

            var meta = new Dictionary<string, object>
            {
                ["width"] = actualW,
                ["height"] = actualH,
                ["encoding"] = "Jpeg",
                ["little_endian"] = true,
            };
            SaveFiles(dir, name, decoded, jpegBytes, meta);

            Console.Write($"(ffmpeg JPEG: {jpegBytes.Length} bytes, decoded {actualW}x{actualH}) ");
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, recursive: true);
        }
    }

    // ---- BGRA pixel data generators ----

    static byte[] MakeSolid(int w, int h, byte r, byte g, byte b)
    {
        var pixels = new byte[w * h * 4];
        for (int i = 0; i < w * h; i++)
        {
            pixels[i * 4] = b;        // B
            pixels[i * 4 + 1] = g;    // G
            pixels[i * 4 + 2] = r;    // R
            pixels[i * 4 + 3] = 255;  // A
        }
        return pixels;
    }

    static byte[] MakeGradientRgb(int w, int h)
    {
        var pixels = new byte[w * h * 4];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = (y * w + x) * 4;
                byte r = (byte)((255 * x) / (w - 1));
                byte g = (byte)((255 * y) / (h - 1));
                byte b = (byte)(128 + (127 * (x + y)) / (w + h - 2));
                pixels[i] = b;
                pixels[i + 1] = g;
                pixels[i + 2] = r;
                pixels[i + 3] = 255;
            }
        }
        return pixels;
    }

    // ---- File output ----

    static void SaveFiles(string dir, string name, byte[] decoded, byte[] encoded, Dictionary<string, object> meta)
    {
        var fullDir = Path.Combine(OutputDir, dir);
        Directory.CreateDirectory(fullDir);

        // .bin = decoded BGRA32 pixels (what the decoder should output)
        File.WriteAllBytes(Path.Combine(fullDir, $"{name}.bin"), decoded);

        // .enc = raw encoded data (what you feed into the decoder)
        File.WriteAllBytes(Path.Combine(fullDir, $"{name}.enc"), encoded);

        // .meta = decoder parameters
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
        // Use OrderedDictionary to put "width" and "height" first
        var ordered = new Dictionary<string, object>();
        if (meta.TryGetValue("width", out var w)) ordered["width"] = w;
        if (meta.TryGetValue("height", out var h)) ordered["height"] = h;
        if (meta.TryGetValue("encoding", out var e)) ordered["encoding"] = e;
        foreach (var kv in meta)
        {
            if (kv.Key != "width" && kv.Key != "height" && kv.Key != "encoding")
                ordered[kv.Key] = kv.Value;
        }
        string json = JsonSerializer.Serialize(ordered, options);
        File.WriteAllText(Path.Combine(fullDir, $"{name}.meta"), json);
    }
}
