using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Supplier;
using Atk.Services.Implementations;
using Atk.Services.Interfaces;
using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierServices _service;
        private static DateTime _lastRequestTime = DateTime.MinValue;

        public SupplierController(ISupplierServices services)
        {
            _service = services;
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
            var supplier = await _service.GetByIdAsync(id);

            if (supplier == null)
            {
                return NotFound(new{ message = "Supplier Tidak ditemukan" });
            }
            return Ok(supplier);
        }

        [EnableRateLimiting("supplier_bulk_limit")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<SupplierCreateDto> dtos)
        {
            var now = DateTime.UtcNow;

            if ((now - _lastRequestTime).TotalMilliseconds < 500) // <0.5 detik?
            {
                return StatusCode(429, new { message = "Terlalu cepat, coba lagi." });
            }

            _lastRequestTime = now;

            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(new { message = "Data supplier tidak boleh kosong" });
            }

            var result = new List<SupplierResponseDto>();

            foreach (var dto in dtos)
            {
                // validasi duplikat
                if (await _service.ExistsByName(dto.NamaSupplier))
                {
                    return BadRequest(new {message = $"{dto.NamaSupplier } sudah ada "});
                }
                var newSupplier = await _service.CreateAsync(dto);
                
                result.Add(newSupplier);
            }

            return Ok(result);
        }



        [HttpPut("{id:int}")]

        public async Task<IActionResult> Update(int id, [FromBody] SupplierUpdateDto dto )
        {
            var update = await _service.UpdateAsync(id, dto);

            if (update == null)
            {
                return NotFound(new {message = "Supplier Tidak Ditemukan"});
            }

            return Ok(update);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var delete = await _service.DeleteAsync(id);

            if (!delete)
            {
                return NotFound(new {message = "Supplier Tidak Ditemukan"});
            }
            return Ok(delete);
        }
    }
    
}