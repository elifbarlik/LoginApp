import 'dart:async';
import 'package:dio/dio.dart';
import 'env.dart';
import 'token_storage.dart';

class ApiClient {
  static final ApiClient _instance = ApiClient._internal();
  factory ApiClient() => _instance;
  ApiClient._internal() {
    _dio = Dio(BaseOptions(
      baseUrl: Env.apiBaseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 20),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    ));

    _dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final accessToken = await TokenStorage.getAccessToken();
        if (accessToken != null && accessToken.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $accessToken';
        }
        handler.next(options);
      },
      onError: (error, handler) async {
        // Only try refresh for non-auth endpoints
        final path = error.requestOptions.path;
        final isAuth = RegExp(r"/auth/(login|register|google|refresh)", caseSensitive: false).hasMatch(path);
        if (error.response?.statusCode == 401 && !_isRefreshing && !isAuth) {
          final refreshed = await _refreshTokens();
          if (refreshed) {
            final accessToken = await TokenStorage.getAccessToken();
            final requestOptions = error.requestOptions;
            requestOptions.headers['Authorization'] = 'Bearer $accessToken';
            try {
              final response = await _dio.fetch(requestOptions);
              return handler.resolve(response);
            } catch (e) {
              return handler.reject(error);
            }
          }
        }
        // If refresh endpoint itself failed, clear tokens
        if (path.contains('/auth/refresh') && (error.response?.statusCode == 400 || error.response?.statusCode == 401)) {
          await TokenStorage.clearTokens();
        }
        handler.next(error);
      },
    ));
  }

  late final Dio _dio;
  bool _isRefreshing = false;
  Completer<bool>? _refreshCompleter;

  Dio get dio => _dio;

  Future<bool> _refreshTokens() async {
    if (_isRefreshing) {
      return _refreshCompleter?.future ?? Future.value(false);
    }
    _isRefreshing = true;
    _refreshCompleter = Completer<bool>();

    try {
      final accessToken = await TokenStorage.getAccessToken() ?? '';
      final refreshToken = await TokenStorage.getRefreshToken() ?? '';
      if (refreshToken.isEmpty) {
        _refreshCompleter?.complete(false);
        return false;
      }

      final res = await _dio.post('/auth/refresh', data: {
        'accessToken': accessToken,
        'refreshToken': refreshToken,
      });

      final data = res.data as Map<String, dynamic>;
      await TokenStorage.saveTokens(
        accessToken: data['accessToken'] as String,
        refreshToken: data['refreshToken'] as String,
        role: data['role'] as String,
      );
      _refreshCompleter?.complete(true);
      return true;
    } catch (_) {
      await TokenStorage.clearTokens();
      _refreshCompleter?.complete(false);
      return false;
    } finally {
      _isRefreshing = false;
      _refreshCompleter = null;
    }
  }
}


