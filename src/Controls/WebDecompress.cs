namespace murrty.controls;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
internal static class WebDecompress {
    public static async Task<byte[]> Brotli(Stream inputStream) {
        inputStream.Position = 0;
        using MemoryStream outputStream = new();
        using Org.Brotli.Dec.BrotliInputStream compressionStream = new(inputStream);
        await compressionStream.CopyToAsync(outputStream);
        return outputStream.ToArray();
    }
    public static async Task<byte[]> GZip(Stream inputStream) {
        inputStream.Position = 0;
        using MemoryStream outputStream = new();
        using GZipStream compressionStream = new(inputStream, CompressionMode.Decompress);
        await compressionStream.CopyToAsync(outputStream);
        return outputStream.ToArray();
    }
    public static async Task<byte[]> Deflate(Stream inputStream) {
        inputStream.Position = 0;
        using MemoryStream outputStream = new();
        using DeflateStream compressionStream = new(inputStream, CompressionMode.Decompress);
        await compressionStream.CopyToAsync(outputStream);
        return outputStream.ToArray();
    }
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