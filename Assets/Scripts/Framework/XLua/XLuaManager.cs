using UnityEngine;
using XLua;
using System.IO;
using XLib;
using System;

public class XLuaManager : MonoSingleton<XLuaManager>
{
    private LuaEnv luaEnv;

    public void InitLuaEnv()
    {
        luaEnv = new LuaEnv();
        if (luaEnv != null)
        {
            luaEnv.AddLoader(LuaFileLoader);
            luaEnv.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
            luaEnv.AddBuildin("lpeg", XLua.LuaDLL.Lua.LoadLpeg);
            luaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadLuaProfobuf);
            luaEnv.AddBuildin("ffi", XLua.LuaDLL.Lua.LoadFFI);
            LoadScript("GameApp");
        }
    }

    public LuaEnv GetEnv()
    {
        return luaEnv;
    }

    public byte[] LuaFileLoader(ref string filepath)
    {
        filepath = filepath.Replace(".", "/") + ".lua";
        var luafilePath = string.Format("{0}/{1}", "Assets/Lua", filepath);
        var request = Assets.LoadAsset(luafilePath, typeof(LuaAsset));
        var luaAsset = request.asset as LuaAsset;
        return luaAsset.data;
    }

    public object[] DoString(string scriptContent)
    {
        if (luaEnv != null)
        {
            try
            {
                return luaEnv.DoString(scriptContent);
            }
            catch(System.Exception exception)
            {
                string msg = string.Format("xlua exception : {0}\n{1}", exception.Message, exception.StackTrace);
                Logger.Error?.Output(msg);
            }
        }
        return null;
    }

    public object[] LoadScript(string scriptName)
    {
        return DoString(string.Format("return require '{0}'", scriptName));
    }
}
