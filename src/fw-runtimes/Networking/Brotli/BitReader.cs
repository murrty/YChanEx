/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/
namespace Org.Brotli.Dec;
/// <summary>Bit reading helpers.</summary>
internal sealed class BitReader {
    /// <summary>
    /// Input byte buffer, consist of a ring-buffer and a "slack" region where bytes from the start of
    /// the ring-buffer are copied.
    /// </summary>
    private const int Capacity = 1024;
    private const int Slack = 16;
    private const int IntBufferSize = Capacity + Slack;
    private const int ByteReadSize = Capacity << 2;
    private const int ByteBufferSize = IntBufferSize << 2;
    private readonly byte[] byteBuffer = new byte[ByteBufferSize];
    private readonly int[] intBuffer = new int[IntBufferSize];
    private readonly IntReader intReader = new();
    private System.IO.Stream input;
    /// <summary>Input stream is finished.</summary>
    private bool endOfStreamReached;
    /// <summary>Pre-fetched bits.</summary>
    internal long accumulator;
    /// <summary>Current bit-reading position in accumulator.</summary>
    internal int bitOffset;
    /// <summary>Offset of next item in intBuffer.</summary>
    private int intOffset;
    private int tailBytes;

    /* Number of bytes in unfinished "int" item. */
    /// <summary>Fills up the input buffer.</summary>
    /// <remarks>
    /// Fills up the input buffer.
    /// <p> No-op if there are at least 36 bytes present after current position.
    /// <p> After encountering the end of the input stream, 64 additional zero bytes are copied to the
    /// buffer.
    /// </remarks>
    internal static void ReadMoreInput(BitReader br) {
        if (br.intOffset <= Capacity - 9) {
            return;
        }
        if (br.endOfStreamReached) {
            if (IntAvailable(br) >= -2) {
                return;
            }
            throw new BrotliRuntimeException("No more input");
        }
        int readOffset = br.intOffset << 2;
        int bytesRead = ByteReadSize - readOffset;
        Array.Copy(br.byteBuffer, readOffset, br.byteBuffer, 0, bytesRead);
        br.intOffset = 0;
        try {
            while (bytesRead < ByteReadSize) {
                int len = br.input.Read(br.byteBuffer, bytesRead, ByteReadSize - bytesRead);
                // EOF is -1 in Java, but 0 in C#.
                if (len <= 0) {
                    br.endOfStreamReached = true;
                    br.tailBytes = bytesRead;
                    bytesRead += 3;
                    break;
                }
                bytesRead += len;
            }
        }
        catch (System.IO.IOException e) {
            throw new BrotliRuntimeException("Failed to read input", e);
        }
        IntReader.Convert(br.intReader, bytesRead >> 2);
    }

    internal static void CheckHealth(BitReader br, bool endOfStream) {
        if (!br.endOfStreamReached) {
            return;
        }
        int byteOffset = (br.intOffset << 2) + ((br.bitOffset + 7) >> 3) - 8;
        if (byteOffset > br.tailBytes) {
            throw new BrotliRuntimeException("Read after end");
        }
        if (endOfStream && (byteOffset != br.tailBytes)) {
            throw new BrotliRuntimeException("Unused bytes after end");
        }
    }

    /// <summary>Advances the Read buffer by 5 bytes to make room for reading next 24 bits.</summary>
    internal static void FillBitWindow(BitReader br) {
        if (br.bitOffset >= 32) {
            br.accumulator = ((long)br.intBuffer[br.intOffset++] << 32) | ((long)(((ulong)br.accumulator) >> 32));
            br.bitOffset -= 32;
        }
    }

    /// <summary>Reads the specified number of bits from Read Buffer.</summary>
    internal static int ReadBits(BitReader br, int n) {
        FillBitWindow(br);
        int val = (int)((long)(((ulong)br.accumulator) >> br.bitOffset)) & ((1 << n) - 1);
        br.bitOffset += n;
        return val;
    }

    /// <summary>Initialize bit reader.</summary>
    /// <remarks>
    /// Initialize bit reader.
    /// <p> Initialisation turns bit reader to a ready state. Also a number of bytes is prefetched to
    /// accumulator. Because of that this method may block until enough data could be read from input.
    /// </remarks>
    /// <param name="br">BitReader POJO</param>
    /// <param name="input">data source</param>
    internal static void Init(BitReader br, System.IO.Stream input) {
        if (br.input != null) {
            throw new InvalidOperationException("Bit reader already has associated input stream");
        }
        IntReader.Init(br.intReader, br.byteBuffer, br.intBuffer);
        br.input = input;
        br.accumulator = 0;
        br.bitOffset = 64;
        br.intOffset = Capacity;
        br.endOfStreamReached = false;
        Prepare(br);
    }

    private static void Prepare(BitReader br) {
        ReadMoreInput(br);
        CheckHealth(br, false);
        FillBitWindow(br);
        FillBitWindow(br);
    }

    internal static void Reload(BitReader br) {
        if (br.bitOffset == 64) {
            Prepare(br);
        }
    }

    internal static void Close(BitReader br, bool leaveOpen) {
        System.IO.Stream @is = br.input;
        br.input = null;
        if (!leaveOpen) {
            @is?.Close();
        }
    }

    internal static void JumpToByteBoundary(BitReader br) {
        int padding = (64 - br.bitOffset) & 7;
        if (padding != 0) {
            int paddingBits = BitReader.ReadBits(br, padding);
            if (paddingBits != 0) {
                throw new BrotliRuntimeException("Corrupted padding bits");
            }
        }
    }

    internal static int IntAvailable(BitReader br) {
        int limit = Capacity;
        if (br.endOfStreamReached) {
            limit = (br.tailBytes + 3) >> 2;
        }
        return limit - br.intOffset;
    }

    internal static void CopyBytes(BitReader br, byte[] data, int offset, int length) {
        if ((br.bitOffset & 7) != 0) {
            throw new BrotliRuntimeException("Unaligned copyBytes");
        }
        // Drain accumulator.
        while ((br.bitOffset != 64) && (length != 0)) {
            data[offset++] = unchecked((byte)((long)(((ulong)br.accumulator) >> br.bitOffset)));
            br.bitOffset += 8;
            length--;
        }
        if (length == 0) {
            return;
        }
        // Get data from shadow buffer with "sizeof(int)" granularity.
        int copyInts = Math.Min(IntAvailable(br), length >> 2);
        if (copyInts > 0) {
            int readOffset = br.intOffset << 2;
            Array.Copy(br.byteBuffer, readOffset, data, offset, copyInts << 2);
            offset += copyInts << 2;
            length -= copyInts << 2;
            br.intOffset += copyInts;
        }
        if (length == 0) {
            return;
        }
        // Read tail bytes.
        if (IntAvailable(br) > 0) {
            // length = 1..3
            FillBitWindow(br);
            while (length != 0) {
                data[offset++] = unchecked((byte)((long)(((ulong)br.accumulator) >> br.bitOffset)));
                br.bitOffset += 8;
                length--;
            }
            CheckHealth(br, false);
            return;
        }
        // Now it is possible to copy bytes directly.
        try {
            while (length > 0) {
                int len = br.input.Read(data, offset, length);
                if (len == -1) {
                    throw new BrotliRuntimeException("Unexpected end of input");
                }
                offset += len;
                length -= len;
            }
        }
        catch (System.IO.IOException e) {
            throw new BrotliRuntimeException("Failed to read input", e);
        }
    }
}
