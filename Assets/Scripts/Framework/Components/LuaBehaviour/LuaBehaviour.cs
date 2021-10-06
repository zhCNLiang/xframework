using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLib;
using XLua;
using System;

public class LuaBehaviour : MonoBehaviour
{
    [SerializeField]
    private string luaEntry;

    public LuaTable behavior
    {
        get; private set;
    }

    private VariableArray variableArray;
    private LuaTable variables;

    private Action<LuaTable> awake;
    private Action<LuaTable> start;
    private Action<LuaTable> onEnable;
    private Action update;
    private Action lateUpdate;
    private Action fixedUpdate;
    private Action<LuaTable> onDisable;
    private Action<LuaTable> onDestroy;

    private void Awake()
    {
        TryGetComponent<VariableArray>(out variableArray);

        var rets = XLuaManager.Instance.LoadScript(luaEntry);
        var table = rets[0] as LuaTable;

        var env = XLuaManager.Instance.GetEnv();
        variables = env.NewTable();
        table.Get<string, Func<LuaBehaviour, LuaTable, LuaTable>>("New", out var @new);
        behavior = @new.Invoke(this, variables);

        behavior.Get<string, Action<LuaTable>>("Awake", out awake);
        behavior.Get<string, Action<LuaTable>>("OnEnable", out onEnable);
        behavior.Get<string, Action<LuaTable>>("Start", out start);
        behavior.Get<string, Action<LuaTable>>("FixedUpdate", out var _fixedUpdate);
        behavior.Get<string, Action<LuaTable>>("Update", out var _update);
        behavior.Get<string, Action<LuaTable>>("LateUpdate", out var _lateUpdate);
        behavior.Get<string, Action<LuaTable>>("OnDisable", out onDisable);
        behavior.Get<string, Action<LuaTable>>("OnDestroy", out onDestroy);

        if (_fixedUpdate != null)
            fixedUpdate = () => _fixedUpdate?.Invoke(behavior);

        if (_update != null)
            update = () => _update?.Invoke(behavior);

        if (_lateUpdate != null)
            lateUpdate = () => _lateUpdate?.Invoke(behavior);

        variableArray?.BindToLua(variables);
        awake?.Invoke(behavior);
    }

    private void RegisterUpdates()
    {
        if (fixedUpdate != null)
        {
            LuaUpdate.Instance.AddFixedUpdate(fixedUpdate);
        }

        if (update != null)
        {
            LuaUpdate.Instance.AddUpdate(update);
        }

        if (lateUpdate != null)
        {
            LuaUpdate.Instance.AddLateUpdate(lateUpdate);
        }
    }

    private void UnRegisterUpdates()
    {
        if (fixedUpdate != null)
        {
            LuaUpdate.Instance.RemoveFixedUpdate(fixedUpdate);
        }

        if (update != null)
        {
            LuaUpdate.Instance.RemoveUpdate(update);
        }

        if (lateUpdate != null)
        {
            LuaUpdate.Instance.RemoveLateUpdate(lateUpdate);
        }
    }

    private void Start()
    {
        start?.Invoke(behavior);
    }

    private void OnEnable()
    {
        RegisterUpdates();
        onEnable?.Invoke(behavior);
    }

    private void OnDisable()
    {
        UnRegisterUpdates();
        onDisable?.Invoke(behavior);
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke(behavior);
        behavior?.Dispose();
        variables?.Dispose();

        awake = null;
        start = null;
        onEnable = null;
        update = null;
        lateUpdate = null;
        fixedUpdate = null;
        onDisable = null;
        onDestroy = null;
    }
}
