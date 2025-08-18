import 'package:dio/dio.dart';
import '../core/api_client.dart';
import '../core/token_storage.dart';

class AuthService {
  final Dio _http = ApiClient().dio;

  Future<void> register({
    required String email,
    required String username,
    required String password,
    String? role,
  }) async {
    final res = await _http.post('/auth/register', data: {
      'email': email,
      'username': username,
      'password': password,
      'role': role,
    });
    final data = res.data as Map<String, dynamic>;
    await TokenStorage.saveTokens(
      accessToken: data['accessToken'] as String,
      refreshToken: data['refreshToken'] as String,
      role: data['role'] as String,
    );
  }

  Future<void> login({
    required String email,
    required String password,
  }) async {
    final res = await _http.post('/auth/login', data: {
      'email': email,
      'password': password,
    });
    final data = res.data as Map<String, dynamic>;
    await TokenStorage.saveTokens(
      accessToken: data['accessToken'] as String,
      refreshToken: data['refreshToken'] as String,
      role: data['role'] as String,
    );
  }

  Future<void> loginWithGoogle({required String idToken}) async {
    final res = await _http.post('/auth/google', data: {
      'idToken': idToken,
    });
    final data = res.data as Map<String, dynamic>;
    await TokenStorage.saveTokens(
      accessToken: data['accessToken'] as String,
      refreshToken: data['refreshToken'] as String,
      role: data['role'] as String,
    );
  }

  Future<void> logout() async {
    final rt = await TokenStorage.getRefreshToken() ?? '';
    await _http.post('/auth/logout', data: {
      'refreshToken': rt,
    });
    await TokenStorage.clearTokens();
  }
}


