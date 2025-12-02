using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Users;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class UserDivisiController : ControllerBase
    {
        private readonly IUserService _service;
        public UserDivisiController(IUserService service)
        {
            _service = service;
        } 

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new {message = "User Tidak Di temukan"});
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDivisiDto dto)
        {
            var newUser = await _service.CreateDivisiUserAsync(dto);
            return Ok(newUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDivisiDto dto)
        {
            var update = await _service.UpdateAsync(id,dto);
            if (!update)
                return NotFound(new {message = "Data Tidak Berhasil Di Update"});
            
            return Ok(new{ message = "User Berhasil Di Update" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var del = await _service.DeleteAsync(id);
            if (!del)
                return NotFound(new { message = "User Tidak Berhasil di Hapus" });

            return Ok(new { message = "User Berhasil di Hapus" });
        }
    }
}
