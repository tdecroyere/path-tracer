import Cocoa

class ImageSurfaceView : NSView {
    @available(*, unavailable) public required init?(coder: NSCoder) { fatalError() }

    public var caLayer: CALayer

    public override init(frame: CGRect) {
        self.caLayer = CALayer()
        self.caLayer.isOpaque = true
        super.init(frame: frame)
        self.wantsLayer = true
        self.layerContentsRedrawPolicy = .onSetNeedsDisplay
    }

    override var wantsUpdateLayer: Bool {
        return true
    }
    
    public override func makeBackingLayer() -> CALayer {
        return self.caLayer
    }

    public override func updateLayer() {
    }
}