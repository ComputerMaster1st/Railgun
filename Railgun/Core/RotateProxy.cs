using System;
using System.Net;

namespace Railgun.Core
{
    public class RotateProxy : WebProxy
    {
        public RotateProxy()
        {
            UseDefaultCredentials = true;
        }

        public void RotateAddress(string address, int port)
        {
            Address = new Uri($"http://{address}:{port}");
        }
    }
}
