import 'package:dio/dio.dart';
import '../core/api_client.dart';

class AdminUserDto {
  final String id;
  final String email;
  final String role;
  final String? username;
  final String? phone;
  final String? address;
  final String? createdAtUtc;
  final String? lastLoginAtUtc;

  AdminUserDto({
    required this.id,
    required this.email,
    required this.role,
    this.username,
    this.phone,
    this.address,
    this.createdAtUtc,
    this.lastLoginAtUtc,
  });

  factory AdminUserDto.fromJson(Map<String, dynamic> json) => AdminUserDto(
        id: json['id'] as String,
        email: json['email'] as String,
        role: json['role'] as String,
        username: json['username'] as String?,
        phone: json['phone'] as String?,
        address: json['address'] as String?,
        createdAtUtc: json['createdAtUtc']?.toString(),
        lastLoginAtUtc: json['lastLoginAtUtc']?.toString(),
      );
}

class AdminStatsDto {
  final int totalUsers;
  final Map<String, int> byRole;
  final List<AdminUserDto> latestUsers;

  AdminStatsDto({required this.totalUsers, required this.byRole, required this.latestUsers});

  factory AdminStatsDto.fromJson(Map<String, dynamic> json) {
    // totalUsers can be 'totalUsers' or 'TotalUsers'
    final totalUsers = (json['totalUsers'] ?? json['TotalUsers']) as num?;

    // byRole can be a map, or we can derive it from AdminUsers/RegularUsers
    final byRole = <String, int>{};
    final byRoleRaw = (json['byRole'] ?? json['ByRole']);
    if (byRoleRaw is Map) {
      byRoleRaw.forEach((key, value) {
        if (key is String && value is num) byRole[key] = value.toInt();
      });
    } else {
      final adminCount = (json['AdminUsers'] as num?)?.toInt();
      final regularCount = (json['RegularUsers'] as num?)?.toInt();
      if (adminCount != null) byRole['Admin'] = adminCount;
      if (regularCount != null) byRole['User'] = regularCount;
    }

    // latest users may come as 'latestUsers' or 'RecentUsers'
    final latestRaw = (json['latestUsers'] ?? json['RecentUsers']);
    final latest = <AdminUserDto>[];
    if (latestRaw is List) {
      for (final e in latestRaw) {
        if (e is Map) latest.add(AdminUserDto.fromJson(Map<String, dynamic>.from(e)));
      }
    }

    return AdminStatsDto(
      totalUsers: totalUsers?.toInt() ?? latest.length,
      byRole: byRole,
      latestUsers: latest,
    );
  }
}

class AdminService {
  final Dio _http = ApiClient().dio;

  Future<List<AdminUserDto>> getUsers({String? role}) async {
    Response res;
    if (role == null) {
      res = await _http.get('/admin/users');
    } else {
      // Try path first, then query
      try {
        res = await _http.get('/admin/users/$role');
      } on DioException {
        res = await _http.get('/admin/users', queryParameters: {'role': role});
      }
    }
    final data = res.data;
    if (data is List) {
      return data.map((e) => AdminUserDto.fromJson(Map<String, dynamic>.from(e))).toList();
    }
    if (data is Map<String, dynamic>) {
      final list = (data['items'] ?? data['data'] ?? data['users'] ?? data['result']) as List?;
      if (list != null) {
        return list.map((e) => AdminUserDto.fromJson(Map<String, dynamic>.from(e))).toList();
      }
    }
    return <AdminUserDto>[];
  }

  Future<AdminStatsDto> getStats() async {
    final res = await _http.get('/admin/stats');
    final data = Map<String, dynamic>.from(res.data as Map);
    return AdminStatsDto.fromJson(data);
  }

  Future<AdminUserDto> getUser(String id) async {
    final res = await _http.get('/admin/users/$id');
    return AdminUserDto.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<AdminUserDto> createUser({
    required String email,
    required String password,
    required String role,
  }) async {
    final res = await _http.post('/admin/users', data: {
      'email': email,
      'password': password,
      'role': role,
    });
    return AdminUserDto.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<AdminUserDto> updateUser({
    required String id,
    required String email,
    String? role,
  }) async {
    final res = await _http.put('/admin/users/$id', data: {
      'email': email,
      'role': role,
    });
    return AdminUserDto.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<void> deleteUser(String id) async {
    await _http.delete('/admin/users/$id');
  }
}


