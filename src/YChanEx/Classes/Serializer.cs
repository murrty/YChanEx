#nullable enable
namespace YChanEx;
public static class Serializer {
    private static readonly System.Runtime.Serialization.Json.DataContractJsonSerializerSettings SerializerSettings = new() {
        UseSimpleDictionaryFormat = true,
        SerializeReadOnlyTypes = false,
    };

    public static string JsonSerialize<T>(this T value) {
        //var Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
        //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        //string returnValue;
        //try {
        //    using System.IO.MemoryStream Stream = new();
        //    var Writer = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonWriter(
        //        Stream, System.Text.Encoding.UTF8, true, true, "    ");
        //    System.Runtime.Serialization.Json.DataContractJsonSerializer Serializer = new(
        //        typeof(T), 
        //        new System.Runtime.Serialization.Json.DataContractJsonSerializerSettings() {
        //            UseSimpleDictionaryFormat = true,
        //        }
        //    );
        //    Serializer.WriteObject(Writer, value);
        //    byte[] bytes = Stream.ToArray();
        //    Writer.Flush();
        //    Stream.Close();
        //    returnValue = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        //}
        //catch {
        //    returnValue = null;
        //}
        //finally {
        //    System.Threading.Thread.CurrentThread.CurrentCulture = Culture;
        //}

        //return returnValue;
        return JsonSerialize<T>(value, System.Text.Encoding.UTF8);
    }
    public static string JsonSerialize<T>(this T value, System.Text.Encoding Encoder) {
        using System.IO.MemoryStream Stream = new();
        System.Runtime.Serialization.Json.DataContractJsonSerializer Serializer = new(typeof(T), SerializerSettings);
        Serializer.WriteObject(Stream, value);
        byte[] json = Stream.ToArray();
        return Encoder.GetString(json, 0, json.Length);
    }

    public static T? JsonDeserialize<T>(this string value) {
        return JsonDeserialize<T?>(value, System.Text.Encoding.UTF8);
    }
    public static T? JsonDeserialize<T>(this string value, System.Text.Encoding Encoder) {
        using System.IO.MemoryStream ms = new(Encoder.GetBytes(value));
        System.Runtime.Serialization.Json.DataContractJsonSerializer ser = new(typeof(T), SerializerSettings);
        object val = ser.ReadObject(ms);
        return val is T t ? t : default;
    }
}
