using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace IthmbCodec.Benchmark.Decoders;

[Config(typeof(BenchmarkConfig))]
public unsafe class ReorderedRgb555Bench
{
    private byte[] _src = null!;
    private IntPtr _dst;
    private int _w = 256;
    private int _h = 256;

    [Params(64, 128, 256)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _w = Size;
        _h = Size;
        _src = new byte[_w * _h * 2];
        Random.Shared.NextBytes(_src);
        _dst = Marshal.AllocHGlobal(_w * _h * 4);
    }

    [GlobalCleanup]
    public void Cleanup() => Marshal.FreeHGlobal(_dst);

    [Benchmark]
    public bool DecodeReorderedRgb555()
    {
        return IthmbCodecPlugin.DecodeReorderedRgb555(_src, (byte*)_dst, _w, _h, littleEndian: true);
    }
}
