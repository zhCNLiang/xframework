using System.Collections.Generic;
using UnityEngine;
using XLua;

public class VariableArray : MonoBehaviour
{
    [SerializeField]
    private List<VariableObject> m_VarObjects = new List<VariableObject>();
    private Dictionary<string, Object> m_VarsMap = new Dictionary<string, Object>();
    
    void Awake()
    {
        foreach(var varObj in m_VarObjects)
        {
            m_VarsMap[varObj.Name] = varObj.Obj;
        }
    }

    public void BindToLua(LuaTable luaTable)
    {
        foreach(var varObj in m_VarObjects)
        {
            luaTable.Set<string, Object>(varObj.Name, varObj.Obj);
        }
    }

    public T GetVariable<T>(string name) where T : class
    {
        if (m_VarsMap.TryGetValue(name, out var obj))
        {
            return obj as T;
        }
        return null;
    }
}
