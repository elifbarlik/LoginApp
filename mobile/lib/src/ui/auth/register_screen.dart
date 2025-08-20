import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import '../../../src/services/auth_service.dart';
import '../../routes.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailCtrl = TextEditingController();
  final _usernameCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  bool _loading = false;
  String? _error;

  // Field-level server errors
  String? _emailServerError;
  String? _usernameServerError;
  String? _passwordServerError;

  // Regex rules to mirror web
  final _emailRegex = RegExp(r'^[^\s@]+@[^\s@]+\.[^\s@]+$');
  final _usernameRegex = RegExp(r'^(?!.*@)[A-Za-z0-9_.\-]+$');
  final _passwordRegex = RegExp(r'^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])\S{8,128}$');

  final _auth = AuthService();

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _loading = true;
      _error = null;
      _emailServerError = null;
      _usernameServerError = null;
      _passwordServerError = null;
    });
    try {
      await _auth.register(
        email: _emailCtrl.text.trim(),
        username: _usernameCtrl.text.trim(),
        password: _passwordCtrl.text,
      );
      if (!mounted) return;
      Navigator.of(context).pushReplacementNamed(Routes.home);
    } on DioException catch (e) {
      final data = e.response?.data;
      final msg = (data is Map && data['message'] is String) ? data['message'] as String : e.message ?? 'Kayıt başarısız';
      setState(() {
        _error = msg;
      });
      // Try to map backend validation errors to fields
      if (data is Map && data['errors'] is List) {
        for (final item in (data['errors'] as List)) {
          if (item is Map) {
            final field = (item['field'] ?? '').toString().toLowerCase();
            final messages = (item['messages'] as List?)?.map((e) => e.toString()).toList() ?? const [];
            if (field.endsWith('email') && messages.isNotEmpty) _emailServerError = messages.first;
            if (field.endsWith('username') && messages.isNotEmpty) _usernameServerError = messages.first;
            if (field.endsWith('password') && messages.isNotEmpty) _passwordServerError = messages.first;
          }
        }
        setState(() {});
      } else if (e.response?.statusCode == 409) {
        // Conflict like email already exists → show under email
        setState(() { _emailServerError = msg; });
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
      appBar: AppBar(title: const Text('Kayıt Ol')),
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
                    controller: _usernameCtrl,
                    decoration: const InputDecoration(labelText: 'Kullanıcı adı'),
                    onChanged: (_) { if (_usernameServerError != null) setState(() => _usernameServerError = null); },
                    validator: (v) {
                      if (v == null || v.isEmpty) return 'Kullanıcı adı zorunlu';
                      final val = v.trim();
                      if (val.length < 3 || val.length > 32) return '3-32 karakter olmalı';
                      if (!_usernameRegex.hasMatch(val)) return 'Sadece harf, rakam, . _ - içerebilir ve email olamaz';
                      if (_usernameServerError != null) return _usernameServerError;
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
                      if (v.length < 8 || v.length > 128) return 'Şifre 8-128 karakter olmalı';
                      if (!_passwordRegex.hasMatch(v)) return 'Büyük, küçük, rakam, sembol içermeli ve boşluk olmamalı';
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
                          : const Text('Kayıt Ol'),
                    ),
                  ),
                  TextButton(
                    onPressed: () => Navigator.of(context).pushReplacementNamed(Routes.login),
                    child: const Text('Zaten hesabın var mı? Giriş yap'),
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


