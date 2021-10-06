using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace XLib.Network.Http
{
    public class UploadRequest : IHttpRequest
    {
        private UnityWebRequest www { get; set; }
        public string url { get; private set; }
        public string error { get; private set; }
        public bool isDone { get; private set; }

        public UploadRequest(string url)
        {

        }

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

    public class UploadManager : HttpRequestManager<UploadRequest>
    {

    }
}