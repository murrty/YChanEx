#nullable enable
namespace YChanEx;
using System;
using System.Net.Http;
using System.Reflection;
internal sealed class RequestMessage : HttpRequestMessage {
    private const string StatusFieldName = // C# 6+ requires the field name to be "_sendStatus" instead.
#if NETCOREAPP
        "_sendStatus";
#else
    "sendStatus";
#endif
    internal const string TimeoutKey = "RequestTimeout";

    private static readonly FieldInfo RequestSentField = typeof(HttpRequestMessage).GetTypeInfo()
            .GetField(StatusFieldName, BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new NullReferenceException("Could not find the request message sent field.");

    public static void ResetRequest(HttpRequestMessage message) {
        RequestSentField.SetValue(message, 0);
    }

    public void Reset() {
        ResetRequest(this);
    }
}
