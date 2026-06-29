using Microsoft.EntityFrameworkCore;
using ql_sang_kien_kinh_nghiem.Models;

namespace ql_sang_kien_kinh_nghiem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Nganh> DbSetNganh { get; set; }
        public DbSet<TrinhDo> DbSetTrinhDo { get; set; }
        public DbSet<ChucVu> DbSetChucVu { get; set; }
        public DbSet<DonVi> DbSetDonVi { get; set; }
        public DbSet<CanBoGiangVien> DbSetCanBoGiangVien { get; set; }
        public DbSet<ChuDauTu> DbSetChuDauTu { get; set; }
        public DbSet<LinhVuc> DbSetLinhVuc { get; set; }
        public DbSet<HoiDong> DbSetHoiDong { get; set; }
        public DbSet<VaiTro> DbSetVaiTro { get; set; }
        public DbSet<TaiKhoan> DbSetTaiKhoan { get; set; }
        public DbSet<CBGVChucVuDonVi> DbSetCBGVChucVuDonVi { get; set; }
        public DbSet<CBGVTrinhDoNganh> DbSetCBGVTrinhDoNganh { get; set; }
        public DbSet<SangKien> DbSetSangKien { get; set; }
        public DbSet<LinhVucSangKien> DbSetLinhVucSangKien { get; set; }
        public DbSet<MonHoc> DbSetMonHoc { get; set; }
        public DbSet<MonHocSangKien> DbSetMonHocSangKien { get; set; }
        public DbSet<CBGVSangKien> DbSetCBGVSangKien { get; set; }
        public DbSet<CBGVVaiTroHoiDong> DbSetCBGVVaiTroHoiDong { get; set; }
        public DbSet<PhanHoi> DbSetPhanHoi { get; set; }
        public DbSet<DanhGia> DbSetDanhGia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // NGANH
            modelBuilder.Entity<Nganh>(e =>
            {
                e.ToTable("NGANH");
                e.HasKey(x => x.MaNganh);

                e.Property(x => x.MaNganh).HasColumnName("MA_NGANH");
                e.Property(x => x.TenNganh).HasColumnName("TEN_NGANH");
            });

            // TRINH_DO
            modelBuilder.Entity<TrinhDo>(e =>
            {
                e.ToTable("TRINH_DO");
                e.HasKey(x => x.MaTD);

                e.Property(x => x.MaTD).HasColumnName("MA_TD");
                e.Property(x => x.TenTD).HasColumnName("TEN_TD");
            });

            // CHUC_VU
            modelBuilder.Entity<ChucVu>(e =>
            {
                e.ToTable("CHUC_VU");
                e.HasKey(x => x.MaCV);

                e.Property(x => x.MaCV).HasColumnName("MA_CV");
                e.Property(x => x.TenCV).HasColumnName("TEN_CV");
            });

            // DON_VI
            modelBuilder.Entity<DonVi>(e =>
            {
                e.ToTable("DON_VI");
                e.HasKey(x => x.MaDV);

                e.Property(x => x.MaDV).HasColumnName("MA_DV");
                e.Property(x => x.TenDV).HasColumnName("TEN_DV");
                e.Property(x => x.Email).HasColumnName("EMAIL");
                e.Property(x => x.SoDienThoai).HasColumnName("SO_DIEN_THOAI");
                e.Property(x => x.MaDVCha).HasColumnName("MA_DV_CHA");

                e.HasOne(x => x.DonViCha)
                    .WithMany(x => x.DSDonViCon)
                    .HasForeignKey(x => x.MaDVCha)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // CBGV
            modelBuilder.Entity<CanBoGiangVien>(e =>
            {
                e.ToTable("CAN_BO_GIANG_VIEN");
                e.HasKey(x => x.MaCBGV);

                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.HoTen).HasColumnName("HO_TEN");
                e.Property(x => x.Email).HasColumnName("EMAIL");
                e.Property(x => x.SoDienThoai).HasColumnName("SO_DIEN_THOAI");
            });

            // CHU_DAU_TU
            modelBuilder.Entity<ChuDauTu>(e =>
            {
                e.ToTable("CHU_DAU_TU");
                e.HasKey(x => x.MaCDT);

                e.Property(x => x.MaCDT).HasColumnName("MA_CDT");
                e.Property(x => x.TenCDT).HasColumnName("TEN_CDT");
                e.Property(x => x.Email).HasColumnName("EMAIL");
                e.Property(x => x.SoDienThoai).HasColumnName("SO_DIEN_THOAI");
                e.Property(x => x.DiaChi).HasColumnName("DIA_CHI");
            });

            // LINH_VUC
            modelBuilder.Entity<LinhVuc>(e =>
            {
                e.ToTable("LINH_VUC");
                e.HasKey(x => x.MaLV);

                e.Property(x => x.MaLV).HasColumnName("MA_LV");
                e.Property(x => x.TenLV).HasColumnName("TEN_LV");
            });

            // HOI_DONG
            modelBuilder.Entity<HoiDong>(e =>
            {
                e.ToTable("HOI_DONG");
                e.HasKey(x => x.MaHD);

                e.Property(x => x.MaHD).HasColumnName("MA_HD");
                e.Property(x => x.TenHD).HasColumnName("TEN_HD");
                e.Property(x => x.CanCu).HasColumnName("CAN_CU").HasColumnType("NCLOB");
                e.Property(x => x.DieuKhoan).HasColumnName("DIEU_KHOAN").HasColumnType("NCLOB");
                e.Property(x => x.NgayLap).HasColumnName("NGAY_LAP");
                e.Property(x => x.NgayKetThuc).HasColumnName("NGAY_KET_THUC");
            });

            // VAI_TRO
            modelBuilder.Entity<VaiTro>(e =>
            {
                e.ToTable("VAI_TRO");
                e.HasKey(x => x.MaVT);

                e.Property(x => x.MaVT).HasColumnName("MA_VT");
                e.Property(x => x.TenVT).HasColumnName("TEN_VT");
            });

            // TAI_KHOAN
            modelBuilder.Entity<TaiKhoan>(e =>
            {
                e.ToTable("TAI_KHOAN");
                e.HasKey(x => x.MaTK);

                e.Property(x => x.MaTK).HasColumnName("MA_TK");
                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.TenDangNhap).HasColumnName("TEN_DANG_NHAP");
                e.Property(x => x.MatKhau).HasColumnName("MAT_KHAU");
                e.Property(x => x.Quyen).HasColumnName("QUYEN");
                e.Property(x => x.TrangThaiTK).HasColumnName("TRANG_THAI_TK");

                e.HasOne(x => x.CanBoGiangVien)
                    .WithOne(x => x.TaiKhoan)
                    .HasForeignKey<TaiKhoan>(x => x.MaCBGV)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CBGV_CHUC_VU_DON_VI
            modelBuilder.Entity<CBGVChucVuDonVi>(e =>
            {
                e.ToTable("CBGV_CHUC_VU_DON_VI");

                e.HasKey(x => new
                {
                    x.MaCBGV,
                    x.MaCV,
                    x.MaDV,
                    x.ThoiGianBatDau
                });

                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.MaCV).HasColumnName("MA_CV");
                e.Property(x => x.MaDV).HasColumnName("MA_DV");
                e.Property(x => x.ThoiGianBatDau).HasColumnName("THOI_GIAN_BAT_DAU");
                e.Property(x => x.ThoiGianKetThuc).HasColumnName("THOI_GIAN_KET_THUC");

                e.HasOne(x => x.CanBoGiangVien)
                    .WithMany(x => x.DSCBGVChucVuDonVi)
                    .HasForeignKey(x => x.MaCBGV);

                e.HasOne(x => x.ChucVu)
                    .WithMany(x => x.DSCBGVChucVuDonVi)
                    .HasForeignKey(x => x.MaCV);

                e.HasOne(x => x.DonVi)
                    .WithMany(x => x.DSCBGVChucVuDonVi)
                    .HasForeignKey(x => x.MaDV);
            });

            // CBGV_TRINH_DO_NGANH
            modelBuilder.Entity<CBGVTrinhDoNganh>(e =>
            {
                e.ToTable("CBGV_TRINH_DO_NGANH");

                e.HasKey(x => new { x.MaCBGV, x.MaTD, x.MaNganh });

                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.MaTD).HasColumnName("MA_TD");
                e.Property(x => x.MaNganh).HasColumnName("MA_NGANH");

                e.HasOne(x => x.CanBoGiangVien)
                    .WithMany(x => x.DSCBGVTrinhDoNganh)
                    .HasForeignKey(x => x.MaCBGV);

                e.HasOne(x => x.TrinhDo)
                    .WithMany(x => x.DSCBGVTrinhDoNganh)
                    .HasForeignKey(x => x.MaTD);

                e.HasOne(x => x.Nganh)
                    .WithMany(x => x.DSCBGVTrinhDoNganh)
                    .HasForeignKey(x => x.MaNganh);
            });

            // SANG_KIEN
            modelBuilder.Entity<SangKien>(e =>
            {
                e.ToTable("SANG_KIEN");
                e.HasKey(x => x.MaSK);

                e.Property(x => x.MaSK).HasColumnName("MA_SK");
                e.Property(x => x.MaCDT).HasColumnName("MA_CDT");
                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.TenSK).HasColumnName("TEN_SK");
                e.Property(x => x.NgayApDung).HasColumnName("NGAY_AP_DUNG");
                e.Property(x => x.ThucTrangTruoc).HasColumnName("THUC_TRANG_TRUOC").HasColumnType("NCLOB");
                e.Property(x => x.NoiDung).HasColumnName("NOI_DUNG").HasColumnType("NCLOB");
                e.Property(x => x.KhaNangApDung).HasColumnName("KHA_NANG_AP_DUNG").HasColumnType("NCLOB");
                e.Property(x => x.LoiIchDuKien).HasColumnName("LOI_ICH_DU_KIEN").HasColumnType("NCLOB");
                e.Property(x => x.DieuKienCanThiet).HasColumnName("DIEU_KIEN_CAN_THIET").HasColumnType("NCLOB");
                e.Property(x => x.ThongTinBaoMat).HasColumnName("THONG_TIN_BAO_MAT").HasColumnType("NCLOB");
                e.Property(x => x.NgayNop).HasColumnName("NGAY_NOP");
                e.Property(x => x.NgayYeuCauChinhSua).HasColumnName("NGAY_YEU_CAU_CHINH_SUA");
                e.Property(x => x.NgayChapNhanDanhGia).HasColumnName("NGAY_CHAP_NHAN_DANH_GIA");
                e.Property(x => x.NgayTraKetQuaDanhGia).HasColumnName("NGAY_TRA_KET_QUA_DANH_GIA");
                e.Property(x => x.NgayYeuCauPhucKhao).HasColumnName("NGAY_YEU_CAU_PHUC_KHAO");
                e.Property(x => x.NgayChapNhanPhucKhao).HasColumnName("NGAY_CHAP_NHAN_PHUC_KHAO");
                e.Property(x => x.NgayTraKetQuaPhucKhao).HasColumnName("NGAY_TRA_KET_QUA_PHUC_KHAO");
                e.Property(x => x.UrlTepDinhKem).HasColumnName("URL_TEP_DINH_KEM");
                e.Property(x => x.UrlTepSK).HasColumnName("URL_TEP_SK");
                e.Property(x => x.TrangThaiSK).HasColumnName("TRANG_THAI_SANG_KIEN");

                e.HasOne(x => x.ChuDauTu)
                    .WithMany(x => x.DSSangKien)
                    .HasForeignKey(x => x.MaCDT)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(x => x.CanBoGiangVien)
                    .WithMany(x => x.DSSangKien)
                    .HasForeignKey(x => x.MaCBGV);
            });

            // LINH_VUC_SANG_KIEN
            modelBuilder.Entity<LinhVucSangKien>(e =>
            {
                e.ToTable("LINH_VUC_SANG_KIEN");
                e.HasKey(x => new { x.MaLV, x.MaSK });

                e.Property(x => x.MaLV).HasColumnName("MA_LV");
                e.Property(x => x.MaSK).HasColumnName("MA_SK");

                e.HasOne(x => x.LinhVuc)
                    .WithMany(x => x.DSLinhVucSangKien)
                    .HasForeignKey(x => x.MaLV);

                e.HasOne(x => x.SangKien)
                    .WithMany(x => x.DSLinhVucSangKien)
                    .HasForeignKey(x => x.MaSK);
            });

            // MON_HOC
            modelBuilder.Entity<MonHoc>(e =>
            {
                e.ToTable("MON_HOC");
                e.HasKey(x => x.MaMH);

                e.Property(x => x.MaMH).HasColumnName("MA_MH");
                e.Property(x => x.MaNganh).HasColumnName("MA_NGANH");
                e.Property(x => x.TenMH).HasColumnName("TEN_MH");
                e.Property(x => x.MoTaMH).HasColumnName("MO_TA_MH").HasColumnType("NCLOB");
                e.Property(x => x.CumNguNghia).HasColumnName("CUM_NGU_NGHIA").HasColumnType("NCLOB");

                e.HasOne(x => x.Nganh)
                    .WithMany(x => x.DSMonHoc)
                    .HasForeignKey(x => x.MaNganh);
            });

            // MON_HOC_SANG_KIEN
            modelBuilder.Entity<MonHocSangKien>(e =>
            {
                e.ToTable("MON_HOC_SANG_KIEN");
                e.HasKey(x => new { x.MaMH, x.MaSK });

                e.Property(x => x.MaMH).HasColumnName("MA_MH");
                e.Property(x => x.MaSK).HasColumnName("MA_SK");

                e.HasOne(x => x.MonHoc)
                    .WithMany(x => x.DSMonHocSangKien)
                    .HasForeignKey(x => x.MaMH);

                e.HasOne(x => x.SangKien)
                    .WithMany(x => x.DSMonHocSangKien)
                    .HasForeignKey(x => x.MaSK);
            });

            // CBGV_SANG_KIEN
            modelBuilder.Entity<CBGVSangKien>(e =>
            {
                e.ToTable("CBGV_SANG_KIEN");

                e.HasKey(x => new { x.MaCBGV, x.MaSK });

                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.MaSK).HasColumnName("MA_SK");
                e.Property(x => x.TiLeDongGop).HasColumnName("TI_LE_DONG_GOP");
                e.Property(x => x.GhiChu).HasColumnName("GHI_CHU");

                e.HasOne(x => x.CanBoGiangVien)
                    .WithMany(x => x.DSCBGVSangKien)
                    .HasForeignKey(x => x.MaCBGV);

                e.HasOne(x => x.SangKien)
                    .WithMany(x => x.DSCBGVSangKien)
                    .HasForeignKey(x => x.MaSK);
            });

            // CBGV_VAI_TRO_HOI_DONG
            modelBuilder.Entity<CBGVVaiTroHoiDong>(e =>
            {
                e.ToTable("CBGV_VAI_TRO_HOI_DONG");

                e.HasKey(x => new { x.MaCBGV, x.MaHD, x.MaVT });

                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.MaHD).HasColumnName("MA_HD");
                e.Property(x => x.MaVT).HasColumnName("MA_VT");

                e.HasOne(x => x.CanBoGiangVien)
                    .WithMany(x => x.DSCBGVVaiTroHoiDong)
                    .HasForeignKey(x => x.MaCBGV);

                e.HasOne(x => x.HoiDong)
                    .WithMany(x => x.DSCBGVVaiTroHoiDong)
                    .HasForeignKey(x => x.MaHD);

                e.HasOne(x => x.VaiTro)
                    .WithMany(x => x.DSCBGVVaiTroHoiDong)
                    .HasForeignKey(x => x.MaVT);
            });

            // DANH_GIA
            modelBuilder.Entity<DanhGia>(e =>
            {
                e.ToTable("DANH_GIA");

                e.HasKey(x => new { x.MaSK, x.MaHD, x.MaCBGV, x.LanDG });

                e.Property(x => x.MaSK).HasColumnName("MA_SK");
                e.Property(x => x.MaHD).HasColumnName("MA_HD");
                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.YKienNhanXet).HasColumnName("Y_KIEN_NHAN_XET").HasColumnType("NCLOB");
                e.Property(x => x.CongNhan).HasColumnName("CONG_NHAN");
                e.Property(x => x.LanDG).HasColumnName("LAN_DG");
                e.Property(x => x.NgayDG).HasColumnName("NGAY_DG");

                e.HasOne(x => x.SangKien)
                    .WithMany(x => x.DSDanhGia)
                    .HasForeignKey(x => x.MaSK);

                e.HasOne(x => x.HoiDong)
                    .WithMany(x => x.DSDanhGia)
                    .HasForeignKey(x => x.MaHD);

                e.HasOne(x => x.CanBoGiangVien)
                    .WithMany(x => x.DSDanhGia)
                    .HasForeignKey(x => x.MaCBGV);
            });

            // PHAN_HOI
            modelBuilder.Entity<PhanHoi>(e =>
            {
                e.ToTable("PHAN_HOI");

                e.HasKey(x => new { x.MaCBGV, x.MaSK, x.NgayGui });

                e.Property(x => x.MaCBGV).HasColumnName("MA_CBGV");
                e.Property(x => x.MaSK).HasColumnName("MA_SK");
                e.Property(x => x.NoiDung).HasColumnName("NOI_DUNG").HasColumnType("NCLOB");
                e.Property(x => x.NgayGui).HasColumnName("NGAY_GUI");

                e.HasOne(x => x.CanBoGiangVien)
                    .WithMany(x => x.DSPhanHoi)
                    .HasForeignKey(x => x.MaCBGV);

                e.HasOne(x => x.SangKien)
                    .WithMany(x => x.DSPhanHoi)
                    .HasForeignKey(x => x.MaSK);
            });
        }
    }
}
