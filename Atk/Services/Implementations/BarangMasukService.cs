using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.BarangMasuk;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Implementations
{
    public class BarangMasukService : IBarangMasuk
    {
        private readonly ApplicationDbContext _context;
        public BarangMasukService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BarangMasukResponseDto> CreateAsync(BarangMasukCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.JumlahMasuk <= 0) throw new ArgumentException("JumlahMasuk harus > 0");

            var now = DateTime.Now;

            var entity = new BarangMasuk
            {
                BarangId = dto.BarangId,
                SupplierId = dto.SupplierId,
                JumlahMasuk = dto.JumlahMasuk,
                HargaSatuan = dto.HargaSatuan,
                TanggalMasuk = dto.TanggalMasuk,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.BarangMasuks.AddAsync(entity);

            // **PERBAIKAN**: update stok pada tabel Barangs, bukan BarangMasuks
            var barang = await _context.Barangs.FindAsync(dto.BarangId);
            if (barang == null)
            {
                // roll back add by not saving and throw
                throw new KeyNotFoundException($"Barang dengan id {dto.BarangId} tidak ditemukan.");
            }

            barang.Stok += dto.JumlahMasuk;

            await _context.SaveChangesAsync();

            return new BarangMasukResponseDto
            {
                Id = entity.Id,
                BarangId = entity.BarangId,
                SupplierId = entity.SupplierId,
                JumlahMasuk = entity.JumlahMasuk,
                HargaSatuan = entity.HargaSatuan,
                TanggalMasuk = entity.TanggalMasuk,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<IEnumerable<BarangMasukResponseDto>> CreateBulkAsync(IEnumerable<BarangMasukCreateDto> dtos)
        {
            if (dtos == null) throw new ArgumentNullException(nameof(dtos));

            var result = new List<BarangMasukResponseDto>();
            var now = DateTime.Now;

            // Gunakan transaction supaya atomic
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var dto in dtos)
                {
                    if (dto.JumlahMasuk <= 0)
                        throw new ArgumentException("JumlahMasuk harus lebih dari 0.");

                    var barang = await _context.Barangs.FindAsync(dto.BarangId);
                    if (barang == null)
                        throw new KeyNotFoundException($"Barang id {dto.BarangId} tidak ditemukan.");

                    var entity = new BarangMasuk
                    {
                        BarangId = dto.BarangId,
                        SupplierId = dto.SupplierId,
                        JumlahMasuk = dto.JumlahMasuk,
                        HargaSatuan = dto.HargaSatuan,
                        TanggalMasuk = dto.TanggalMasuk,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    await _context.BarangMasuks.AddAsync(entity);

                    // update stok
                    barang.Stok += dto.JumlahMasuk;

                    result.Add(new BarangMasukResponseDto
                    {
                        Id = entity.Id, // note: id akan terisi setelah SaveChanges
                        BarangId = entity.BarangId,
                        SupplierId = entity.SupplierId,
                        JumlahMasuk = entity.JumlahMasuk,
                        HargaSatuan = entity.HargaSatuan,
                        TanggalMasuk = entity.TanggalMasuk,
                        CreatedAt = entity.CreatedAt
                    });
                }

                await _context.SaveChangesAsync();

                // after SaveChanges, adjust Ids in result (because entity.Id assigned after save)
                // reload newly added entries to get their Ids (or update by reading last inserted)
                // simplest: fetch created rows by createdAt timestamp range (ok for small sets)
                await transaction.CommitAsync();

                // Refresh Ids by querying entries createdAt == now (best-effort)
                var createdList = await _context.BarangMasuks
                    .Where(b => b.CreatedAt == now)
                    .ToListAsync();

                // map back Ids to result items (match by BarangId, JumlahMasuk, HargaSatuan, CreatedAt)
                foreach (var r in result)
                {
                    var match = createdList.FirstOrDefault(c =>
                        c.BarangId == r.BarangId &&
                        c.JumlahMasuk == r.JumlahMasuk &&
                        c.HargaSatuan == r.HargaSatuan &&
                        c.CreatedAt == r.CreatedAt);

                    if (match != null)
                        r.Id = match.Id;
                }

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<BarangMasukResponseDto>> GetAllAsync()
        {
            var list = await _context.BarangMasuks
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return list.Select(p => new BarangMasukResponseDto
            {
                Id = p.Id,
                BarangId = p.BarangId,
                SupplierId = p.SupplierId,
                JumlahMasuk = p.JumlahMasuk,
                HargaSatuan = p.HargaSatuan,
                TanggalMasuk = p.TanggalMasuk,
                CreatedAt = p.CreatedAt
            });
        }

        public async Task<BarangMasukResponseDto?> GetByIdAsync(int id)
        {
            var p = await _context.BarangMasuks
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return null;

            return new BarangMasukResponseDto
            {
                Id = p.Id,
                BarangId = p.BarangId,
                SupplierId = p.SupplierId,
                JumlahMasuk = p.JumlahMasuk,
                HargaSatuan = p.HargaSatuan,
                TanggalMasuk = p.TanggalMasuk,
                CreatedAt = p.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, BarangMasukUpdateDto dto)
        {
            var entity = await _context.BarangMasuks.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            // adjust stok: kurangi stok lama lalu tambah stok baru jika BarangId sama
            var oldBarang = await _context.Barangs.FindAsync(entity.BarangId);
            if (oldBarang != null)
            {
                oldBarang.Stok -= entity.JumlahMasuk;
            }

            // apply update
            entity.BarangId = dto.BarangId;
            entity.SupplierId = dto.SupplierId;
            entity.JumlahMasuk = dto.JumlahMasuk;
            entity.HargaSatuan = dto.HargaSatuan;
            entity.TanggalMasuk = dto.TanggalMasuk;
            entity.UpdatedAt = DateTime.Now;

            var newBarang = await _context.Barangs.FindAsync(dto.BarangId);
            if (newBarang != null)
            {
                newBarang.Stok += dto.JumlahMasuk;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.BarangMasuks.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            // rollback stok
            var barang = await _context.Barangs.FindAsync(entity.BarangId);
            if (barang != null)
            {
                barang.Stok -= entity.JumlahMasuk;
            }

            _context.BarangMasuks.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalBarangMasukByDateAsync(DateTime date)
        {
            var d = date.Date;
            return await _context.BarangMasuks
                .Where(b => b.TanggalMasuk.Date == d)
                .SumAsync(b => b.JumlahMasuk);
        }
    }
}
