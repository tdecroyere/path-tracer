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