Log = {}

Log.Level = CS.Logger.LoggerLevel

local trace, info, warning, error

local function loginfo()
    local i = debug.getinfo(4, "Sl")
    return string.format("%s line:%d\n", i.source, i.linedefined)
end

local function SetLogger()
    trace = CS.Logger.Trace
    info = CS.Logger.Info
    warning = CS.Logger.Warning
    error = CS.Logger.Error
end

function Log.SetLevel(level)
    logLevel = level
    CS.Logger.Level = logLevel

    SetLogger()
end

local function innerOutput(logger, fmt, ...)
    local cnt = select('#', ...)
    local msg = cnt > 0 and string.format(fmt, ...) or fmt
    logger:Output(loginfo() ..msg, false)
end

function Log.Trace(fmt, ...)
    if trace then
        innerOutput(trace, fmt, ...)
    end
end

function Log.Info(fmt, ...)
    if info then
        innerOutput(info, fmt, ...)
    end
end

function Log.Warning(fmt, ...)
    if warning then
        innerOutput(warning, fmt, ...)
    end
end

function Log.Error(fmt, ...)
    if error then
        innerOutput(error, fmt, ...)
    end
end

SetLogger()