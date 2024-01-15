﻿/*
Copyright © 2012-2015 Ruslan Khuduev <x-rus@list.ru>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
namespace SocksSharp.Proxy.Response;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocksSharp.Extensions;
using SocksSharp.Helpers;
internal class ResponseBuilder : IResponseBuilder {
    private sealed class BytesWraper {
        public int Length { get; set; }

        public byte[] Value { get; set; }
    }

    private static readonly byte[] openHtmlSignature = Encoding.ASCII.GetBytes("<html");
    private static readonly byte[] closeHtmlSignature = Encoding.ASCII.GetBytes("</html>");

    private readonly string newLine = "\r\n";
    private readonly int bufferSize;

    private int contentLength;

    private NetworkStream networkStream;
    private Stream commonStream;

    private HttpResponseMessage response;
    private Dictionary<string, List<string>> contentHeaders;

    private readonly CookieContainer cookies;
    private readonly Uri uri;

    private readonly ReceiveHelper receiveHelper;

    private CancellationToken cancellationToken;

    public int ReceiveTimeout { get; set; }

    public ResponseBuilder(int bufferSize, CookieContainer cookies = null, Uri uri = null) {
        this.bufferSize = bufferSize;

        this.cookies = cookies;
        this.uri = uri;

        contentLength = -1;
        receiveHelper = new ReceiveHelper(bufferSize);
    }

    public Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, Stream stream) {
        return GetResponseAsync(request, stream, CancellationToken.None);
    }

    public Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, Stream stream, CancellationToken cancellationToken) {
        this.networkStream = stream as NetworkStream;
        this.commonStream = stream;

        this.cancellationToken = cancellationToken;

        receiveHelper.Init(stream);

        response = new HttpResponseMessage();
        contentHeaders = [];

        response.RequestMessage = request;

        var task = Task.Run(() => {
            ReceiveStartingLine();
            ReceiveHeaders();
            ReceiveContent();

            return response;
        });

        return task;
    }

    private void ReceiveStartingLine() {
        string startingLine;
        while (true) {
            startingLine = receiveHelper.ReadLine();
            if (startingLine.Length == 0) {
                //throw exception;
            }
            else if (startingLine == newLine) {
                continue;
            }
            else {
                break;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        string version = startingLine.Substring("HTTP/", " ");
        string statusCode = startingLine.Substring(" ", " ");
        if (statusCode.Length == 0) {
            // Если сервер не возвращает Reason Phrase
            statusCode = startingLine.Substring(" ", newLine);
        }
        if (version.Length == 0 || statusCode.Length == 0) {
            throw new ProxyException("Received empty response");
        }

        response.Version = System.Version.Parse(version);
        response.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCode);
    }

    private void ReceiveHeaders() {
        while (true) {
            string header = receiveHelper.ReadLine();

            if (header == newLine) {
                return;
            }

            int separatorPos = header.IndexOf(':');
            if (separatorPos == -1) {
                //string message = string.Format(
                //Resources.HttpException_WrongHeader, header, Address.Host);
                //throw NewHttpException(message);
            }
            string headerName = header[..separatorPos];
            string headerValue = header[(separatorPos + 1)..].Trim(' ', '\t', '\r', '\n');

            if (cookies != null && headerName.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)) {
                SetCookie(headerValue);
            }
            else if (ContentHelper.IsContentHeader(headerName)) {
                if (contentHeaders.TryGetValue(headerName, out List<string> values)) {
                    values.Add(headerValue);
                }
                else {
                    values = [
                        headerValue
                    ];

                    contentHeaders.Add(headerName, values);
                }
            }
            else {
                response.Headers.TryAddWithoutValidation(headerName, headerValue);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private void ReceiveContent() {
        if (contentHeaders.Count != 0) {
            contentLength = GetContentLength();

            if (contentLength < 0) {
                response.Content = new StreamContent(Stream.Null);
            }
            else {
                var memoryStream = new MemoryStream(contentLength);

                try {
                    IEnumerable<BytesWraper> source = GetMessageBodySource();
                    foreach (var bytes in source) {
                        memoryStream.Write(bytes.Value, 0, bytes.Length);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch (Exception ex) {
                    if (ex is IOException || ex is InvalidOperationException) {
                        //throw NewHttpException(Resources.HttpException_FailedReceiveMessageBody, ex);
                    }

                    throw;
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                response.Content = new StreamContent(memoryStream);
                foreach (var pair in contentHeaders) {
                    response.Content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
                }
            }
        }
    }

    private void SetCookie(string value) {
        if (value.Length == 0) {
            return;
        }

        // Ищем позицию, где заканчивается куки и начинается описание его параметров.
        int endCookiePos = value.IndexOf(';');

        // Ищем позицию между именем и значением куки.
        int separatorPos = value.IndexOf('=');

        if (separatorPos == -1) {
            throw new InvalidDataException("Invalid cookie");
        }

        string cookieValue;
        string cookieName = value[..separatorPos];

        if (endCookiePos == -1) {
            cookieValue = value[(separatorPos + 1)..];
        }
        else {
            cookieValue = value.Substring(separatorPos + 1, endCookiePos - separatorPos - 1);

            #region Get cookie expires time
            int expiresPos = value.IndexOf("expires=");

            if (expiresPos != -1) {
                string expiresStr;
                int endExpiresPos = value.IndexOf(';', expiresPos);

                expiresPos += 8;

                if (endExpiresPos == -1) {
                    expiresStr = value[expiresPos..];
                }
                else {
                    expiresStr = value[expiresPos..endExpiresPos];
                }

                // Если время куки вышло, то удаляем её.
                if (DateTime.TryParse(expiresStr, out DateTime expires) &&
                    expires < DateTime.Now) {
                    var collection = cookies.GetCookies(uri);
                    if (collection[cookieName] != null)
                        collection[cookieName].Expired = true;
                }
            }

            #endregion
        }

        // Если куки нужно удалить.
        if (cookieValue.Length == 0 ||
            cookieValue.Equals("deleted", StringComparison.OrdinalIgnoreCase)) {
            var collection = cookies.GetCookies(uri);
            if (collection[cookieName] != null)
                collection[cookieName].Expired = true;
        }
        else {
            cookies.Add(new Cookie(cookieName, cookieValue, "/", uri.Host));
        }
    }

    private IEnumerable<BytesWraper> GetMessageBodySource() {
        return GetMessageBodySourceStd();
    }

    // Загрузка обычных данных.
    private IEnumerable<BytesWraper> GetMessageBodySourceStd() {
        if (response.Headers.Contains("Transfer-Encoding")) {
            return ReceiveMessageBodyChunked();
        }
        if (contentLength != -1) {
            return ReceiveMessageBody(contentLength);
        }

        return ReceiveMessageBody(commonStream);
    }

    private int GetContentLength() {
        if (contentHeaders.TryGetValue("Content-Length", out List<string> values)) {
            if (Int32.TryParse(values[0], out int length)) {
                return length;
            }
        }

        return -1;
    }

    private void WaitData() {
        int sleepTime = 0;
        int delay = (ReceiveTimeout < 10) ?
            10 : ReceiveTimeout;

        var dataAvailable = networkStream?.DataAvailable;
        while (dataAvailable == false) {
            if (sleepTime >= delay) {
                throw new ProxyException("Wait data timeout");
            }

            sleepTime += 10;
            Thread.Sleep(10);
        }
    }

    #region Receive Content (F*cking trash, but works (not sure (really)))

    // Загрузка тела сообщения неизвестной длины.
    private IEnumerable<BytesWraper> ReceiveMessageBody(Stream stream) {
        var bytesWraper = new BytesWraper();
        byte[] buffer = new byte[this.bufferSize];
        bytesWraper.Value = buffer;
        int begBytesRead = 0;

        // Считываем начальные данные из тела сообщения.
        if (receiveHelper.HasData) {
            begBytesRead = receiveHelper.Read(buffer, 0, bufferSize);
        }
        if (begBytesRead < bufferSize) {
            begBytesRead += stream.Read(buffer, begBytesRead, bufferSize - begBytesRead);
        }

        // Возвращаем начальные данные.
        bytesWraper.Length = begBytesRead;
        yield return bytesWraper;
        // Проверяем, есть ли открывающий тег '<html'.
        // Если есть, то считываем данные то тех пор, пока не встретим закрывающий тек '</html>'.
        bool isHtml = FindSignature(buffer, begBytesRead, openHtmlSignature);
        if (isHtml) {
            bool found = FindSignature(buffer, begBytesRead, closeHtmlSignature);
            // Проверяем, есть ли в начальных данных закрывающий тег.
            if (found) {
                yield break;
            }
        }
        while (true) {
            int bytesRead = stream.Read(buffer, 0, bufferSize);
            // Если тело сообщения представляет HTML.
            if (isHtml) {
                if (bytesRead == 0) {
                    WaitData();
                    continue;
                }
                bool found = FindSignature(buffer, bytesRead, closeHtmlSignature);
                if (found) {
                    bytesWraper.Length = bytesRead;
                    yield return bytesWraper;
                    yield break;
                }
            }
            else if (bytesRead == 0) {
                yield break;
            }
            bytesWraper.Length = bytesRead;
            yield return bytesWraper;
        }
    }

    // Загрузка тела сообщения известной длины.
    private IEnumerable<BytesWraper> ReceiveMessageBody(int contentLength) {
        //Stream stream = _request.ClientStream;
        var bytesWraper = new BytesWraper();
        byte[] buffer = new byte[bufferSize];
        bytesWraper.Value = buffer;

        int totalBytesRead = 0;
        while (totalBytesRead != contentLength) {
            int bytesRead;
            if (receiveHelper.HasData) {
                bytesRead = receiveHelper.Read(buffer, 0, bufferSize);
            }
            else {
                bytesRead = commonStream.Read(buffer, 0, bufferSize);
            }
            if (bytesRead == 0) {
                WaitData();
            }
            else {
                totalBytesRead += bytesRead;
                bytesWraper.Length = bytesRead;
                yield return bytesWraper;
            }
        }
    }

    // Загрузка тела сообщения частями.
    private IEnumerable<BytesWraper> ReceiveMessageBodyChunked() {
        //Stream stream = _request.ClientStream;
        var bytesWraper = new BytesWraper();
        byte[] buffer = new byte[this.bufferSize];
        bytesWraper.Value = buffer;
        while (true) {
            string line = receiveHelper.ReadLine();
            // Если достигнут конец блока.
            if (line == newLine) {
                continue;
            }

            line = line.Trim(' ', '\r', '\n');
            // Если достигнут конец тела сообщения.
            if (line?.Length == 0) {
                yield break;
            }

            int blockLength;
            int totalBytesRead = 0;
            #region Задаём длину блока
            try {
                blockLength = Convert.ToInt32(line, 16);
            }
            catch (Exception ex) {
                if (ex is FormatException || ex is OverflowException) {
                    //throw NewHttpException(string.Format(
                    //Resources.HttpException_WrongChunkedBlockLength, line), ex);
                }
                throw;
            }
            #endregion
            // Если достигнут конец тела сообщения.
            if (blockLength == 0) {
                yield break;
            }

            while (totalBytesRead != blockLength) {
                int length = blockLength - totalBytesRead;
                if (length > bufferSize) {
                    length = bufferSize;
                }
                int bytesRead;
                if (receiveHelper.HasData) {
                    bytesRead = receiveHelper.Read(buffer, 0, length);
                }
                else {
                    bytesRead = commonStream.Read(buffer, 0, length);
                }
                if (bytesRead == 0) {
                    WaitData();
                }
                else {
                    totalBytesRead += bytesRead;
                    bytesWraper.Length = bytesRead;
                    yield return bytesWraper;
                }
            }
        }
    }

    private bool FindSignature(byte[] source, int sourceLength, byte[] signature) {
        int length = (sourceLength - signature.Length) + 1;
        for (int sourceIndex = 0; sourceIndex < length; ++sourceIndex) {
            for (int signatureIndex = 0; signatureIndex < signature.Length; ++signatureIndex) {
                byte sourceByte = source[signatureIndex + sourceIndex];
                char sourceChar = (char)sourceByte;
                if (char.IsLetter(sourceChar)) {
                    sourceChar = char.ToLower(sourceChar);
                }
                sourceByte = (byte)sourceChar;
                if (sourceByte != signature[signatureIndex]) {
                    break;
                }
                else if (signatureIndex == (signature.Length - 1)) {
                    return true;
                }
            }
        }
        return false;
    }

    #endregion
}