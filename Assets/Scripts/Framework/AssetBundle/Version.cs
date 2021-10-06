using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Version
{
    [Serializable]
    public struct Manifest
    {
        public string name;
        public long len;
        public string hash;
    }

    public float appVersion;
    public int resVersion;

    public Manifest manifest;
}
