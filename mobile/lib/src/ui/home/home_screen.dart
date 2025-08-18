import 'package:flutter/material.dart';
import '../../services/user_service.dart';
import '../../services/auth_service.dart';
import '../../services/admin_service.dart';
import '../../routes.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final _userService = UserService();
  final _authService = AuthService();
  late Future<UserProfileDto> _future;
  final _adminService = AdminService();
  bool _loadingStats = false;
  String? _error;
  int _totalUsers = 0;
  Map<String, int> _byRole = const {};
  List<AdminUserDto> _users = const [];
  String _filterRole = '';
  List<AdminUserDto> _latestUsers = const [];
  final TextEditingController _emailCtrl = TextEditingController();
  final TextEditingController _usernameCtrl = TextEditingController();
  final TextEditingController _phoneCtrl = TextEditingController();
  final TextEditingController _addressCtrl = TextEditingController();
  bool _controllersInitialized = false;
  bool _saving = false;
  String? _saveError;
  String? _saveSuccess;

  @override
  void initState() {
    super.initState();
    _future = _userService.getProfile();
  }

  @override
  void dispose() {
    _emailCtrl.dispose();
    _usernameCtrl.dispose();
    _phoneCtrl.dispose();
    _addressCtrl.dispose();
    super.dispose();
  }

  Future<void> _logout() async {
    await _authService.logout();
    if (mounted) {
      Navigator.of(context).pushReplacementNamed(Routes.login);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Dashboard')),
      body: FutureBuilder<UserProfileDto>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(child: Text('Hata: ${snapshot.error}'));
          }
          final p = snapshot.data!;
          if (!_controllersInitialized) {
            _emailCtrl.text = p.email;
            _usernameCtrl.text = p.username ?? '';
            _phoneCtrl.text = p.phone ?? '';
            _addressCtrl.text = p.address ?? '';
            _controllersInitialized = true;
          }
          final isAdmin = p.role.toLowerCase() == 'admin';
          if (isAdmin && !_loadingStats && _users.isEmpty && _latestUsers.isEmpty) {
            // Schedule fetch after first frame to avoid setState during build
            WidgetsBinding.instance.addPostFrameCallback((_) {
              if (mounted) {
                _fetchStatsAndUsers();
              }
            });
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header bar like Angular toolbar (simplified)
                Row(
                  children: [
                    const Text('Logify', style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
                    const Spacer(),
                    Text(p.username ?? p.email),
                    const SizedBox(width: 8),
                    CircleAvatar(child: Text(p.email.substring(0, 1).toUpperCase())),
                    const SizedBox(width: 8),
                    OutlinedButton(onPressed: _logout, child: const Text('Logout')),
                  ],
                ),
                const SizedBox(height: 16),
                const Text('Dashboard', style: TextStyle(fontSize: 26, fontWeight: FontWeight.w600)),
                const Text('Your account overview and quick settings'),
                const Divider(height: 32),

                // Profile card
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: const [
                            Expanded(child: Text('Welcome back', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold))),
                            Icon(Icons.person, color: Colors.indigo),
                          ],
                        ),
                        const SizedBox(height: 12),
                        Wrap(
                          spacing: 24,
                          runSpacing: 12,
                          children: [
                            _Stat(label: 'User ID', value: p.id),
                            _Stat(label: 'Role', value: p.role),
                            _Stat(label: 'Created', value: p.createdAtUtc),
                            _Stat(label: 'Last Login', value: p.lastLoginAtUtc ?? '-'),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),

                const SizedBox(height: 16),

                // Profile settings (email/username/phone/address) – skeleton only
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: const [
                            Expanded(child: Text('Profile settings', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold))),
                            Icon(Icons.settings, color: Colors.indigo),
                          ],
                        ),
                        const SizedBox(height: 12),
                        TextFormField(controller: _emailCtrl, decoration: const InputDecoration(labelText: 'Email')),
                        TextFormField(controller: _usernameCtrl, decoration: const InputDecoration(labelText: 'Username')),
                        TextFormField(controller: _phoneCtrl, decoration: const InputDecoration(labelText: 'Phone')),
                        TextFormField(controller: _addressCtrl, decoration: const InputDecoration(labelText: 'Address')),
                        const SizedBox(height: 8),
                        if (_saveError != null)
                          Padding(
                            padding: const EdgeInsets.only(bottom: 8),
                            child: Text(_saveError!, style: const TextStyle(color: Colors.red)),
                          ),
                        if (_saveSuccess != null)
                          Padding(
                            padding: const EdgeInsets.only(bottom: 8),
                            child: Text(_saveSuccess!, style: const TextStyle(color: Colors.green)),
                          ),
                        Row(
                          children: [
                            OutlinedButton(
                              onPressed: () {
                                setState(() {
                                  _emailCtrl.text = p.email;
                                  _usernameCtrl.text = p.username ?? '';
                                  _phoneCtrl.text = p.phone ?? '';
                                  _addressCtrl.text = p.address ?? '';
                                  _saveError = null;
                                  _saveSuccess = null;
                                });
                              },
                              child: const Text('Cancel'),
                            ),
                            const SizedBox(width: 8),
                            FilledButton(
                              onPressed: _saving
                                  ? null
                                  : () async {
                                      setState(() {
                                        _saving = true;
                                        _saveError = null;
                                        _saveSuccess = null;
                                      });
                                      try {
                                        await _userService.updateProfile(
                                          email: _emailCtrl.text.trim(),
                                          username: _usernameCtrl.text.trim().isEmpty ? null : _usernameCtrl.text.trim(),
                                          phone: _phoneCtrl.text.trim().isEmpty ? null : _phoneCtrl.text.trim(),
                                          address: _addressCtrl.text.trim().isEmpty ? null : _addressCtrl.text.trim(),
                                        );
                                        setState(() {
                                          _saveSuccess = 'Profil güncellendi';
                                          _future = _userService.getProfile();
                                          _controllersInitialized = false;
                                        });
                                      } catch (e) {
                                        setState(() {
                                          _saveError = e.toString();
                                        });
                                      } finally {
                                        setState(() {
                                          _saving = false;
                                        });
                                      }
                                    },
                              child: _saving
                                  ? const SizedBox(height: 18, width: 18, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                                  : const Text('Save changes'),
                            ),
                          ],
                        )
                      ],
                    ),
                  ),
                ),

                if (isAdmin) ...[
                  const SizedBox(height: 16),
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: const [
                              Expanded(child: Text('Admin panel', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold))),
                              Icon(Icons.admin_panel_settings, color: Colors.indigo),
                            ],
                          ),
                          const SizedBox(height: 12),
                          Row(
                            children: [
                              FilledButton(
                                onPressed: _fetchUsers,
                                child: const Text('Refresh users'),
                              ),
                              const SizedBox(width: 8),
                              OutlinedButton(
                                onPressed: () => setState(() => _filterRole = ''),
                                child: const Text('All'),
                              ),
                              const SizedBox(width: 8),
                              OutlinedButton(
                                onPressed: () => setState(() => _filterRole = 'User'),
                                child: const Text('Users'),
                              ),
                              const SizedBox(width: 8),
                              OutlinedButton(
                                onPressed: () => setState(() => _filterRole = 'Admin'),
                                child: const Text('Admins'),
                              ),
                            ],
                          ),
                          const SizedBox(height: 12),
                          if (_loadingStats) const Text('Loading...') else ...[
                            if (_error != null) Text(_error!, style: const TextStyle(color: Colors.red)),
                            Text('Total users: $_totalUsers'),
                            Text('By role: User ${_byRole['User'] ?? 0}, Admin ${_byRole['Admin'] ?? 0}'),
                            const SizedBox(height: 8),
                            const Text('Latest users:'),
                            for (final u in _latestUsers.take(5))
                              ListTile(
                                dense: true,
                                title: Text(u.email),
                                subtitle: Text('${u.role} • ${u.createdAtUtc ?? ''}'),
                              ),
                            const Divider(),
                            for (final u in _users.where((u) => _filterRole.isEmpty || u.role == _filterRole))
                              ListTile(
                                dense: true,
                                title: Text(u.email),
                                subtitle: Text(u.role),
                                trailing: Wrap(
                                  spacing: 8,
                                  children: [
                                    OutlinedButton(
                                      onPressed: () => _onEditUser(u),
                                      child: const Text('Edit'),
                                    ),
                                    OutlinedButton(
                                      onPressed: () => _onDeleteUser(u),
                                      style: OutlinedButton.styleFrom(foregroundColor: Colors.red),
                                      child: const Text('Delete'),
                                    ),
                                  ],
                                ),
                              ),
                          ],
                        ],
                      ),
                    ),
                  ),
                ]
              ],
            ),
          );
        },
      ),
    );
  }

  Future<void> _fetchUsers() async {
    setState(() {
      _loadingStats = true;
      _error = null;
    });
    try {
      final list = await _adminService.getUsers();
      final total = list.length;
      final br = <String, int>{};
      for (final u in list) {
        br[u.role] = (br[u.role] ?? 0) + 1;
      }
      setState(() {
        _users = list;
        _totalUsers = total;
        _byRole = br;
      });
    } catch (e) {
      setState(() {
        _error = e.toString();
      });
    } finally {
      setState(() {
        _loadingStats = false;
      });
    }
  }

  Future<void> _fetchStatsAndUsers() async {
    setState(() {
      _loadingStats = true;
      _error = null;
    });
    try {
      final stats = await _adminService.getStats();
      final list = await _adminService.getUsers();
      setState(() {
        _totalUsers = stats.totalUsers;
        _byRole = stats.byRole;
        _latestUsers = stats.latestUsers;
        _users = list;
      });
    } catch (e) {
      setState(() {
        _error = e.toString();
      });
    } finally {
      setState(() {
        _loadingStats = false;
      });
    }
  }

  Future<void> _onEditUser(AdminUserDto user) async {
    final controller = TextEditingController(text: user.role);
    final newRole = await showDialog<String>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: Text('Edit role for ${user.email}')
              ,
          content: DropdownButtonFormField<String>(
            value: (controller.text == 'Admin' || controller.text == 'User') ? controller.text : 'User',
            items: const [
              DropdownMenuItem(value: 'User', child: Text('User')),
              DropdownMenuItem(value: 'Admin', child: Text('Admin')),
            ],
            onChanged: (v) => controller.text = v ?? 'User',
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancel')),
            FilledButton(onPressed: () => Navigator.pop(context, controller.text), child: const Text('Save')),
          ],
        );
      },
    );
    if (newRole == null || newRole == user.role) return;
    try {
      await _adminService.updateUser(id: user.id, email: user.email, role: newRole);
      await _fetchUsers();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Update failed: $e')));
    }
  }

  Future<void> _onDeleteUser(AdminUserDto user) async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete user'),
        content: Text('Are you sure you want to delete ${user.email}?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancel')),
          FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Delete')),
        ],
      ),
    );
    if (ok != true) return;
    try {
      await _adminService.deleteUser(user.id);
      await _fetchUsers();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Delete failed: $e')));
    }
  }
}

class _Stat extends StatelessWidget {
  final String label;
  final String value;
  const _Stat({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 220,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: const TextStyle(color: Colors.grey)),
          const SizedBox(height: 4),
          SelectableText(value, style: const TextStyle(fontFamily: 'monospace')),
        ],
      ),
    );
  }
}


