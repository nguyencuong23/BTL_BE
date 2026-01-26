using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVienTruongHoc.Models.Library
{
    public enum LoanStatus
    {
        DangMuon = 1,
        DaTra = 2,
        QuaHan = 3
    }

    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn độc giả")]
        [Display(Name = "Độc giả")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Mã sách không được để trống")]
        [StringLength(7)]
        [Display(Name = "Sách")]
        public string BookId { get; set; } = null!;

        [Required(ErrorMessage = "Ngày mượn không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày mượn")]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày hẹn trả không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày hẹn trả")]
        public DateTime DueDate { get; set; } // set = BorrowDate + 14 khi tạo phiếu

        [DataType(DataType.Date)]
        [Display(Name = "Ngày trả")]
        public DateTime? ReturnDate { get; set; }

        [Range(0, 999999999, ErrorMessage = "Tiền phạt không hợp lệ")]
        [Display(Name = "Tiền phạt")]
        public decimal Fine { get; set; } = 0;

        [Required]
        [Display(Name = "Trạng thái")]
        public LoanStatus Status { get; set; } = LoanStatus.DangMuon;

        // Navigation
        public Users.User? User { get; set; }
        public Book? Book { get; set; }
    }
}