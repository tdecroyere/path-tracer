import Cocoa
import NativeUIServiceModule

var applicationInputQueues = [UnsafeMutablePointer<Int8>: [NSEvent]]()

public func nativeInputProcessEvent(_ application: UnsafeMutablePointer<Int8>, _ event: NSEvent) {
    if (applicationInputQueues[application] == nil) {
        applicationInputQueues[application] = []
    }

    applicationInputQueues[application]!.append(event)
}

@_cdecl("PT_UpdateInputState")
public func updateInputState(application: UnsafeMutablePointer<Int8>, inputState: UnsafeMutablePointer<InputState>) {
    guard let queue = applicationInputQueues[application] else {
        return
    } 

    for event in queue { 
        let inputObjects = inputState.pointee.InputObjectPointer.bindMemory(to: InputObject.self, capacity: Int(inputState.pointee.InputObjectCount))

        if (event.type == .keyUp || event.type == .keyDown) {
            processKey(inputObjects, KeyA, "a", event)
            processKey(inputObjects, KeyB, "b", event)
            processKey(inputObjects, KeyC, "c", event)
            processKey(inputObjects, KeyD, "d", event)
            processKey(inputObjects, KeyE, "e", event)
            processKey(inputObjects, KeyF, "f", event)
            processKey(inputObjects, KeyG, "g", event)
            processKey(inputObjects, KeyH, "h", event)
            processKey(inputObjects, KeyI, "i", event)
            processKey(inputObjects, KeyJ, "j", event)
            processKey(inputObjects, KeyK, "k", event)
            processKey(inputObjects, KeyL, "l", event)
            processKey(inputObjects, KeyM, "m", event)
            processKey(inputObjects, KeyN, "n", event)
            processKey(inputObjects, KeyO, "o", event)
            processKey(inputObjects, KeyP, "p", event)
            processKey(inputObjects, KeyQ, "q", event)
            processKey(inputObjects, KeyR, "r", event)
            processKey(inputObjects, KeyS, "s", event)
            processKey(inputObjects, KeyT, "t", event)
            processKey(inputObjects, KeyU, "u", event)
            processKey(inputObjects, KeyV, "v", event)
            processKey(inputObjects, KeyW, "w", event)
            processKey(inputObjects, KeyX, "x", event)
            processKey(inputObjects, KeyY, "y", event)
            processKey(inputObjects, KeyZ, "z", event)

            // Left Arrow
            processKey(inputObjects, Left, 123, event)
            
            // Right Arrow
            processKey(inputObjects, Right, 124, event)
            
            // Up Arrow
            processKey(inputObjects, Up, 126, event)
            
            // Down Arrow
            processKey(inputObjects, Down, 125, event)

        } else if (event.type == .leftMouseDown || event.type == .leftMouseUp) {
            inputObjects[Int(MouseLeftButton.rawValue)].Value = (event.type == .leftMouseDown) ? 1.0 : 0.0
        } else if (event.type == .mouseMoved || event.type == .leftMouseDragged) {
            let contentView = globalWindow!.window.contentView! as NSView
            let mainScreenScaling = globalWindow!.window.screen!.backingScaleFactor

            let size = contentView.frame.size
            
            inputObjects[Int(MouseAxisX.rawValue)].Value = Float(event.locationInWindow.x) * Float(mainScreenScaling)
            inputObjects[Int(MouseAxisY.rawValue)].Value = (Float(size.height) - Float(event.locationInWindow.y)) * Float(mainScreenScaling)
        }
    }

    applicationInputQueues[application] = []
}

private func processKey(_ inputObjects: UnsafeMutablePointer<InputObject>, _ key: InputObjectKey, _ keyCode: String, _ event: NSEvent) {
   processKey(inputObjects, key, Int(Character(keyCode).asciiValue!), event) 
}

private func processKey(_ inputObjects: UnsafeMutablePointer<InputObject>, _ key: InputObjectKey, _ keyCode: Int, _ event: NSEvent)
{
    var isValid = event.keyCode == keyCode

    if (event.characters != nil) {
        guard let keyChar = event.characters else {
            return
        }

        let asciiValue = Character(keyChar).asciiValue

        if (asciiValue != nil && asciiValue! == keyCode) {
            isValid = true 
        }
    }

    if (isValid) {
        let keyRawValue = Int(key.rawValue)
        inputObjects[keyRawValue].Value = (event.type == .keyDown) ? 1.0 : 0.0;

        if (event.type == .keyDown) {
            inputObjects[keyRawValue].Repeatcount += 1;
        } else {
            inputObjects[keyRawValue].Repeatcount = 0;
        }
    }
}

private func updateInputObject(_ inputObject: inout InputObject, _ event: NSEvent) {
    inputObject.Value = (event.type == .keyDown || event.type == .leftMouseDown) ? 1.0 : 0.0
}
