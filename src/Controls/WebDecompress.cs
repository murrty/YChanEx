#nullable enable
namespace murrty.controls;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
internal static class WebDecompress {
    internal static async Task<byte[]> Brotli(Stream inputStream) {
        inputStream.Position = 0;
        using MemoryStream outputStream = new();
        using Org.Brotli.Dec.BrotliInputStream DecompressorStream = new(inputStream);
        await DecompressorStream.CopyToAsync(outputStream);
        return outputStream.ToArray();
    }
    internal static async Task Brotli(Stream inputStream, Stream outputStream) {
        inputStream.Position = 0;
        using Org.Brotli.Dec.BrotliInputStream DecompressorStream = new(inputStream);
        await DecompressorStream.CopyToAsync(outputStream);
    }
    internal static Stream BrotliStream(Stream inputStream) {
        return new Org.Brotli.Dec.BrotliInputStream(inputStream, false);
    }

    internal static async Task<byte[]> GZip(Stream inputStream) {
        inputStream.Position = 0;
        using MemoryStream outputStream = new();
        using GZipStream DecompressorStream = new(inputStream, CompressionMode.Decompress);
        await DecompressorStream.CopyToAsync(outputStream);
        return outputStream.ToArray();
    }
    internal static async Task GZip(Stream inputStream, Stream outputStream) {
        inputStream.Position = 0;
        using GZipStream DecompressorStream = new(inputStream, CompressionMode.Decompress);
        await DecompressorStream.CopyToAsync(outputStream);
    }
    internal static Stream GZipStream(Stream inputStream) {
        return new GZipStream(inputStream, CompressionMode.Decompress, false);
    }

    internal static async Task<byte[]> Deflate(Stream inputStream) {
        inputStream.Position = 0;
        using MemoryStream outputStream = new();
        using DeflateStream DecompressorStream = new(inputStream, CompressionMode.Decompress);
        await DecompressorStream.CopyToAsync(outputStream);
        return outputStream.ToArray();
    }
    internal static async Task Deflate(Stream inputStream, Stream outputStream) {
        inputStream.Position = 0;
        using DeflateStream DecompressorStream = new(inputStream, CompressionMode.Decompress);
        await DecompressorStream.CopyToAsync(outputStream);
    }
    internal static Stream DeflateStream(Stream inputStream) {
        return new DeflateStream(inputStream, CompressionMode.Decompress, false);
    }
}
