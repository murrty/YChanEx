using System.Text.RegularExpressions;

namespace YChanEx {
    internal class SevenChanThread {

        public static string TranslateMessage(string Message, ThreadInfo CurThr) {

            if (!string.IsNullOrWhiteSpace(Message)) {

                while (Message.EndsWith("<br />")) {
                    Message = Message[..^6];
                }

                Regex Reply = new($"\\<a href=\"\\/{CurThr.Data.ThreadBoard}\\/res\\/{CurThr.Data.ThreadID}.html#([0-9]+)\" class=\"ref\\|{CurThr.Data.ThreadBoard}\\|{CurThr.Data.ThreadID}\\|([0-9]+)\"\\>");

                MatchCollection Matches = Reply.Matches(Message);

                if (Matches.Count > 0) {
                    for (int i = 0; i < Matches.Count; i++) {
                        Message = Message
                            .Replace(Matches[i].Value, $"<a href=\"#p{Matches[i].Value[(Matches[i].Value.LastIndexOf("|") + 1)..Matches[i].Value.LastIndexOf("\"")]}\">");
                    }
                }

                //Regex Quotes = new(">>([0-9]+)");
                //MatchCollection Matches = Quotes.Matches(Message);

                //if (Matches.Count > 0) {
                //    for (int i = 0; i < Matches.Count; i++) {
                //        Message = Message.Replace(Matches[i].Value, $"<a href=\"#p{Matches[i].Value[2..]}\">{Matches[i].Value}</a><br />");
                //    }
                //}

            }
            
            return Message;

        }

    }
}
