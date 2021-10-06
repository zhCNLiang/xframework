UIPanel = Class(LuaBehaviour)

function UIPanel:Awake()
    local _, viewCanvas = self.transform:TryGetComponent(typeof(Canvas))
    self.viewCanvas = viewCanvas

    local _, viewCanvasGroup = self.transform:TryGetComponent(typeof(CanvasGroup))
    self.viewCanvasGroup = viewCanvasGroup

    local _, viewGraphicRaycaster = self.transform:TryGetComponent(typeof(GraphicRaycaster))
    self.viewGraphicRaycaster = viewGraphicRaycaster
end

function UIPanel:OnOpen(...)

end

function UIPanel:Show()
    self.viewCanvas.enabled = true
end

function UIPanel:Hide()
    self.viewCanvas.enabled = false
end

function UIPanel:IsVisible()
    return self.viewCanvas.enabled
end

function UIPanel:OnDoAnimationShow(onViewShow)
    onViewShow()
end

function UIPanel:OnViewShow()

end

function UIPanel:OnDoAnimationHide(onViewHide)
    onViewHide()
end

function UIPanel:OnViewHide()
    self:Hide()
end

function UIPanel:OnDestroy()
    self.viewCanvas = nil
    self.viewCanvasGroup = nil
    self.viewGraphicRaycaster = nil
end

return UIPanel