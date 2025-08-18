import 'package:dio/dio.dart';
import '../core/api_client.dart';

class UserProfileDto {
  final String id;
  final String email;
  final String role;
  final String? username;
  final String? phone;
  final String? address;
  final String createdAtUtc;
  final String? lastLoginAtUtc;

  UserProfileDto({
    required this.id,
    required this.email,
    required this.role,
    required this.createdAtUtc,
    this.username,
    this.phone,
    this.address,
    this.lastLoginAtUtc,
  });

  factory UserProfileDto.fromJson(Map<String, dynamic> json) => UserProfileDto(
        id: json['id'] as String,
        email: json['email'] as String,
        role: json['role'] as String,
        username: json['username'] as String?,
        phone: json['phone'] as String?,
        address: json['address'] as String?,
        createdAtUtc: json['createdAtUtc']?.toString() ?? '',
        lastLoginAtUtc: json['lastLoginAtUtc']?.toString(),
      );
}

class UserService {
  final Dio _http = ApiClient().dio;

  Future<UserProfileDto> getProfile() async {
    final res = await _http.get('/user/profile');
    return UserProfileDto.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<UserProfileDto> updateProfile({
    String? email,
    String? username,
    String? phone,
    String? address,
  }) async {
    final res = await _http.put('/user/profile', data: {
      'email': email,
      'username': username,
      'phone': phone,
      'address': address,
    });
    return UserProfileDto.fromJson(Map<String, dynamic>.from(res.data as Map));
  }
}


