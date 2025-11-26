// ============================
// Variable Declaration Assignment
// ============================
const functions = require("firebase-functions/v2");
const admin = require("firebase-admin");
const cors = require("cors")({ origin: true });
const fetch = require("node-fetch");
const jwt = require("jsonwebtoken");

//Destructuring assignment
const { onDocumentUpdated } = require("firebase-functions/v2/firestore");

// Notification
const SOUND_NAME_NOTIFICATION_IOS ="alert.wav";
const SOUND_CHANNEL_ID_ANDROID ="custom_channel";

// Apple Configuration
const TEAM_ID = "YOUR TEAM ID APPLE"; // ex: A1B2C3D4E5
const CLIENT_ID = "YOUR CLIENT ID"; //ex: com.YourCompagny.YourProductName.SignInWithApple
const KEY_ID = "YOUR KEY ID";   // ex: 1A2B3C4D5E
const PRIVATE_KEY = `-----BEGIN PRIVATE KEY-----
YOUR PRIVATE KEY APPLE
-----END PRIVATE KEY-----`;

// ============================
// Apple Configuration (Use secret environment) => Use this method for storing sensitive value

// const TEAM_ID = process.env.APPLE_TEAM_ID;
// const CLIENT_ID = process.env.APPLE_CLIENT_ID;
// const KEY_ID = process.env.APPLE_KEY_ID;
// const PRIVATE_KEY = process.env.APPLE_PRIVATE_KEY.replace(/\\n/g, '\n');

// ============================

const REDIRECT_URI ="YOUR REDIRECT URI";
const DEEP_LINK_BASE ="YOUR DEEP LINK BASE";// ex: unitydl://com.YourCompagny.YourProductName

// ============================
// Apple Sign-In
// ============================

// Unity Redirection
exports.handleAppleRedirect = functions.https.onRequest((req, res) => {
  cors(req, res, () => {
    if (req.method !== "POST") {
      return res.status(405).send("Method Not Allowed");
    }

    const deepLinkBase = DEEP_LINK_BASE;
    const code = req.body.code;
    const error = req.body.error;

    if (code) {
      const deepLink = `${deepLinkBase}?code=${encodeURIComponent(code)}`;
      return res.redirect(302, deepLink);
    } else if (error) {
      const deepLink = `${deepLinkBase}?error=${encodeURIComponent(error)}`;
      return res.redirect(302, deepLink);
    } else {
      const deepLink = `${deepLinkBase}?error=missing_data_from_apple`;
      return res.redirect(302, deepLink);
    }
  });
});

// Redeeming Apple code for a JWT id_token
exports.exchangeAppleCode = functions.https.onRequest(async (req, res) => {
  cors(req, res, async () => {
    if (req.method !== "POST") {
      return res.status(405).send("Method Not Allowed");
    }

    const code = req.body.code;
    if (!code) {
      return res.status(400).send("Missing authorization code");
    }

    try {
      const now = Math.floor(Date.now() / 1000);
      const claims = {
        iss: TEAM_ID,
        iat: now,
        exp: now + 300,
        aud: "https://appleid.apple.com",
        sub: CLIENT_ID,
      };

      const clientSecret = jwt.sign(claims, PRIVATE_KEY, {
        algorithm: "ES256",
        keyid: KEY_ID,
      });

      const params = new URLSearchParams();
      params.append("client_id", CLIENT_ID);
      params.append("client_secret", clientSecret);
      params.append("code", code);
      params.append("grant_type", "authorization_code");
      params.append("redirect_uri", REDIRECT_URI);

      const tokenResponse = await fetch("https://appleid.apple.com/auth/token", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: params.toString(),
      });

      const data = await tokenResponse.json();

      if (!data.id_token) {
        console.error("Erreur Apple OAuth:", data);
        return res.status(500).send("Apple token exchange failed");
      }

      // Retourne le id_token au client (Android → Unity)
      return res.status(200).json({ id_token: data.id_token });
    } catch (error) {
      console.error("Erreur interne:", error);
      return res.status(500).send("Internal server error");
    }
  });
});

// ============================
// Notification when score reaches 5
// ============================
if (!admin.apps.length) {
  admin.initializeApp();
}
exports.notifyWhenScoreFive = onDocumentUpdated(
  "users/{userId}", 
  async (event) => {
    const change = event.data;
    if (!change) {
      return;
    }
    const before = change.before.data();
    const after = change.after.data();

     // Check if score field exists
    if (!before || !after || before.score === undefined || after.score === undefined) {
      return null;
    }
    // Check if score has just reached 5
    if (before.score < 5 && after.score >= 5) {
      const fcm_token = after.fcm_token;// Stored in Firestore by Unity app // MAKE SURE THE NAME IS THE SAME YOU USE IN FIREBASE THIS IS CASE SENSITIVE

      if (!fcm_token) {
        console.log("No fcmToken found for user:", event.params.userId);
        return null;
      }
      else
      {
         console.log("FcmToken is found for the user:", event.params.userId);
      }

      const yourTitle ="Congratulations :)";
      const yourBody ="You reached a score of 5!";
      const message = {
        token: fcm_token,
        notification: {
          title: yourTitle,
          body: yourBody,
        },
        android: {
          notification: {
            channelId: SOUND_CHANNEL_ID_ANDROID,//"default", // Make sure this channel exists in your Android app or put default if you don't want use custom sound
          },
        },
        apns: {
          payload: {
            aps: {
              alert: {
                title: yourTitle,
                body: yourBody,
              },
              sound: SOUND_NAME_NOTIFICATION_IOS,//"default",
            },
          },
        },
      };

      try {
        const response = await admin.messaging().send(message);
        console.log("Notification sent to", event.params.userId, ":", response);
      } catch (error) {
        console.error("Error sending FCM message:", error);
      }
    }
  }
);