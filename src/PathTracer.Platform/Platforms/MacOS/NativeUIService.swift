import Cocoa
import NativeUIServiceModule

class NativeWindow {
    let window: NSWindow

    init(_ window: NSWindow) {
        self.window = window
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

@_cdecl("PT_CreateWindow")
public func createWindow(application: UnsafeMutablePointer<Int8>, title: UnsafeMutablePointer<Int8>, width: Int, height: Int, windowState: NativeWindowState) -> UnsafeMutableRawPointer {
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
    return Unmanaged.passRetained(nativeWindow).toOpaque()
}

@_cdecl("PT_GetWindowRenderSize")
public func getWindowRenderSize(window: UnsafeMutablePointer<Int8>) -> NativeWindowSize {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()

    let contentView = nativeWindow.window.contentView! as NSView
    let mainScreenScaling = nativeWindow.window.screen!.backingScaleFactor

    var size = contentView.frame.size
    size.width *= mainScreenScaling;
    size.height *= mainScreenScaling;

    return NativeWindowSize(Width: Int32(size.width), Height: Int32(size.height))
}

@_cdecl("PT_SetWindowTitle")
public func setWindowTitle(window: UnsafeMutablePointer<Int8>, title: UnsafeMutablePointer<Int8>) {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()
    nativeWindow.window.title = String(cString: title)
}

@_cdecl("PT_CreateImageSurface")
public func createImageSurface(window: UnsafeMutablePointer<Int8>, width: Int, height: Int) -> UnsafeMutableRawPointer {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()

    let contentView = nativeWindow.window.contentView! as NSView
    let view = ImageSurfaceView()
    view.frame = contentView.frame
    contentView.addSubview(view)

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