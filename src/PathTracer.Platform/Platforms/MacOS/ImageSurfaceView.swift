import Cocoa

class ImageSurfaceView : NSView {
    @available(*, unavailable) public required init?(coder: NSCoder) { fatalError() }

    private var image: CGImage?
    //private var layerDelegate: ImageSurfaceViewDelegate

    public override init(frame: CGRect) {
        //self.layerDelegate = ImageSurfaceViewDelegate()
        super.init(frame: frame)
        self.wantsLayer = true
        self.layerContentsRedrawPolicy = .onSetNeedsDisplay
        self.layer!.isOpaque = true
        self.layer!.magnificationFilter = .nearest
        //self.layer!.delegate = self.layerDelegate 
    }

    override var wantsUpdateLayer: Bool {
        return true
    }

    public func setImage(_ image: CGImage) {
        self.image = image
    }

    override func mouseUp(with event: NSEvent) { print("mouseUp")
    //self.textField2.window?.makeFirstResponder(nil) 
    }
    
    /*public override func makeBackingLayer() -> CALayer {
        print("create layer")
        let caLayer = CALayer()
        caLayer.isOpaque = true
        caLayer.backgroundColor = CGColor(red: 1.0, green: 1.0, blue: 0.0, alpha: 1.0)
        caLayer.delegate = ImageSurfaceViewDelegate() 

        print("\(self.canDrawSubviewsIntoLayer)");

        return caLayer
    }*/

    public override func updateLayer() {
        if (self.image != nil) {
            self.layer!.contents = self.image!
        }
    }
}
/*
class ImageSurfaceViewDelegate : NSObject, CALayerDelegate {
    func draw(_ layer: CALayer, in ctx: CGContext) {
        print("ok")
        /*UIGraphicsPushContext(ctx)
        //[[UIImage imageNamed: @"smiley"] drawInRect:CGContextGetClipBoundingBox(ctx)];
        UIImage(named:"smiley")!.draw(at: .zero)
        UIGraphicsPopContext()
        print("\(#function)")
        print(layer.contentsGravity)*/
    }
}*/