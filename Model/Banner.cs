using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Budget
{
    public class Banner
    {
        public enum BannerType
        {
            Picture = 0,
            Text,
            Rating,
            None
        }

        private int _id;
        private BannerType _type;
        private string _content;
        private string _link;
        private string _url;

        public Banner()
        {
            _id = 0;
            _type = BannerType.None;
            _content = String.Empty;
            _link = String.Empty;
            _url = String.Empty;
        }

        public Banner(int id, BannerType type, string content, string link, string url)
        {
            _type = type;
            _content = content;
            _link = link;
            _url = url;
        }

        public Banner(DataRow row) : base()
        {
            try { _id = Convert.ToInt32(row["ID"]); } catch {}
            var isPict = false;
            try { isPict = Convert.ToBoolean(row["IsPicture"]); } catch {}
            _type = isPict ? Banner.BannerType.Picture : Banner.BannerType.Text;
            _content = row["Content"].ToString();
            _link = row["Link"].ToString();
            _url = row["Url"].ToString();
        }

        public override bool Equals(object obj)
        {
            var b1 = this;

            if (obj is Banner)
            {
                var b2 = obj as Banner;

                if (b1.Type == b2.Type && b1.Content == b2.Content && b1.Link == b2.Link && b1.Url == b2.Url)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int ID
        {
            get { return _id; }
        }

        public string Content
        {
            get { return _content; }
        }

        public BannerType Type
        {
            get { return _type; }
        }

        public string Link
        {
            get { return _link; }
        }

        public string Url
        {
            get { return _url; }
        }
    }
}
