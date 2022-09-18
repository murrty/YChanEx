namespace YChanEx {
    public class FourChanThread : ThreadBase {

        public Post[] posts { get; set; }

        public sealed class Post {
            public int no { get; set; }
            //public string now { get; set; }
            public string name { get; set; }
            public string com { get; set; }
            public string filename { get; set; }
            public string sub { get; set; }
            public string ext { get; set; }
            public int? w { get; set; }
            public int? h { get; set; }
            public int? tn_w { get; set; }
            public int? tn_h { get; set; }
            public long? tim { get; set; }
            public double time { get; set; }
            public string md5 { get; set; }
            public long? fsize { get; set; }
            public int? spoiler { get; set; }
            //public int? resto { get; set; }
            public string id { get; set; }
            public string capcode { get; set; }
            public string trip { get; set; }
            //public int? bumplimit { get; set; }
            //public int? imagelimit { get; set; }
            //public string semantic_url { get; set; }
            //public int? custom_spoiler { get; set; }
            //public int? replies { get; set; }
            //public int? images { get; set; }
            //public int? unique_ips { get; set; }
            //public int? sticky { get; set; }
            //public int? closed { get; set; }
            public bool? archived { get; set; }
            //public int? filedeleted { get; set; }
        }


    }
}
