namespace YChanEx {
    public class EightKunThread : ThreadBase {

        public Post[] posts { get; set; }

        public class Post {
            public int no { get; set; }
            public string sub { get; set; }
            public string com { get; set; }
            public string name { get; set; }
            public long time { get; set; }
            //public int? omitted_posts { get; set; }
            //public int? omitted_images { get; set; }
            //public int sticky { get; set; }
            //public int locked { get; set; }
            //public string cyclical { get; set; }
            //public string bumplocked { get; set; }
            //public int last_modified { get; set; }
            //public string id { get; set; }
            //public bool? tx_link { get; set; }
            public int? tn_h { get; set; }
            public int? tn_w { get; set; }
            public int? h { get; set; }
            public int? w { get; set; }
            public int? fsize { get; set; }
            public string filename { get; set; }
            public string ext { get; set; }
            public string tim { get; set; }
            public int? fpath { get; set; }
            public int? spoiler { get; set; }
            public string md5 { get; set; }
            //public int resto { get; set; }
            public ExtraFile[] extra_files { get; set; }
        }

        public class ExtraFile {
            public int tn_h { get; set; }
            public int tn_w { get; set; }
            public int h { get; set; }
            public int w { get; set; }
            public int fsize { get; set; }
            public string filename { get; set; }
            public string ext { get; set; }
            public string tim { get; set; }
            public int fpath { get; set; }
            public int spoiler { get; set; }
            public string md5 { get; set; }
        }

    }

    public class EightKunBoards {

        public class Board {
            public string uri { get; init; }
            public string title { get; init; }
            public string subtitle { get; init; }
            //public string indexed { get; init; }
            //public string sfw { get; init; }
            //public string posts_total { get; init; }
            //public string time { get; init; }
            //public int weight { get; init; }
            //public string locale { get; init; }
            //public string[] tags { get; init; }
            //public string max { get; init; }
            //public int active { get; init; }
            //public int pph { get; init; }
            //public int ppd { get; init; }
            //public float pph_average { get; init; }
        }

        public static Board[] GetBoards() {
            using murrty.classcontrols.ExtendedWebClient wc = new();
            wc.Method = murrty.classcontrols.HttpMethod.GET;
            wc.UserAgent = Config.Settings.Advanced.UserAgent;
            try {
                return wc.DownloadString("https://8kun.top/boards.json").JsonDeserialize<Board[]>();
            }
            catch { return null; }
        }

    }
}
