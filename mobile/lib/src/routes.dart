import 'package:flutter/material.dart';
import 'ui/auth/login_screen.dart';
import 'ui/auth/register_screen.dart';
import 'ui/home/home_screen.dart';
import 'core/token_storage.dart';

class Routes {
  static const String initial = '/';
  static const String login = '/login';
  static const String register = '/register';
  static const String home = '/home';

  static Route<dynamic> onGenerateRoute(RouteSettings settings) {
    switch (settings.name) {
      case initial:
        return MaterialPageRoute(builder: (_) => const InitialDecider());
      case login:
        return MaterialPageRoute(builder: (_) => const LoginScreen());
      case register:
        return MaterialPageRoute(builder: (_) => const RegisterScreen());
      case home:
        return MaterialPageRoute(builder: (_) => const HomeScreen());
      default:
        return MaterialPageRoute(
          builder: (_) => Scaffold(
            body: Center(child: Text('Route not found: ${settings.name}')),
          ),
        );
    }
  }
}

class InitialDecider extends StatefulWidget {
  const InitialDecider({super.key});

  @override
  State<InitialDecider> createState() => InitialDeciderState();
}

class InitialDeciderState extends State<InitialDecider> {
  @override
  void initState() {
    super.initState();
    _decide();
  }

  Future<void> _decide() async {
    final at = await TokenStorage.getAccessToken();
    final rt = await TokenStorage.getRefreshToken();
    final target = (at != null && at.isNotEmpty && rt != null && rt.isNotEmpty)
        ? Routes.home
        : Routes.login;
    if (!mounted) return;
    Navigator.of(context).pushReplacementNamed(target);
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(child: CircularProgressIndicator()),
    );
  }
}


