using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget
{
    public class Hint
    {
        private int _id;
        private string _header;
        private string _content;
        private string _alias;

        public Hint()
        {
            _id = -1;
            _header = String.Empty;
            _content = String.Empty;
            _alias = String.Empty;
        }

        public Hint(string id, string header, string content, string alias)
        {
            _id = default(int);
            Int32.TryParse(id, out _id);

            _header = header;
            _content = content;
            _alias = alias;
        }

        public override bool Equals(object obj)
        {
            var h1 = this;

            if (obj is Hint)
            {
                var h2 = obj as Hint;

                if (h1.ID == h2.ID && h1.Header == h2.Header && h1.Content == h2.Content && h1.Alias == h2.Alias)
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

        public string Header
        {
            get { return _header; }
        }

        public string Content
        {
            get { return _content; }
        }

        public string Alias
        {
            get { return _alias; }
        }
    }
}
