using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XLib.Network.Http
{
    public class HttpRequest : IHttpRequest
    {
        public void Start()
        {
            
        }

        public void Stop()
        {

        }

        public bool Update()
        {
            return false;
        }
    }

    public class HttpManager : HttpRequestManager<HttpRequest>
    {

    }
}