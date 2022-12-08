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
            processKey(inputObjects, KeyA, Int(Character("a").asciiValue!), event)
            processKey(inputObjects, KeyB, Int(Character("b").asciiValue!), event)
            processKey(inputObjects, KeyC, Int(Character("c").asciiValue!), event)
            processKey(inputObjects, KeyD, Int(Character("d").asciiValue!), event)
            processKey(inputObjects, KeyE, Int(Character("e").asciiValue!), event)
            processKey(inputObjects, KeyF, Int(Character("f").asciiValue!), event)
            processKey(inputObjects, KeyG, Int(Character("g").asciiValue!), event)
            processKey(inputObjects, KeyH, Int(Character("h").asciiValue!), event)
            processKey(inputObjects, KeyI, Int(Character("i").asciiValue!), event)
            processKey(inputObjects, KeyJ, Int(Character("j").asciiValue!), event)
            processKey(inputObjects, KeyK, Int(Character("k").asciiValue!), event)
            processKey(inputObjects, KeyL, Int(Character("l").asciiValue!), event)
            processKey(inputObjects, KeyM, Int(Character("m").asciiValue!), event)
            processKey(inputObjects, KeyN, Int(Character("n").asciiValue!), event)
            processKey(inputObjects, KeyO, Int(Character("o").asciiValue!), event)
            processKey(inputObjects, KeyP, Int(Character("p").asciiValue!), event)
            processKey(inputObjects, KeyQ, Int(Character("q").asciiValue!), event)
            processKey(inputObjects, KeyR, Int(Character("r").asciiValue!), event)
            processKey(inputObjects, KeyS, Int(Character("s").asciiValue!), event)
            processKey(inputObjects, KeyT, Int(Character("t").asciiValue!), event)
            processKey(inputObjects, KeyU, Int(Character("u").asciiValue!), event)
            processKey(inputObjects, KeyV, Int(Character("v").asciiValue!), event)
            processKey(inputObjects, KeyW, Int(Character("w").asciiValue!), event)
            processKey(inputObjects, KeyX, Int(Character("x").asciiValue!), event)
            processKey(inputObjects, KeyY, Int(Character("y").asciiValue!), event)
            processKey(inputObjects, KeyZ, Int(Character("z").asciiValue!), event)

             // Left Arrow
                processKey(inputObjects, Left, 123, event)
            /* else if (keyCode == 124) { // Right Arrow
                updateInputObject(&inputState.pointee.Keyboard.ArrowRight, event)
            } else if (keyCode == 126) { // Up Arrow
                updateInputObject(&inputState.pointee.Keyboard.ArrowUp, event)
            } else if (keyCode == 125) { // Down Arrow
                updateInputObject(&inputState.pointee.Keyboard.ArrowDown, event)
            }*/
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
    var isValid = false
    // TODO: Check keycode first

    guard let keyChar = event.characters else {
        return
    }

    if (Character(keyChar).asciiValue! == keyCode) {
       isValid = true 
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
