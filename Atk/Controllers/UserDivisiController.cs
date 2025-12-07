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

       // File: UserDivisiController.cs (REVISI AKHIR)

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Panggil service. isSuccess akan bernilai TRUE (sukses) atau FALSE (gagal).
            var isSuccess = await _service.DeleteAsync(id); 

            if (!isSuccess)
            {
                // 2. Jika GAGAL, kita periksa apakah user tersebut benar-benar ada.
                var userExists = await _service.GetByIdAsync(id);

                if (userExists == null)
                {
                    // Jika isSuccess FALSE DAN user tidak ada di database:
                    // User sudah hilang, atau ID salah, jadi 404.
                    return NotFound(new { message = "User gagal dihapus karena tidak ditemukan." }); // 404 Not Found
                }
                else
                {
                    // Jika isSuccess FALSE TAPI user MASIH ada di database:
                    // Berarti database menolak penghapusan (yaitu Foreign Key Constraint). 409 Conflict.
                    return Conflict(new 
                    { 
                        message = "Gagal menghapus User. User ini masih memiliki data terkait (relasi) di sistem." 
                    }); // 409 Conflict
                }
            }

            // 3. Jika isSuccess TRUE:
            return Ok(new { message = "User Berhasil dihapus" }); // 200 OK
        }
    }
}
