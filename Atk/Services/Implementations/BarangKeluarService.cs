using Atk.Data;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services
{
    public class BarangKeluarService : IBarangKeluar
    {
        private readonly ApplicationDbContext _context;


        public BarangKeluarService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ambil semua data barang keluar
        public async Task<List<BarangKeluar>> GetAllAsync()
        {
            return await _context.BarangKeluars
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                .OrderByDescending(x => x.TanggalKeluar)
                .ToListAsync();
        }

        // Ambil berdasarkan Id
        public async Task<BarangKeluar?> GetByIdAsync(int id)
        {
            return await _context.BarangKeluars
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // Ambil berdasarkan permintaan
        public async Task<List<BarangKeluar>> GetByPermintaanAsync(int permintaanId)
        {
            return await _context.BarangKeluars
                .Where(x => x.PermintaanId == permintaanId)
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                .ToListAsync();
        }

        // Ambil berdasarkan barang tertentu
        public async Task<List<BarangKeluar>> GetByBarangAsync(int barangId)
        {
            return await _context.BarangKeluars
                .Where(x => x.BarangId == barangId)
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                .ToListAsync();
        }
    }
}
