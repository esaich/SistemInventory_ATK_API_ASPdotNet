using Atk.Data;
using Atk.Models;
using Atk.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Atk.Tests.Services
{
    public class BarangKeluarServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new ApplicationDbContext(options);

            // =======================
            // SEED DATA
            // =======================
            var barang = new Barang
            {
                Id = 1,
                NamaBarang = "Kertas A4",
                Satuan = "pak",
                Stok = 100
            };

            var permintaan = new PermintaanBarang
            {
                Id = 10,
                UserId = 1,
                BarangId = 1,
                JumlahDiminta = 5,
                Status = StatusPermintaan.Disetujui,
                CreatedAt = DateTime.Now
            };

            var keluar1 = new BarangKeluar
            {
                Id = 100,
                BarangId = 1,
                PermintaanId = 10,
                JumlahKeluar = 5,
                TanggalKeluar = new DateTime(2025, 1, 1)
            };

            var keluar2 = new BarangKeluar
            {
                Id = 101,
                BarangId = 1,
                PermintaanId = 10,
                JumlahKeluar = 3,
                TanggalKeluar = new DateTime(2025, 1, 5)
            };

            db.Barangs.Add(barang);
            db.PermintaanBarangs.Add(permintaan);
            db.BarangKeluars.AddRange(keluar1, keluar2);
            db.SaveChanges();

            return db;
        }

        private BarangKeluarService GetService(ApplicationDbContext db)
        {
            return new BarangKeluarService(db);
        }

        // ================================================
        // TEST 1: GetAllAsync
        // ================================================
        [Fact]
        public async Task GetAllAsync_Should_Return_All_Data()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var result = await service.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.True(result[0].TanggalKeluar > result[1].TanggalKeluar); // Ordered desc
        }

        // ================================================
        // TEST 2: GetByIdAsync
        // ================================================
        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Data()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var result = await service.GetByIdAsync(100);

            Assert.NotNull(result);
            Assert.Equal(5, result.JumlahKeluar);
            Assert.Equal(1, result.BarangId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_If_NotFound()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var result = await service.GetByIdAsync(999);

            Assert.Null(result);
        }

        // ================================================
        // TEST 3: GetByPermintaanAsync
        // ================================================
        [Fact]
        public async Task GetByPermintaanAsync_Should_Return_Filtered_Data()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var list = await service.GetByPermintaanAsync(10);

            Assert.Equal(2, list.Count);
            Assert.All(list, x => Assert.Equal(10, x.PermintaanId));
        }

        [Fact]
        public async Task GetByPermintaanAsync_Should_Return_Empty_When_None()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var result = await service.GetByPermintaanAsync(999);

            Assert.Empty(result);
        }

        // ================================================
        // TEST 4: GetByBarangAsync
        // ================================================
        [Fact]
        public async Task GetByBarangAsync_Should_Return_Correct_Data()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var result = await service.GetByBarangAsync(1);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByBarangAsync_Should_Return_Empty_When_NotFound()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var result = await service.GetByBarangAsync(999);

            Assert.Empty(result);
        }
    }
}