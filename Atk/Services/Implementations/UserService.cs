using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.Users;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateDivisiUserAsync(UserCreateDivisiDto dto)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                Password = hashedPassword,
                Nama = dto.Nama,
                NamaDivisi = dto.NamaDivisi,
                Role = UserRole.Divisi
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(int id, UserUpdateDivisiDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.Username = dto.Username;
            user.Nama = dto.Nama;
            user.NamaDivisi = dto.NamaDivisi;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);

            try
            {
                // ✅ Tambahkan try-catch di sini
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Ini menangkap error database (seperti Foreign Key Constraint)
                // Sebaiknya log exception 'ex' di server Anda (tidak ditampilkan di sini)
                
                // Jika user yang dihapus memiliki relasi data di tabel lain
                // Maka operasi hapus gagal. Kita kembalikan 'false'.
                return false; 
            }
            catch (Exception)
            {
                // Menangkap error umum lainnya
                return false;
            }
        }
    }
    
}