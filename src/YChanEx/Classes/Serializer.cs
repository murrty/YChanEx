#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Json;
public static class Serializer {
    private static class GenericSerializer<T> {
        private static readonly DataContractJsonSerializerSettings SerializerSettings = new() {
            UseSimpleDictionaryFormat = true,
            SerializeReadOnlyTypes = false,
        };
        public static readonly DataContractJsonSerializer Serializer = new(typeof(T), SerializerSettings);
    }

    public static string JsonSerialize<T>(this T value) {
        return JsonSerialize<T>(value, System.Text.Encoding.UTF8);
    }
    public static string JsonSerialize<T>(this T value, System.Text.Encoding Encoder) {
        using MemoryStream Stream = new();
        //DataContractJsonSerializer Serializer = new(typeof(T), SerializerSettings);
        //Serializer.WriteObject(Stream, value);
        GenericSerializer<T>.Serializer.WriteObject(Stream, value);
        byte[] json = Stream.ToArray();
        return Encoder.GetString(json, 0, json.Length);
    }
    public static void JsonSerialize<T>(this T value, Stream writeStream) {
        //DataContractJsonSerializer Serializer = new(typeof(T), SerializerSettings);
        //Serializer.WriteObject(writeStream, value);
        GenericSerializer<T>.Serializer.WriteObject(writeStream, value);
    }

    public static T? JsonDeserialize<T>(this string value) {
        return JsonDeserialize<T?>(value, System.Text.Encoding.UTF8);
    }
    public static T? JsonDeserialize<T>(this string value, System.Text.Encoding Encoder) {
        using MemoryStream ms = new(Encoder.GetBytes(value));
        //DataContractJsonSerializer ser = new(typeof(T), SerializerSettings);
        //object val = ser.ReadObject(ms);
        object? val = GenericSerializer<T>.Serializer.ReadObject(ms);
        return val is T t ? t : default;
    }
    public static T? JsonDeserialize<T>(this Stream readStream) {
        //DataContractJsonSerializer ser = new(typeof(T), SerializerSettings);
        //object val = ser.ReadObject(readStream);
        object? val = GenericSerializer<T>.Serializer.ReadObject(readStream);
        return val is T t ? t : default;
    }

    public static bool TryJsonDeserialize<T>(this string value, [NotNullWhen(true)] out T? result) {
        return TryJsonDeserialize<T>(value, System.Text.Encoding.UTF8, out result);
    }
    public static bool TryJsonDeserialize<T>(this string value, System.Text.Encoding Encoder, [NotNullWhen(true)] out T? result) {
        try {
            using MemoryStream ms = new(Encoder.GetBytes(value));
            object? val = GenericSerializer<T>.Serializer.ReadObject(ms);
            if (val is T t) {
                result = t;
                return true;
            }
            result = default;
            return false;
        }
        catch {
            result = default;
            return false;
        }
    }
    public static bool TryJsonDeserialize<T>(this Stream readStream, [NotNullWhen(true)] out T? result) {
        try {
            object? val = GenericSerializer<T>.Serializer.ReadObject(readStream);
            if (val is T t) {
                result = t;
                return true;
            }
            result = default;
            return false;
        }
        catch {
            result = default;
            return false;
        }
    }
}
