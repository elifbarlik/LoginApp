import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import '../../../src/services/auth_service.dart';
import '../../routes.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  bool _loading = false;
  String? _error;

  final _auth = AuthService();
  final _emailRegex = RegExp(r'^[^\s@]+@[^\s@]+\.[^\s@]+$');
  String? _emailServerError;
  String? _passwordServerError;

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _loading = true;
      _error = null;
      _emailServerError = null;
      _passwordServerError = null;
    });
    try {
      await _auth.login(email: _emailCtrl.text.trim(), password: _passwordCtrl.text);
      if (!mounted) return;
      Navigator.of(context).pushReplacementNamed(Routes.home);
    } on DioException catch (e) {
      final data = e.response?.data;
      final msg = (data is Map && data['message'] is String) ? data['message'] as String : e.message ?? 'Giriş başarısız';
      setState(() { _error = msg; });
      if (data is Map && data['errors'] is List) {
        for (final item in (data['errors'] as List)) {
          if (item is Map) {
            final field = (item['field'] ?? '').toString().toLowerCase();
            final messages = (item['messages'] as List?)?.map((e) => e.toString()).toList() ?? const [];
            if (field.endsWith('email') && messages.isNotEmpty) _emailServerError = messages.first;
            if (field.endsWith('password') && messages.isNotEmpty) _passwordServerError = messages.first;
          }
        }
        setState(() {});
      }
    } finally {
      if (mounted) {
        setState(() {
          _loading = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Giriş')),
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 480),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  TextFormField(
                    controller: _emailCtrl,
                    decoration: const InputDecoration(labelText: 'Email'),
                    keyboardType: TextInputType.emailAddress,
                    onChanged: (_) { if (_emailServerError != null) setState(() => _emailServerError = null); },
                    validator: (v) {
                      if (v == null || v.isEmpty) return 'Email zorunlu';
                      if (!_emailRegex.hasMatch(v.trim())) return 'Geçerli bir email girin (.domain içermeli)';
                      if (_emailServerError != null) return _emailServerError;
                      return null;
                    },
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _passwordCtrl,
                    decoration: const InputDecoration(labelText: 'Şifre'),
                    obscureText: true,
                    onChanged: (_) { if (_passwordServerError != null) setState(() => _passwordServerError = null); },
                    validator: (v) {
                      if (v == null || v.isEmpty) return 'Şifre zorunlu';
                      if (_passwordServerError != null) return _passwordServerError;
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),
                  if (_error != null)
                    Text(_error!, style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 8),
                  SizedBox(
                    width: double.infinity,
                    child: FilledButton(
                      onPressed: _loading ? null : _submit,
                      child: _loading
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                            )
                          : const Text('Giriş Yap'),
                    ),
                  ),
                  TextButton(
                    onPressed: () => Navigator.of(context).pushNamed(Routes.register),
                    child: const Text('Hesabın yok mu? Kayıt ol'),
                  )
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}


