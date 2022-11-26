import Cocoa
import NativeUIServiceModule

class NativeWindow {
    let window: NSWindow

    init(_ window: NSWindow) {
        self.window = window
    }
}

class NativePanel {
    let view: NSView

    init(_ view: NSView) {
        self.view = view
    }
}

class NativeControl {
    let control: NSControl

    init(_ control: NSControl) {
        self.control = control
    }
}

class NativeImageSurface {
    let imageSurfaceView: ImageSurfaceView
    let window: NSWindow
    let width: Int
    let height: Int

    init(_ imageSurfaceView: ImageSurfaceView, window: NSWindow, width: Int, height: Int) {
        self.imageSurfaceView = imageSurfaceView
        self.window = window
        self.width = width
        self.height = height
    }
}

var globalWindow: NativeWindow?

@_cdecl("PT_CreateWindow")
public func createWindow(application: UnsafeRawPointer, title: UnsafeMutablePointer<Int8>, width: Int, height: Int, windowState: NativeWindowState) -> UnsafeMutableRawPointer {
    let window = NSWindow(contentRect: NSMakeRect(0, 0, CGFloat(width), CGFloat(height)), 
                            styleMask: [.resizable, .titled, .miniaturizable, .closable], 
                            backing: .buffered, 
                            defer: false)

    window.title = String(cString: title);

    window.center()
    window.makeKeyAndOrderFront(nil)

    if (windowState == Maximized) {
        window.setFrame(window.screen!.visibleFrame, display: true, animate: false)
    }

    let nativeWindow = NativeWindow(window)
    globalWindow = nativeWindow
    return Unmanaged.passRetained(nativeWindow).toOpaque()
}

@_cdecl("PT_GetWindowRenderSize")
public func getWindowRenderSize(window: UnsafeRawPointer) -> NativeWindowSize {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()

    let contentView = nativeWindow.window.contentView! as NSView
    let mainScreenScaling = nativeWindow.window.screen!.backingScaleFactor

    var size = contentView.frame.size
    size.width *= mainScreenScaling;
    size.height *= mainScreenScaling;

    return NativeWindowSize(Width: Int32(size.width), Height: Int32(size.height), UIScale: Float(mainScreenScaling))
}

@_cdecl("PT_GetWindowSystemHandle")
public func getWindowSystemHandle(window: UnsafeRawPointer) -> UnsafeMutableRawPointer {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()
    return Unmanaged.passRetained(nativeWindow.window).toOpaque()
}

@_cdecl("PT_SetWindowTitle")
public func setWindowTitle(window: UnsafeRawPointer, title: UnsafeMutablePointer<Int8>) {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()
    nativeWindow.window.title = String(cString: title)
}

@_cdecl("PT_CreateImageSurface")
public func createImageSurface(window: UnsafeRawPointer, width: Int, height: Int) -> UnsafeMutableRawPointer {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()

    let contentView = nativeWindow.window.contentView! as NSView
    let view = ImageSurfaceView()
    view.frame = contentView.frame
    //contentView.addSubview(view)

    // TEST SWIFT UI
    // See: https://stackoverflow.com/questions/56833659/what-is-content-in-swiftui
    //let testView = TestView()
    //let myView = NSHostingView(rootView: testView)
    //myView.translatesAutoresizingMaskIntoConstraints = false

    //nativeWindow.window.contentView = myView
    //contentView.centerYAnchor.constraint(equalTo: contentView.centerYAnchor).isActive = true
    //contentView.centerXAnchor.constraint(equalTo: contentView.centerXAnchor).isActive = true

    let nativeImageSurface = NativeImageSurface(view, window: nativeWindow.window, width: width, height: height)
    return Unmanaged.passRetained(nativeImageSurface).toOpaque()
}

@_cdecl("PT_GetImageSurfaceInfo")
public func getImageSurfaceInfo(imageSurface: UnsafeMutablePointer<Int8>) -> NativeImageSurfaceInfo {
    return NativeImageSurfaceInfo(RedShift: 0, GreenShift: 8, BlueShift: 16, AlphaShift: 24)
}

@_cdecl("PT_UpdateImageSurface")
public func updateImageSurface(imageSurface: UnsafeMutablePointer<Int8>, data: UnsafeMutablePointer<UInt8>) {
    autoreleasepool {
        let nativeImageSurface = Unmanaged<NativeImageSurface>.fromOpaque(imageSurface).takeUnretainedValue()
        let view = nativeImageSurface.imageSurfaceView
        let width = nativeImageSurface.width
        let height = nativeImageSurface.height

        let colorSpace = CGColorSpaceCreateDeviceRGB()
        let provider = CGDataProvider(dataInfo: nil, data: data, size: width * height * 4, releaseData: { data, _, _ in })

        let bitmapInfo = CGBitmapInfo(rawValue: CGImageAlphaInfo.noneSkipLast.rawValue)
        let image = CGImage(width: width,
                            height: height,
                            bitsPerComponent: 8,
                            bitsPerPixel: 32,
                            bytesPerRow: width * 4,
                            space: colorSpace,
                            bitmapInfo: bitmapInfo, 
                            provider: provider!, 
                            decode: nil, 
                            shouldInterpolate: false, 
                            intent: .defaultIntent)

        view.setImage(image!)
        view.needsDisplay = true
    }
}

@_cdecl("PT_CreatePanel")
public func createPanel(window: UnsafeRawPointer) -> UnsafeMutableRawPointer {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()
    print("Create Panel")

    let contentView = nativeWindow.window.contentView! as NSView
    let panel = NSVisualEffectView()//NSView()
    panel.material = .sidebar
    panel.frame = NSMakeRect(0, 0, 400, 400)
    nativeWindow.window.contentView!.addSubview(panel)

    let nativePanel = NativePanel(panel)
    return Unmanaged.passRetained(nativePanel).toOpaque()
}

@_cdecl("PT_CreateButton")
public func createButton(parent: UnsafeRawPointer, title: UnsafeMutablePointer<Int8>) -> UnsafeMutableRawPointer {
    let nativePanel = Unmanaged<NativePanel>.fromOpaque(parent).takeUnretainedValue()
    print("Create Button")
    let button = NSButton(frame: NSMakeRect(0, 0, 100, 30)) 
    button.title = String(cString: title)
    button.bezelStyle = .rounded
    button.bezelColor = .controlAccentColor
    nativePanel.view.addSubview(button)

    let test = NSTextField(frame: NSMakeRect(50, 50, 100, 30))
    nativePanel.view.addSubview(test)

    let nativeControl = NativeControl(button)
    return Unmanaged.passRetained(nativeControl).toOpaque()
}