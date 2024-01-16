#nullable enable
namespace murrty.controls;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
internal static class WebDecompress {
    internal static async Task Brotli(Stream inputStream, Stream outputStream) {
        inputStream.Position = 0;
        using Org.Brotli.Dec.BrotliInputStream DecompressorStream = new(inputStream);
        await DecompressorStream.CopyToAsync(outputStream);
    }
    internal static async Task GZip(Stream inputStream, Stream outputStream) {
        inputStream.Position = 0;
        using GZipStream DecompressorStream = new(inputStream, CompressionMode.Decompress);
        await DecompressorStream.CopyToAsync(outputStream);
    }
    internal static async Task Deflate(Stream inputStream, Stream outputStream) {
        inputStream.Position = 0;
        using DeflateStream DecompressorStream = new(inputStream, CompressionMode.Decompress);
        await DecompressorStream.CopyToAsync(outputStream);
    }
}
