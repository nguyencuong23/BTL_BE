using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.ViewModels
{
    public class BorrowRequestViewModel
    {
        [Required]
        public string BookId { get; set; }

        [Required]
        public string StudentId { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }
    }

    public class BookApiViewModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string image { get; set; }
        public int available { get; set; }
    }
}