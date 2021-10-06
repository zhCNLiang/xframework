--[[
    序列化table
]]
local serialize

serialize = function(input, output, level)
    assert(type(input) == "table", "input value must be table.")

    output = output or {}
    level = level or 0
    table.insert(output, "{\n")

    local isEmpty = true
    for k, v in pairs(input) do
        if type(k) == "number" then
            table.insert(output, string.rep("\t", level + 1) ..string.format("[%s]", k))
        else
            table.insert(output, string.rep("\t", level + 1) ..tostring(k))
        end

        table.insert(output, " = ")

        if type(v) == "table" then
            serialize(v, output, level + 1)
        elseif type(v) == "number" or type(v) == "boolean" then
            table.insert(output, string.format("%s", v))
        else
            table.insert(output, string.format("\"%s\"", tostring(v)))
        end

        table.insert(output, ",\n")
        isEmpty = false
    end

    if not isEmpty then
        output[#output] = "\n"
    end

    table.insert(output, string.rep("\t", level) .."}")

    if level == 0 then
        local str = table.concat(output, "")
        return str
    end
end

table.serialize = serialize

--[[
    只读表
]]
local readonly

readonly = function(tb)
    local mt = {
        __index = function(t, k)
            local ret = rawget(tb, k)
            if type(ret) == "table" then
                ret = readonly(ret)
                rawset(t, k, ret)
            end
            return ret
        end,
        __newindex = function(_, k, _)
            assert(false, string.format("can not modify or add key = %s", tostring(k)))
        end
    }
    return setmetatable({}, mt)
end

table.readonly = readonly

--[[
    获取table字典的键
]]

function table.keys(t)
    local ret = {}
    for k, _ in pairs(t) do
        table.insert(ret, k)
    end
    return ret
end

--[[
    获取table字典的值
]]

function table.values(t)
    local ret = {}
    for _, v in pairs(t) do
        table.insert(ret, v)
    end
    return ret
end

--[[
    查询table数据的值所在索引
]]
function table.indexof(t, value)
    for i, v in ipairs(t) do
        if v == value then
            return i
        end
    end
end

-- 获取table的key值
function table.get(t, key, default)
    return t[key] or default
end

-- 移除字典类型table里的某个值
function table.removeByValue(t, value)
    for i, v in pairs(t) do
        if v == value then
            t[i] = nil
            break
        end
    end
end

-- 遍历table array/map
function table.walkmap(t, fn)
    for k, v in pairs(t) do
        local _break = fn(k, v)
        if _break then
            break
        end
    end
end

function table.walkarray(t, fn)
    for i=1, #t do
        local del, br = fn(i, t[i])
        if del then
            table.remove(t, i)
            i = i - 1
        end

        if br then
            break
        end
    end
end