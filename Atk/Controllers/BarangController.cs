using Atk.DTOs;
using Atk.DTOs.Barang;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BarangController : ControllerBase
    {
        private readonly IBarang _service;
        private static DateTime _lastRequestTime = DateTime.MinValue;
        
        public BarangController(IBarang service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var barang = await _service.GetByIdAsync(id);
            if (barang == null)
            {
                return NotFound(new {message = "id tidak ditemukan"});
            }

            return Ok(barang);
        }

        [EnableRateLimiting("barang_bulk_limit")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<BarangCreateDto> dtos)
        {
             var now = DateTime.UtcNow;

            if ((now - _lastRequestTime).TotalMilliseconds < 500) // <0.5 detik?
            {
                return StatusCode(429, new { message = "Terlalu cepat, coba lagi." });
            }

            _lastRequestTime = now;

            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(new { message = "Data Barang tidak boleh kosong" });
            }

            var result = new List<BarangResponseDto>();

            foreach (var dto in dtos)
            {
                if (await _service.ExistsByName(dto.NamaBarang))
                {
                    return BadRequest(new {message = $"{dto.NamaBarang} sudah ada "});
                }
                var newBarang = await _service.CreateAsync(dto);
                
                result.Add(newBarang);
            }
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BarangUpdateDto dto)
        {
            var upt = await _service.UpdateAsync(id, dto);
            if (upt == null)
            {
                return NotFound(new {message = "Supplier Tidak Ditemukan"});
            }

            return Ok(upt);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var del = await _service.DeleteAsync(id);
            if (!del)
            {
                return NotFound(new {message = "Data Tidak Terhapus"});
            }
            return Ok(del);
        }

    }
    
}