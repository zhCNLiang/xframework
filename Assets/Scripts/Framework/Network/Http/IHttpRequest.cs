using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface IHttpRequest
{
    void Start();
    void Stop();
    bool Update();
}
