
using Atk.DTOs.Divisi;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class DivisiController : ControllerBase
    {
        private readonly IDivisi _service;
        public DivisiController(IDivisi service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create(DivisiCreateDto dto) =>
            Ok(await _service.CreateAsync(dto));



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DivisiCreateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
         public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok("Deleted");
        }
    }
}