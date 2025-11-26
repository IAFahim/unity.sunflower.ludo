import Foundation
import UserNotifications
import UIKit

@objc public class IOSNotificationBridge: NSObject {

    @objc public static let shared = IOSNotificationBridge()
    private let center = UNUserNotificationCenter.current()
    
    private override init() {
        super.init()
        center.delegate = NotificationDelegate.shared
    }

    @objc public func sendNotification(_ title: String, body: String, sound: String?) {
        let content = UNMutableNotificationContent()
        content.title = title
        content.body = body
        content.sound = (sound != nil) ? UNNotificationSound(named: UNNotificationSoundName(sound!)) : UNNotificationSound.default

        let trigger = UNTimeIntervalNotificationTrigger(timeInterval: 0.1, repeats: false)
        let request = UNNotificationRequest(identifier: UUID().uuidString, content: content, trigger: trigger)

        center.add(request) { error in
            if let error = error {
                print("[IOSNotificationBridge] Error scheduling notification: \(error.localizedDescription)")
            } else {
                print("[IOSNotificationBridge] Notification scheduled successfully.")
            }
        }
    }

    @objc public func requestPermission() {
        center.requestAuthorization(options: [.alert, .sound, .badge]) { granted, _ in
            print("[IOSNotificationBridge] Permission granted: \(granted)")
        }
    }
}

fileprivate class NotificationDelegate: NSObject, UNUserNotificationCenterDelegate {
    static let shared = NotificationDelegate()

    func userNotificationCenter(_ center: UNUserNotificationCenter,
                                willPresent notification: UNNotification,
                                withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        completionHandler([.alert, .sound, .badge])
    }
}
