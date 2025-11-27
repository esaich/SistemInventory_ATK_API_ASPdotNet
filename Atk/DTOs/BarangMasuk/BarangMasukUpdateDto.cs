using System;
using System.ComponentModel.DataAnnotations;

namespace Atk.DTOs.BarangMasuk
{
    public class BarangMasukUpdateDto
    {
        [Required]
        public int BarangId { get; set; }

        public int? SupplierId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int JumlahMasuk { get; set; }

        [Required]
        public decimal HargaSatuan { get; set; }

        [Required]
        public DateTime TanggalMasuk { get; set; }
    }
}
