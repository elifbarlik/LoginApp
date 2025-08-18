import 'package:flutter/material.dart';
import '../../services/admin_service.dart';

class AdminUsersScreen extends StatefulWidget {
  const AdminUsersScreen({super.key});

  @override
  State<AdminUsersScreen> createState() => _AdminUsersScreenState();
}

class _AdminUsersScreenState extends State<AdminUsersScreen> {
  final _service = AdminService();
  late Future<List<AdminUserDto>> _future;

  @override
  void initState() {
    super.initState();
    _future = _service.getUsers();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Admin - Users')),
      body: FutureBuilder<List<AdminUserDto>>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(child: Text('Hata: ${snapshot.error}'));
          }
          final users = snapshot.data ?? const <AdminUserDto>[];
          if (users.isEmpty) {
            return const Center(child: Text('Kullanıcı bulunamadı'));
          }
          return ListView.separated(
            itemCount: users.length,
            separatorBuilder: (_, __) => const Divider(height: 1),
            itemBuilder: (context, index) {
              final u = users[index];
              return ListTile(
                title: Text(u.email),
                subtitle: Text('${u.role} • ${u.createdAtUtc ?? ''}'),
              );
            },
          );
        },
      ),
    );
  }
}


