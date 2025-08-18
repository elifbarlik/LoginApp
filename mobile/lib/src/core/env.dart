import 'dart:io' show Platform;

class Env {
  static String get apiBaseUrl {
    // If running on web, Platform is not supported, but our imports limit to dart:io only on io builds.
    // For Windows/macOS/Linux/Android/iOS, keep HTTPS. For web, prefer HTTP to avoid self-signed cert issues.
    try {
      if (Platform.isAndroid) return 'https://10.0.2.2:7128';
      if (Platform.isWindows || Platform.isLinux || Platform.isMacOS) return 'https://localhost:7128';
      return 'https://localhost:7128';
    } catch (_) {
      // Web path: fall back to http to avoid self-signed TLS issues
      return 'http://localhost:5112';
    }
  }
}


