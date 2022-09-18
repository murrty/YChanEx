// 1.0
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Windows.Forms;

namespace murrty.classes {

    /// <summary>
    /// This class will control the errors that get reported in try-catch statements.
    /// </summary>
    [DebuggerStepThrough]
    internal sealed class Log {

        /// <summary>
        /// The information regarding the current computer.
        /// </summary>
        public static string ComputerVersionInformation { get; private set; }
        /// <summary>
        /// If the log is currently enabled or not.
        /// </summary>
        private static bool LogEnabled { get; set; } = false;
        /// <summary>
        /// If exceptions should be saved to a file.
        /// </summary>
        private static bool SaveExceptionsToFile { get; set; } = false;

        public static void Initialize() {
            EnableLogging();
            Write("Creating ComputerVersionInformation for exceptions.");
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_OperatingSystem");
            ManagementObject info = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            ComputerVersionInformation =
                $"System Caption: {info.Properties["Caption"].Value}\n" +
                $"Version: {info.Properties["Version"].Value}\n" +
                $"Service Pack Major: {info.Properties["ServicePackMajorVersion"].Value}\n" +
                $"Service Pack Minor: {info.Properties["ServicePackMinorVersion"].Value}";

            Write("Creating unhandled exception event.");
            System.AppDomain.CurrentDomain.UnhandledException += (sender, exception) => {
                using murrty.forms.frmException UnrecoverableException = new(new(exception) {
                    Unrecoverable = true
                });
                UnrecoverableException.ShowDialog();
            };
            Write("Creating unhandled thread exception event.");
            Application.ThreadException += (sender, exception) => {
                using murrty.forms.frmException UnrecoverableException = new(new(exception) {
                    Unrecoverable = true
                });
                UnrecoverableException.ShowDialog();
            };
        }

        #region Log handlers

        /// <summary>
        /// Enables the logging form.
        /// </summary>
        public static void EnableLogging() {
            //if (LogEnabled) {
            //    Write("Logging is already enabled.");
            //}
            //else {
            //    LogForm = new();
            //    LogEnabled = true;
            //    Write("Logging has been enabled.", true);
            //}
        }

        /// <summary>
        /// Disables the logging form, but debug logging will still occur.
        /// </summary>
        public static void DisableLogging() {
            //if (LogEnabled) {
            //    LogEnabled = false;
            //    Write("Disabling logging.");

            //    if (LogForm.WindowState == FormWindowState.Minimized || LogForm.WindowState == FormWindowState.Maximized) {
            //        LogForm.Opacity = 0;
            //        LogForm.WindowState = FormWindowState.Normal;
            //    }

            //    LogForm.Dispose();
            //}
        }

        /// <summary>
        /// Shows the log form.
        /// </summary>
        public static void ShowLog() {
            //if (LogEnabled && LogForm != null) {
            //    if (LogForm.IsShown) {
            //        Write("The log form is already shown.");
            //        LogForm.Activate();
            //    }
            //    else {
            //        Write("Showing log");
            //        LogForm.IsShown = true;
            //        LogForm.Show();
            //    }
            //}
        }

        #endregion

        #region Write handlers

        /// <summary>
        /// Writes a message to the log.
        /// </summary>
        /// <param name="message">The message to be sent to the log</param>
        /// <param name="initial">If the message is the initial one to be sent, does not add a new line break.</param>
        public static void Write(string message, bool initial = false) {
            if (!string.IsNullOrWhiteSpace(message)) {
                Debug.Print(message);
                //LogForm?.Append(message, initial);
            }
        }

        /// <summary>
        /// Writes a message from an object to the log, without much parsing.
        /// </summary>
        /// <param name="message">The object to be sent to the log</param>
        /// <param name="initial">If the message is the initial one to be sent, does not add a new line break.</param>
        public static void Write(object message, bool initial = false) {
            if (message != null) {
                Debug.Print(message.ToString());
                //LogForm?.Append(message, initial);
            }
        }

        /// <summary>
        /// Writes an array of messages from the object[] params.
        /// </summary>
        /// <param name="msgs">An indefninte amount of messages to be posted.</param>
        public static void Write(params object[] msgs) {
            string message;
            for (int i = 0; i < msgs.Length; i++) {
                message = msgs[i].ToString();
                if (!string.IsNullOrWhiteSpace(message)) {
                    Debug.Print(message);
                    //LogForm?.Append(message, initial);
                }
            }
        }

        #endregion

        #region Exception Handler

        /// <summary>
        /// Reports a web exception to the log,
        /// shows a new <see cref="murrty.forms.frmException"/> regarding the exception,
        /// and writes the exception to a .log file, if enabled.
        /// </summary>
        /// <param name="ReceivedException">The <see cref="System.Exception"/> (or derivitives of) that was caught.</param>
        /// <param name="ExtraData">Optional extra data that may help pinpoint the exceptions' cause</param>
        public static void ReportException(dynamic ReceivedException, object ExtraData = null) {
            string ExtraMessage = null;

            switch (ReceivedException) {
                case WebException ReceivedWebException: {
                    Write($"A web exception occured at {ExtraData ?? "an unknown site"}.\n\n{ReceivedException}");

                    #region Obscene amount of handles here
                    switch (ReceivedWebException.Status) {

                        #region NameResolutionFailure
                        case WebExceptionStatus.NameResolutionFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nName resolution failure" +
                                            "\nThe name resolver service could not resolve the host name.";
                        }
                        break;
                        #endregion
                        #region ConnectFailure
                        case WebExceptionStatus.ConnectFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nConnection failure" +
                                            "\nThe remote service point could not be contacted at the transport level.";
                        }
                        break;
                        #endregion
                        #region RecieveFailure
                        case WebExceptionStatus.ReceiveFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nRecieve failure" +
                                            "\nA complete response was not received from the remote server.";
                        }
                        break;
                        #endregion
                        #region SendFailure
                        case WebExceptionStatus.SendFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nSend failure" +
                                            "\nA complete response could not be sent to the remote server.";
                        }
                        break;
                        #endregion
                        #region PipelineFailure
                        case WebExceptionStatus.PipelineFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nPipeline failure" +
                                            "\nThe request was a piplined request and the connection was closed before the response was received.";
                        }
                        break;
                        #endregion
                        #region RequestCanceled
                        case WebExceptionStatus.RequestCanceled: {
                        }
                        return;
                        #endregion
                        #region ProtocolError
                        case WebExceptionStatus.ProtocolError: {

                            if (ReceivedWebException.Response is HttpWebResponse WebResponse) {
                                ExtraMessage = (int)WebResponse.StatusCode switch {

                                    // Hold on
                                    #region 100 Continue
                                    100 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 100 - Continue" +
                                           "\nThe server is processing the request and nothing bad has occured yet. The client may continue with the request, or ignore the response.",
                                    #endregion

                                    #region 101 Switching protocols
                                    101 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 101 - Switching protocols" +
                                           "\nThe server is switching to another protocol by request of the client.",
                                    #endregion

                                    #region 103 Early hints
                                    103 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 103 - Early hints" +
                                           "\nThe server requested that the requester start preloading resources. Or something else.",
                                    #endregion

                                    // Here you go
                                    #region 200  OK
                                    200 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 200 - OK" +
                                           "\nThe request succeeded. Why are you here?",
                                    #endregion

                                    #region 201 Created
                                    201 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 201 - Created" +
                                           "\nThe request has succeeded, and a resource was created from it.",
                                    #endregion

                                    #region 202 Accepted
                                    202 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 202 - Accepted" +
                                           "\nThe request was accepted for processing, but it has not started or completed.",
                                    #endregion

                                    #region 203 Non-authoritative information
                                    203 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 203 - Non-authoritative information" +
                                           "\nThe request succeeded, but the payload was modified by a transformative proxy.",
                                    #endregion

                                    #region 204 No content
                                    204 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 204 - No content" +
                                           "\nThe request has succeeded, but no redirect is required.",
                                    #endregion

                                    #region 205 Reset content
                                    205 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 205 - Reset content" +
                                           "\nThe resource requested that the content be refreshed.",
                                    #endregion

                                    #region 206 Partial content
                                    206 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 206 - Partial content" +
                                           "\nThe request succeeded to the range requested.",
                                    #endregion

                                    // Go away
                                    #region 300 Multiple choices
                                    300 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 300 - Multiple choice" +
                                           "\nThe requested resource has more than one possible response.",
                                    #endregion

                                    #region 301 Moved / Moved permanently
                                    301 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 301 - Moved / Moved permanently" +
                                           "\nThe requested information has been moved to the URI specified in the Location header.",
                                    #endregion

                                    #region 302 Found
                                    302 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 302 - Found" +
                                           "\nThe requested resource was moved to another area.",
                                    #endregion

                                    #region 303 See other
                                    303 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 303 - See other" +
                                           "\nThe accessed resource doesn't link to the newly uploaded resource, but it links elsewhere.",
                                    #endregion

                                    #region 304 Not modified
                                    304 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 304 - Not modified" +
                                           "\nThe requested resource has not been modified since the last time it was accessed, due to a \"If-Modified-Since\" or \"If-None-Match\" header.",
                                    #endregion

                                    #region 307 Temporary redirect
                                    307 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 307 - Temporary redirect" +
                                           "\nThe requested resource has temporarily moved to a different area.",
                                    #endregion

                                    #region 308 Permanent redirect
                                    308 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 308 - Permanent redirect" +
                                           "\nThe requested resource has been moved to a different area.",
                                    #endregion

                                    // You fucked up
                                    #region 400 Bad request
                                    400 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 400 - Bad request" +
                                           "\nThe request could not be understood by the server.",
                                    #endregion

                                    #region 401 Unauthorized
                                    401 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 401 - Unauthorized" +
                                           "\nThe requested resource requires authentication.",
                                    #endregion

                                    #region 402 Payment required
                                    402 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 402 - Payment required" +
                                           "\nPayment is required to view this content.\nThis status code isn't natively used.",
                                    #endregion

                                    #region 403 Forbidden
                                    403 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 403 - Forbidden" +
                                           "\nYou do not have permission to view this file.",
                                    #endregion

                                    #region 404 Not found
                                    404 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 404 - Not found" +
                                           "\nThe file does not exist on the server.",
                                    #endregion

                                    #region 405 Method not allowed
                                    405 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 405 - Method not allowed" +
                                           "\nThe request method (GET) is not allowed on the requested resource.",
                                    #endregion

                                    #region 406 Not acceptable
                                    406 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 406 - Not acceptable" +
                                           "\nThe client has indicated with Accept headers that it will not accept any of the available representations from the resource.",
                                    #endregion

                                    #region 407 Proxy authentication required
                                    407 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 407 - Proxy authentication required" +
                                           "\nThe requested proxy requires authentication.",
                                    #endregion

                                    #region 408 Request timeout
                                    408 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 408 - Request timeout" +
                                           "\nThe client did not send a request within the time the server was expection the request.",
                                    #endregion

                                    #region 409 Conflict
                                    409 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 409 - Conflict" +
                                           "\nThe request could not be carried out because of a conflict on the server.",
                                    #endregion

                                    #region 410 Gone
                                    410 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 410 - Gone" +
                                           "\nThe requested resource is no longer available.",
                                    #endregion

                                    #region 411 Length required
                                    411 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 410 - Length required" +
                                           "\nThe required Content-length header is missing.",
                                    #endregion

                                    #region 412 Precondition failed
                                    412 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 412 - Precondition failed" +
                                           "\nThe server refused to process the request because an unmatched conditional header is present in the headers.",
                                    //"\nA condition set for this request failed, and the request cannot be carried out.",
                                    #endregion

                                    #region 413 Request entity too large
                                    413 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 413 - Request entity too large" +
                                           "\nThe request is too large for the server to process.",
                                    #endregion

                                    #region 414 Request uri too long
                                    414 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 414 - Request uri too long" +
                                           "\nThe uri is too long.",
                                    #endregion

                                    #region 415 Unsupported media type
                                    415 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 415 - Unsupported media type" +
                                           "\nThe request is an unsupported type.",
                                    #endregion

                                    #region 416 Requested range not satisfiable
                                    416 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 416 - Requested range not satisfiable" +
                                           "\nThe range of data requested from the resource cannot be returned.",
                                    #endregion

                                    #region 417 Expectation failed
                                    417 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 417 - Expectation failed" +
                                           "\nAn expectation given in an Expect header could not be met by the server.",
                                    #endregion

                                    #region 418 I'm a teapot
                                    418 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 418 - I'm a teapot" +
                                           "\nThe server cannot brew coffee at this time.",
                                    #endregion

                                    #region 422 Unprocessable Entity
                                    422 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 422 - Unprocessable entity" +
                                           "\nThe server refused to process the request because it is considered unprocessable.",
                                    #endregion

                                    #region 425 Too early
                                    425 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 425 - Too early" +
                                           "\nThe server refused to process the request due to a replay attack mitigation.",
                                    #endregion

                                    #region 426 Upgrade required
                                    426 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 426 - Upgrade required" +
                                           "\nThe server refused to process the request using your current protocol.",
                                    #endregion

                                    #region 428 Precondition required
                                    428 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 428 - Precondition required" +
                                           "\nThe server refused the connection because there are no required conditional headers.",
                                    #endregion

                                    #region 429 Too many requests
                                    429 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 429 - Too many requests" +
                                           "\nThe server refused to process the request because it was sent too many requests. You may be rate limited.",
                                    #endregion

                                    #region 431 Request header fields too large
                                    431 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 431 - Request header fields too large" +
                                           "\nThe server refused to process the request because one header or all headers are too large.",
                                    #endregion

                                    #region 451 Unavailable for legal reasons
                                    451 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 451 - Unavailable for legal reasons" +
                                           "\nThe server cannot process the request due to a legal reason in your IPs' region.",
                                    #endregion

                                    // I fucked up
                                    #region 500 Internal server error
                                    500 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 500 - Internal server error" +
                                           "\nAn error occured on the server.",
                                    #endregion

                                    #region 501 Not implemented
                                    501 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 501 - Not implemented" +
                                           "\nThe server does not support the requested function.",
                                    #endregion

                                    #region 502 Bad gateway
                                    502 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 502 - Bad gateway" +
                                           "\nThe proxy server recieved a bad response from another proxy or the origin server.",
                                    #endregion

                                    #region 503  Service unavailable
                                    503 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 503 - Service unavailable" +
                                           "\nThe server is temporarily unavailable, likely due to high load or maintenance.",
                                    #endregion

                                    #region 504 Gateway timeout
                                    504 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 504 - Gateway timeout" +
                                           "\nAn intermediate proxy server timed out while waiting for a response from another proxy or the origin server.",
                                    #endregion

                                    #region 505 Http version not supported
                                    505 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 505 - Http version not supported" +
                                           "\nThe requested HTTP version is not supported by the server.",
                                    #endregion

                                    #region  506 Variant also negotiates
                                    506 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 506 - Variant also negotiates" +
                                           "\nAn internal server configuration error in which, who knows, I don't. I'm not well versed in networking.",
                                    #endregion

                                    #region  507 Insufficient storage
                                    507 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 507 - Insufficient storage" +
                                           "\nThe server cannot store the data with the request.",
                                    #endregion

                                    #region  508 Loop detected
                                    508 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 508 - Loop detected" +
                                           "\nThe server encountered an infinite loop, and the operation failed.",
                                    #endregion

                                    #region  510 Not extended
                                    510 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 510 - Not extended" +
                                           "\nThe server... okay i have no idea.",
                                    #endregion

                                    #region  511 Network authentication required
                                    511 => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                           "\n\nThe address returned 511 - Network authentication required" +
                                           "\nThe user must authenticate to gain access to the resources' network.",
                                    #endregion

                                    _ => $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                         "\n\nThe address returned " + WebResponse.StatusCode.ToString() +
                                         "\n" + WebResponse.StatusDescription.ToString()

                                };
                            }
                        }
                        break;
                        #endregion
                        #region ConnectionClosed
                        case WebExceptionStatus.ConnectionClosed: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nConnection closed" +
                                            "\nThe connection was prematurely closed.";
                        }
                        break;
                        #endregion
                        #region TrustFailure
                        case WebExceptionStatus.TrustFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nTrust failure" +
                                            "\nA server certificate could not be validated.";
                        }
                        break;
                        #endregion
                        #region SecureChannelFailure
                        case WebExceptionStatus.SecureChannelFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nSecure channel failure" +
                                            "\nAn error occurred while establishing a connection using SSL.";
                        }
                        break;
                        #endregion
                        #region ServerProtocolViolation
                        case WebExceptionStatus.ServerProtocolViolation: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nServer protocol violation" +
                                            "\nThe server response was not a valid HTTP response.";
                        }
                        break;
                        #endregion
                        #region KeepAliveFailure
                        case WebExceptionStatus.KeepAliveFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nKeep alive failure" +
                                            "\nThe connection for a request that specifies the Keep-alive header was closed unexpectedly.";
                        }
                        break;
                        #endregion
                        #region Pending
                        case WebExceptionStatus.Pending: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nPending" +
                                            "\nAn internal asynchronous request is pending.";
                        }
                        break;
                        #endregion
                        #region Timeout
                        case WebExceptionStatus.Timeout: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nTimeout" +
                                            "\nNo response was received during the time-out period for a request.";
                        }
                        break;
                        #endregion
                        #region ProxyNameResolutionFailure
                        case WebExceptionStatus.ProxyNameResolutionFailure: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nProxy name resolution failure" +
                                            "\nThe name resolver service could not resolve the proxy host name.";
                        }
                        break;
                        #endregion
                        #region UnknownError
                        case WebExceptionStatus.UnknownError: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nUnknown error" +
                                            "\nAn exception of unknown type has occurred.";
                        }
                        break;
                        #endregion
                        #region MessageLengthLimitExceeded
                        case WebExceptionStatus.MessageLengthLimitExceeded: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nMessage length limit exceeded" +
                                            "\nA message was received that exceeded the specified limit when sending a request or receiving a response from the server.";
                        }
                        break;
                        #endregion
                        #region CacheEntryNotFound
                        case WebExceptionStatus.CacheEntryNotFound: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nCache entry not found" +
                                            "\nThe specified cache entry was not found.";
                        }
                        break;
                        #endregion
                        #region RequestProhibitedByCachePolicy
                        case WebExceptionStatus.RequestProhibitedByCachePolicy: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nRequest prohibited by cache policy" +
                                            "\nThe request was not permitted by the cache policy.";
                        }
                        break;
                        #endregion
                        #region RequestProhibitedByProxy
                        case WebExceptionStatus.RequestProhibitedByProxy: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\n\nRequest prohibited by proxy" +
                                            "\nThis request was not permitted by the proxy.";
                        }
                        break;
                        #endregion

                        default: {
                            ExtraMessage = $"A WebException occured{(ExtraData is not null ? $" at {ExtraData}" : "")}." +
                                            "\nNo status was reported, so no parsing was done.";
                        }
                        break;

                    }
                    #endregion

                }
                break;

                default: {
                    Write($"An exception occured.\n\n{ReceivedException}{ExtraData ?? ""}");
                }
                break;
            }

            using forms.frmException ReportException = new(new(ReceivedException) {
                ExtraInfo = ExtraData,
                ExtraMessage = ExtraMessage
            });
            ReportException.ShowDialog();

            //if (SaveExceptionsToFile) {
            //    try {
            //        string FileName = $"\\error_{System.DateTime.Now}.log";
            //        Write($"Saving exception as {FileName}.");
            //        System.IO.File.WriteAllText(FileName, ReceivedException.ToString());
            //    }
            //    catch (System.Exception ex) {
            //        Write(ex.ToString());
            //        using murrty.forms.frmException FileReportException = new(new(ex));
            //        FileReportException.ShowDialog();
            //    }
            //}

            System.GC.Collect();
        }

        #endregion

    }
}
