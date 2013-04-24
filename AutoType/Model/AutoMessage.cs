using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoType.Model
{
    class AutoMessage
    {
        private string _code;
        private string _content;
        
        public string Code
        {
            get { return this._code;}
            set { this._code = value.ToLower().Trim(); }
        }
        public string Content
        {
            get { return this._content; }
            set { this._content = value.Trim(); }
        }

        public AutoMessage(string pCode,string pContent) 
        {
            this.Code = pCode;
            this.Content = pContent;
        }

        public override string ToString()
        {
            this.Content = this.Content.Replace("[[date]]", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

            return this.Content;
        }
    }
}
