using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HttpRequestManager<T> : MonoSingleton<HttpRequestManager<T>>
    where T : IHttpRequest
{
    private List<IHttpRequest> waitLoad = new List<IHttpRequest>();
    private List<IHttpRequest> loading = new List<IHttpRequest>();

    protected readonly int MAX_LOAD_NUM = 9;

    public void AddRequest(T request)
    {
        waitLoad.Add(request);
    }

    // Update is called once per frame
    protected void Update()
    {
        for(int i = waitLoad.Count - 1; i >= 0; i--)
        {
            if (loading.Count >= MAX_LOAD_NUM) break;
            var request = waitLoad[i];
            request.Start();
            loading.Add(request);
            waitLoad.RemoveAt(i);
        }

        for(int i = loading.Count - 1; i >= 0; i--)
        {
            var request = loading[i];
            if (!request.Update())
            {
                request.Stop();
                loading.RemoveAt(i);
            }
        }
    }
}
