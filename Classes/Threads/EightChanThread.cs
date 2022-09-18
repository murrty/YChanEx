namespace YChanEx {
    public class EightChanThread : ThreadBase {
        //public object signedRole { get; init; }
        //public object id { get; init; }
        public string name { get; init; }
        //public string email { get; init; }
        //public string boardUri { get; init; }
        public int threadId { get; init; }
        public string subject { get; init; }
        public string markdown { get; init; }
        public string message { get; init; }
        public string creation { get; init; }
        //public bool locked { get; init; }
        public bool? archived { get; init; }
        //public bool pinned { get; init; }
        //public bool cyclic { get; init; }
        //public bool autoSage { get; init; }
        public File[] files { get; init; }
        public Post[] posts { get; init; }
        //public int uniquePosters { get; init; }
        //public int maxMessageLength { get; init; }
        //public bool usesCustomCss { get; init; }
        //public int wssPort { get; init; }
        //public int wsPort { get; init; }
        //public bool usesCustomJs { get; init; }
        public string boardName { get; init; }
        public string boardDescription { get; init; }
        //public object boardMarkdown { get; init; }
        //public int maxFileCount { get; init; }
        //public string maxFileSize { get; init; }
        public class File {
            public string originalName { get; init; }
            public string path { get; init; }
            public string thumb { get; init; }
            //public string mime { get; set; }
            public long size { get; init; }
            public int width { get; init; }
            public int height { get; init; }
        }
        public class Post {
            public string name { get; init; }
            //public object signedRole { get; init; }
            //public string email { get; init; }
            //public object id { get; init; }
            public string subject { get; init; }
            public string markdown { get; init; }
            public string message { get; init; }
            public int postId { get; init; }
            public string creation { get; init; }
            public File[] files { get; init; }
        }

        public static DateTime GetDateTime(string Date) {
            if (System.Text.RegularExpressions.Regex.IsMatch(Date, "^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{3}Z$")) {
                DateTime Parsed = DateTime.ParseExact(Date, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture);
                return Parsed;
            }
            throw new InvalidOperationException("Invalid date!");
        }

        public static string CleanseMessage(string Input, ThreadInfo Thread) {
            if (Input == null)
                return string.Empty;

            Input = Input.Replace("\n", "<br />");
            if (Input.IndexOf($"<a class=\"quoteLink\" href=\"/{Thread.Data.ThreadBoard}/res/{Thread.Data.ThreadID}.html#") > -1) {
                Input = Input.Replace(
                    $"<a class=\"quoteLink\" href=\"/{Thread.Data.ThreadBoard}/res/{Thread.Data.ThreadID}.html#",
                    "<a class=\"quoteLink\" href=\"#p");
            }

            return Input;
        }

    }
}