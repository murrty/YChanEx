#nullable enable
namespace YChanEx;
/// <summary>
/// Enumeration of file download status, that determines the current state of the file.
/// </summary>
public enum FileDownloadStatus : byte {
    /// <summary>
    /// No attempt to download the file has occurred yet.
    /// <para>A download attempt will occur.</para>
    /// </summary>
    Undownloaded = 0,
    /// <summary>
    /// The file successfully downloaded.
    /// <para>No download attempt will occur for this file again.</para>
    /// </summary>
    Downloaded = 1,
    /// <summary>
    /// The file was given a 404, and is assumed to be deleted.
    /// <para>No download attempt will occur for this file again.</para>
    /// </summary>
    FileNotFound = 2,
    /// <summary>
    /// The file was not able to be downloaded.
    /// <para>A download attempt will occur.</para>
    /// </summary>
    Error = 3,
    /// <summary>
    /// The file was removed from the thread.<para/>
    /// <para>No download attempt will occur for this file again.</para>
    /// </summary>
    RemovedFromThread = 4,
}