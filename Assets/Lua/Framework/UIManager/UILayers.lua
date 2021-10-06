UILayers = {}

UILayers = {
    BackGroundLayer = "BackGroundLayer",
    FixedLayer      = "FixedLayer",
    PageLayer       = "PageLayer",
    FloatLayer      = "FloatLayer",
    PopLayer        = "PopLayer",
    MsgLayer        = "MsgLayer",
    TipLayer        = "TipLayer",
    LoadingLayer    = "LoadingLayer",
    SysLayer        = "SysLayer"
}

-- 层级显示层级
UILayers.SortOrder = {
    [UILayers.BackGroundLayer]       = 100,
    [UILayers.FixedLayer]            = 200,
    [UILayers.PageLayer]             = 300,
    [UILayers.FloatLayer]            = 350,
    [UILayers.PopLayer]              = 400,
    [UILayers.MsgLayer]              = 500,
    [UILayers.TipLayer]              = 600,
    [UILayers.LoadingLayer]          = 700,
    [UILayers.SysLayer]              = 800
}

-- 同层级打开是，对前一个Panel的操作
UILayers.Opt = {
    None = 1,   -- 不做处理，即叠加显示
    Hide = 2,   -- 隐藏前一个，关闭顶部Panel会打开前一个Panel
    Close = 3   -- 关闭前一个，这个层只会存活顶部（最后打卡的）Panel
}

UILayers.OptAction = {
    [UILayers.BackGroundLayer]   = UILayers.Opt.Close,
    [UILayers.FixedLayer]        = UILayers.Opt.Close,
    [UILayers.PageLayer]         = UILayers.Opt.Hide,
    [UILayers.FloatLayer]         = UILayers.Opt.None,
    [UILayers.PopLayer]          = UILayers.Opt.None,
    [UILayers.MsgLayer]          = UILayers.Opt.Close,
    [UILayers.TipLayer]          = UILayers.Opt.None,
    [UILayers.LoadingLayer]      = UILayers.Opt.Close,
    [UILayers.SysLayer]          = UILayers.Opt.Close
}

return UILayers