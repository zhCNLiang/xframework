UIPanelManager = {}

local tPanels = {}
local tWaitOpenQuene = {}
local loadingloader = nil
local loadingUIDef = nil
local uiRootCanvas = nil
local tLayerCanvas = {}

function UIPanelManager.OpenUI(uiDef, ...)
    local view = tPanels[uiDef]
    if view then
        view:OnOpen(...)
        view:Show()
        view:OnDoAnimationShow(Handler(view, view.OnViewShow))
        return
    end

    if #tWaitOpenQuene > 0 then
        table.insert(tWaitOpenQuene, {
            Def = uiDef,
            Args = {...}
        })
        return
    end

    local assetPath = uiDef.AssetPath
    local layer = uiDef.Layer
    local args = {...}

    loadingUIDef = uiDef
    local layerCanvas = UIPanelManager.GetLayer(layer)
    loadingloader = AssetsUtility.LoadPrefabAsync(assetPath, function(go)
        loadingUIDef = nil
        loadingloader = nil

        local name = go.name
        go = GameObject.Instantiate(go)
        go.name = name

        local flag, panel = go:TryGetComponent(typeof(CS.UIPanel))
        assert(flag, "can not find lua behaviour component")

        view = panel.behavior
        tPanels[uiDef] = view

        layerCanvas.transform:AddChild(view.transform)
        view.transform:ResetRTS()
        view.transform:SetAnchoredMin(0, 0)
        view.transform:SetAnchoredMax(1, 1)
        view.transform:SetOffsetMin(0, 0)
        view.transform:SetOffsetMax(0, 0)

        view:OnOpen(table.unpack(args))
        view:Show()
        view:OnDoAnimationShow(Handler(view, view.OnViewShow))

        local nextUI = table.remove(tWaitOpenQuene, 1)
        if nextUI then
            UIPanelManager.OpenUI(nextUI.Def, table.unpack(nextUI.Args))
        end
    end)
end

local function IfLoadingStopAndOpenNextUI(uiDef)
    if loadingUIDef == uiDef then
        loadingUIDef = nil
        if loadingloader then
            loadingloader:Stop()
            loadingloader = nil

            local nextUI = table.remove(tWaitOpenQuene, 1)
            if nextUI then
                UIPanelManager.OpenUI(nextUI.Def, table.unpack(nextUI.Args))
            end
        end
    end
end

function UIPanelManager.CloseUI(uiDef, bNoAnimation)
    IfLoadingStopAndOpenNextUI(uiDef)

    local view = tPanels[uiDef]
    if not view then
        return
    end

    if not bNoAnimation then
        view:OnDoAnimationHide(Handler(view, view.OnViewHide))
    else
        view:Hide()
    end
end

function UIPanelManager.DestroyUI(uiDef)
    IfLoadingStopAndOpenNextUI(uiDef)

    local view = tPanels[uiDef]
    if view then
        GameObject.Destroy(view.gameObject)
    end
    tPanels[uiDef] = nil
end

function UIPanelManager.GetRootCanvas()
    if not uiRootCanvas then
        local loader = AssetsUtility.LoadPrefab("Assets/Art/UI/Prefab/UIRootCanvas.prefab")
        local go = GameObject.Instantiate(loader.request.asset)
        GameObject.DontDestroyOnLoad(go)
        local _, component = go:TryGetComponent(typeof(Canvas))
        uiRootCanvas = component
        uiRootCanvas.gameObject.name = loader.request.asset.name
    end
    return uiRootCanvas
end

function UIPanelManager.GetLayer(layer)
    local canvas = tLayerCanvas[layer]
    if not canvas then
        local rootCanvas = UIPanelManager.GetRootCanvas()
        local go = GameObject(layer)
        go:AddComponent(typeof(RectTransform))
        GameObject.DontDestroyOnLoad(go)

        local transform = go.transform
        rootCanvas.transform:AddChild(transform)
        transform:ResetRTS()
        transform:SetAnchoredMin(0, 0)
        transform:SetAnchoredMax(1, 1)
        transform:SetOffsetMin(0, 0)
        transform:SetOffsetMax(0, 0)

        go:AddComponent(typeof(GraphicRaycaster))
        canvas = go:GetComponent(typeof(Canvas))
        canvas.overrideSorting = true
        canvas.sortingOrder = UILayers.SortOrder[layer]
        tLayerCanvas[layer] = canvas
    end
    return canvas
end

return UIPanelManager