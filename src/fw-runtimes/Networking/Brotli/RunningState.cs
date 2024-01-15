/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/
namespace Org.Brotli.Dec;
/// <summary>Enumeration of decoding state-machine.</summary>
internal enum RunningState : byte {
    Uninitialized = 0,
    BlockStart = 1,
    CompressedBlockStart = 2,
    MainLoop = 3,
    ReadMetadata = 4,
    CopyUncompressed = 5,
    InsertLoop = 6,
    CopyLoop = 7,
    CopyWrapBuffer = 8,
    Transform = 9,
    Finished = 10,
    Closed = 11,
    Write = 12,
}
