using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atk.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")] 
    public class BarangKeluarController : ControllerBase
    {
        private readonly IBarangKeluar _service;
        public BarangKeluarController(IBarangKeluar service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task <IActionResult> GetById(int id)
        {
            var getId = await _service.GetByIdAsync(id);
            if(getId == null) 
                return NotFound(new{ message = "Barang id tidak ditemukan"});

            return Ok(getId);
        }

        [HttpGet("by-permintaan/{permintaanId}")]
        public async Task<IActionResult> GetByPermintaan(int permintaanId)
        {
            var list = await _service.GetByPermintaanAsync(permintaanId);
            return Ok(list);
        }

        [HttpGet("by-barang/{barangId}")]
        public async Task<IActionResult> GetByBarang(int barangId)
        {
            var list = await _service.GetByBarangAsync(barangId);
            return Ok(list);
        }
        
    }
}