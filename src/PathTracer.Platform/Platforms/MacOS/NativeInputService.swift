import Cocoa
import NativeUIServiceModule

var applicationInputQueues = [UnsafeMutablePointer<Int8>: [NSEvent]]()

public func nativeInputProcessKeyboardEvent(_ application: UnsafeMutablePointer<Int8>, _ event: NSEvent) {
    if (applicationInputQueues[application] == nil) {
        applicationInputQueues[application] = []
    }

    applicationInputQueues[application]!.append(event)
}

@_cdecl("PT_GetInputState")
public func getInputState(application: UnsafeMutablePointer<Int8>, inputState: UnsafeMutablePointer<NativeInputState>) {
    guard let queue = applicationInputQueues[application] else {
        return
    } 

    for event in queue { 
        guard let keyChar = event.characters else {
            return
        }

        let keyCode = event.keyCode

        if (keyCode == 123) { // Left Arrow
            updateInputObject(&inputState.pointee.Keyboard.ArrowLeft, event)
        } else if (keyCode == 124) { // Right Arrow
            updateInputObject(&inputState.pointee.Keyboard.ArrowRight, event)
        } else if (keyCode == 126) { // Up Arrow
            updateInputObject(&inputState.pointee.Keyboard.ArrowUp, event)
        } else if (keyCode == 125) { // Down Arrow
            updateInputObject(&inputState.pointee.Keyboard.ArrowDown, event)
        }

        switch (keyChar) {
        case "a":
            updateInputObject(&inputState.pointee.Keyboard.KeyA, event)
        case "d":
            updateInputObject(&inputState.pointee.Keyboard.KeyD, event)
        case "q":
            updateInputObject(&inputState.pointee.Keyboard.KeyQ, event)
        case "s":
            updateInputObject(&inputState.pointee.Keyboard.KeyS, event)
        case "z":
            updateInputObject(&inputState.pointee.Keyboard.KeyZ, event)
        default:
            continue
        }
    }

    applicationInputQueues[application] = []
}

private func updateInputObject(_ inputObject: inout NativeInputObject, _ event: NSEvent) {
    inputObject.Value = (event.type == .keyDown || event.type == .leftMouseDown) ? 1.0 : 0.0
}
