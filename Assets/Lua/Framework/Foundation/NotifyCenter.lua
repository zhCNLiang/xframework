NotifyCenter = {}

local tNotifications = {}

function NotifyCenter.AddHandler(subscribe, obj, method)
    assert(type(subscribe) == "string", "subscribe must be a string value")
    tNotifications[subscribe] = tNotifications[subscribe] or {}
    local listeners = tNotifications[subscribe]

    for _, v in ipairs(listeners) do
        if v.obj == obj then
            Log.Warning("notify center is already add this object handler")
            return
        end
    end

    table.insert(listeners, {
        obj = obj,
        method = method
    })
end

function NotifyCenter.RemoveHandler(subscribe, obj)
    tNotifications[subscribe] = tNotifications[subscribe] or {}
    local listeners = tNotifications[subscribe]
    for i, v in ipairs(listeners) do
        if v.obj == obj then
            table.remove(listeners, i)
            break
        end
    end
end

function NotifyCenter.Broadcast(subscribe, ...)
    tNotifications[subscribe] = tNotifications[subscribe] or {}
    local listeners = tNotifications[subscribe]
    for _, v in ipairs(listeners) do
        local obj = v.obj
        local method = v.method
        method(obj, ...)
    end
end