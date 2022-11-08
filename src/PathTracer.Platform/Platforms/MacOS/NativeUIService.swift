import Cocoa
import NativeUIServiceModule

class NativeApplication {

}

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

@_cdecl("CreateApplication")
public func createApplication(applicationName: UnsafeMutablePointer<Int8>) -> UnsafeMutableRawPointer {
    NSApplication.shared.activate(ignoringOtherApps: true)
    NSApplication.shared.finishLaunching()
    
    buildMainMenu(applicationName: String(cString: applicationName))

    let application = NativeApplication()
    return Unmanaged.passRetained(application).toOpaque()
}

@_cdecl("CreateNativeWindow")
public func createNativeWindow(application: UnsafeMutablePointer<Int8>, title: UnsafeMutablePointer<Int8>, width: Int, height: Int, windowState: NativeWindowState) -> UnsafeMutableRawPointer {
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

@_cdecl("SetWindowTitle")
public func setWindowTitle(window: UnsafeMutablePointer<Int8>, title: UnsafeMutablePointer<Int8>) {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()
    nativeWindow.window.title = String(cString: title)
}

@_cdecl("CreateImageSurface")
public func createImageSurface(window: UnsafeMutablePointer<Int8>, width: Int, height: Int) -> UnsafeMutableRawPointer {
    let nativeWindow = Unmanaged<NativeWindow>.fromOpaque(window).takeUnretainedValue()

    let contentView = nativeWindow.window.contentView! as NSView
    let view = ImageSurfaceView()
    view.frame = contentView.frame
    contentView.addSubview(view)

    let nativeImageSurface = NativeImageSurface(view, window: nativeWindow.window, width: width, height: height)
    return Unmanaged.passRetained(nativeImageSurface).toOpaque()
}

@_cdecl("GetImageSurfaceInfo")
public func getImageSurfaceInfo(imageSurface: UnsafeMutablePointer<Int8>) -> NativeImageSurfaceInfo {
    return NativeImageSurfaceInfo(RedShift: 0, GreenShift: 8, BlueShift: 16, AlphaShift: 24)
}

@_cdecl("UpdateImageSurface")
public func updateImageSurface(imageSurface: UnsafeMutablePointer<Int8>, data: UnsafeMutablePointer<UInt8>) {
    autoreleasepool {
        let nativeImageSurface = Unmanaged<NativeImageSurface>.fromOpaque(imageSurface).takeUnretainedValue()
        let view = nativeImageSurface.imageSurfaceView
        let width = nativeImageSurface.width
        let height = nativeImageSurface.height

        // // TODO: ImageScale
        // // TODO: Is there a faster way?
        let colorSpace = CGColorSpaceCreateDeviceRGB()
        let context = CGContext(data: data, 
                                width: width, 
                                height: height, 
                                bitsPerComponent: 8, 
                                bytesPerRow: width * 4, 
                                space: colorSpace, 
                                bitmapInfo: CGImageAlphaInfo.noneSkipLast.rawValue)

        let imageRef = context?.makeImage()
        view.caLayer.contents = imageRef
        //view.caLayer.render(in: context!)

        /*let rect = CGRect(x: 0, y: 0, width: width, height: height)
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

        let graphicsContext = NSGraphicsContext(window: nativeImageSurface.window).cgContext
        graphicsContext.draw(image!, in: rect)
        graphicsContext.flush()
        
        view.caLayer.render(in: graphicsContext)
        
        let contentView = nativeImageSurface.window.contentView! as NSView
        contentView.setNeedsDisplay(rect)*/
    }
}

@_cdecl("ProcessSystemMessages")
public func processSystemMessages(application: UnsafeMutablePointer<Int8>) -> NativeAppStatus {
    var rawEvent: NSEvent? = nil
    var isActive = 1 // TODO: To change

    repeat {
        if (isActive == 1) {
            rawEvent = NSApplication.shared.nextEvent(matching: .any, until: nil, inMode: .default, dequeue: true)
        } else {
            //rawEvent = NSApplication.shared.nextEvent(matching: .any, until: nil, inMode: .default, dequeue: true)
        }

        guard let event = rawEvent else {
            isActive = 1
            return NativeAppStatus(IsRunning: 1, IsActive: 1)
        }

        switch event.type {
        /*case .keyUp, .keyDown:
            if (!event.modifierFlags.contains(.command)) {
                inputsManager.processKeyboardEvent(event)
            } else {
                NSApplication.shared.sendEvent(event)
            }
        case .mouseMoved, .leftMouseDragged:
            inputsManager.processMouseMovedEvent(event)
            NSApplication.shared.sendEvent(event)
        case .leftMouseUp, .leftMouseDown:
            // TODO: Prevent the event to be catched when dragging the window title
            inputsManager.processMouseLeftButtonEvent(event)
            NSApplication.shared.sendEvent(event)*/
        default:
            NSApplication.shared.sendEvent(event)
        }
    } while (rawEvent != nil)

    return NativeAppStatus(IsRunning: 1, IsActive: 1)
}

func buildMainMenu(applicationName: String) {
    let mainMenu = NSMenu(title: "MainMenu")
    
    let menuItem = mainMenu.addItem(withTitle: "ApplicationMenu", action: nil, keyEquivalent: "")
    let subMenu = NSMenu(title: "Application")
    mainMenu.setSubmenu(subMenu, for: menuItem)
    
    subMenu.addItem(withTitle: "About \(applicationName)", action: #selector(NSApplication.orderFrontStandardAboutPanel(_:)), keyEquivalent: "")
    subMenu.addItem(NSMenuItem.separator())

    let servicesMenuSub = subMenu.addItem(withTitle: "Services", action: nil, keyEquivalent: "")
    let servicesMenu = NSMenu(title:"Services")
    mainMenu.setSubmenu(servicesMenu, for: servicesMenuSub)
    NSApp.servicesMenu = servicesMenu
    subMenu.addItem(NSMenuItem.separator())
    
    var menuItemAdded = subMenu.addItem(withTitle: "Hide \(applicationName)", action:#selector(NSApplication.hide(_:)), keyEquivalent:"h")
    menuItemAdded.target = NSApp

    menuItemAdded = subMenu.addItem(withTitle: "Hide Others", action:#selector(NSApplication.hideOtherApplications(_:)), keyEquivalent:"h")
    menuItemAdded.keyEquivalentModifierMask = [.command, .option]
    menuItemAdded.target = NSApp

    menuItemAdded = subMenu.addItem(withTitle: "Show All", action:#selector(NSApplication.unhideAllApplications(_:)), keyEquivalent:"")
    menuItemAdded.target = NSApp

    subMenu.addItem(NSMenuItem.separator())
    subMenu.addItem(withTitle: "Quit", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")

    let windowMenuItem = mainMenu.addItem(withTitle: "Window", action: nil, keyEquivalent: "")
    let windowSubMenu = NSMenu(title: "Window")
    mainMenu.setSubmenu(windowSubMenu, for: windowMenuItem)

    windowSubMenu.addItem(withTitle: "Minimize", action: #selector(NSWindow.performMiniaturize(_:)), keyEquivalent: "m")
    windowSubMenu.addItem(withTitle: "Zoom", action: #selector(NSWindow.performZoom), keyEquivalent: "")
    
    NSApp.mainMenu = mainMenu
    NSApp.windowsMenu = windowSubMenu
}