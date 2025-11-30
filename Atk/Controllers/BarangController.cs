using Atk.DTOs;
using Atk.DTOs.Barang;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BarangController : ControllerBase
    {
        private readonly IBarang _service;

        public BarangController(IBarang service)
        {
            _service = service;
        }

        // ================================
        // GET ALL
        // ================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        // ================================
        // GET BY ID
        // ================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var barang = await _service.GetByIdAsync(id);
            if (barang == null)
            {
                return NotFound(new { message = "ID tidak ditemukan" });
            }

            return Ok(barang);
        }

        // ================================
        // CREATE BULK (Admin Only)
        // ================================
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("barang_bulk_limit")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<BarangCreateDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(new { message = "Data barang tidak boleh kosong" });
            }

            var result = new List<BarangResponseDto>();

            foreach (var dto in dtos)
            {
                if (await _service.ExistsByName(dto.NamaBarang))
                {
                    return BadRequest(new { message = $"{dto.NamaBarang} sudah ada" });
                }

                var newBarang = await _service.CreateAsync(dto);
                result.Add(newBarang);
            }

            return Ok(result);
        }

        // ================================
        // UPDATE (Admin Only)
        // ================================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BarangUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
            {
                return NotFound(new { message = "Barang tidak ditemukan" });
            }

            return Ok(updated);
        }

        // ================================
        // DELETE (Admin Only)
        // ================================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Data tidak ditemukan atau gagal dihapus" });
            }

            return Ok(new { message = "Berhasil dihapus" });
        }
    }
}
