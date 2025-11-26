import Foundation

@_silgen_name("UnitySendMessage")
private func UnitySendMessage(_ objectName: UnsafePointer<CChar>, _ methodName: UnsafePointer<CChar>, _ message: UnsafePointer<CChar>)

class UnityMessenger {
    static func send(_ objectName: String, _ methodName: String, _ message: String) {
        if let objectNameC = objectName.cString(using: .utf8),
           let methodNameC = methodName.cString(using: .utf8),
           let messageC = message.cString(using: .utf8) {
            UnitySendMessage(objectNameC, methodNameC, messageC)
        }else {
            print("Failed to convert string to CChar for UnitySendMessage.")
        }
    }
}
