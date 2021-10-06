using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[RequireComponent(typeof(VariableArray))]
public class ListItem : MonoBehaviour
{
    public int index { get; set; }
    public LuaTable elements { get; private set; }
    private VariableArray variables;
    private void Awake()
    {
        var env = XLuaManager.Instance.GetEnv();
        elements = env.NewTable();

        variables = GetComponent<VariableArray>();
        variables.BindToLua(elements);
    }

    private void OnDestroy()
    {
        elements?.Dispose();
    }
}
