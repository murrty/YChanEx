namespace YChanEx;
using System.Runtime.Serialization;
/// <summary>
/// Represents a structure of the data representing the version, such as the content type (x-*) and the size of the file.
/// </summary>
[DataContract]
public sealed class GithubAsset {
    /// <summary>
    /// Gets the content type.
    /// </summary>
    [DataMember(Name = "content_type")]
    public string Content { get; set; }

    /// <summary>
    /// Gets the size of the file.
    /// </summary>
    [DataMember(Name = "size")]
    public long Length { get; set; }

    /// <summary>
    /// Initializes an empty asset.
    /// </summary>
    public GithubAsset() { }

    /// <summary>
    /// Initializes an asset.
    /// </summary>
    /// <param name="Content">The content type that the asset represents.</param>
    /// <param name="Length">The length (in bytes) the asset is.</param>
    public GithubAsset(string Content, long Length) {
        this.Content = Content;
        this.Length = Length;
    }
}