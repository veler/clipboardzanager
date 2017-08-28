using System;
using System.Net;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Represents a <see cref="System.Net.WebClient"/> with a time out
    /// </summary>
    internal class WebClient : System.Net.WebClient
    {
        internal int Timeout { get; set; }

        public WebClient()
        {
            Timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var lWebRequest = base.GetWebRequest(uri);
            if (lWebRequest != null)
            {
                lWebRequest.Timeout = Timeout;
                ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
                return lWebRequest;
            }

            return null;
        }
    }
}
