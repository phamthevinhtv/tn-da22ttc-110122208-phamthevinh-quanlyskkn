using System.Text.RegularExpressions;

namespace ql_sang_kien_kinh_nghiem.Services;

public class XuLyVanBanService
{
    private readonly HashSet<string> StopwordsBien = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public XuLyVanBanService()
    {
        string thuMucDuAnGoc = Directory.GetCurrentDirectory();
        string duongDanFile = Path.Combine(thuMucDuAnGoc, "Data", "stopwords.txt");
        
        TaiStopwordsTuFile(duongDanFile);
    }

    public void TaiStopwordsTuFile(string duongDanFile)
    {
        if (!File.Exists(duongDanFile)) return;

        StopwordsBien.Clear();
        string[] cacDong = File.ReadAllLines(duongDanFile);
        foreach (var dong in cacDong)
        {
            string sw = dong.Trim().Trim('\uFEFF').ToLower(); 
            if (!string.IsNullOrWhiteSpace(sw))
            {
                StopwordsBien.Add(sw);
            }
        }
    }

    public string[] TachVeCau(string vanBanTho)
    {
        if (string.IsNullOrWhiteSpace(vanBanTho))
            return Array.Empty<string>();

        string vanBanSach = vanBanTho.Replace("\r\n", "\n").Replace("\r", "\n");
        vanBanSach = Regex.Replace(vanBanSach, @"(?<=^|\s|\n)(\d+\.|\w\.|\w\)|[\-\+\*])\s+", " ");

        vanBanSach = vanBanSach
            .Replace("(", ", ").Replace(")", ", ")
            .Replace("[", ", ").Replace("]", ", ")
            .Replace("{", ", ").Replace("}", ", ")
            .Replace("\"", ", ").Replace("'", ", ")
            .Replace("“", ", ").Replace("”", ", ") 
            .Replace("/", ", ")
            .Replace(" - ", ", ");

        vanBanSach = Regex.Replace(vanBanSach, @"([.,?!;:…\n]+)(?=\s|$|\p{Lu}\p{Ll}|\p{Lu}\s)", "|");
        string[] cacVeTho = vanBanSach.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        List<string> danhSachVeSach = new List<string>();
        foreach (var ve in cacVeTho)
        {
            string veSach = Regex.Replace(ve.Trim(), @"\s+", " ");
            if (!string.IsNullOrWhiteSpace(veSach) && veSach.Length >= 2)
            {
                danhSachVeSach.Add(veSach);
            }
        }
        return danhSachVeSach.ToArray();
    }

    public List<string> SinhCumTuTho(string[] danhSachVeSach)
    {
        List<string> danhSachCumTu = new List<string>();
        foreach (var ve in danhSachVeSach)
        {
            if (string.IsNullOrWhiteSpace(ve)) continue;

            string[] cacTuDon = ve.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int soTu = cacTuDon.Length;

            if (soTu < 2) continue;

            for (int soGram = 2; soGram <= 4; soGram++)
            {
                for (int i = 0; i <= soTu - soGram; i++)
                {
                    string[] cuaSoHienTai = new string[soGram];
                    Array.Copy(cacTuDon, i, cuaSoHienTai, 0, soGram);
                    string cumTuGhep = string.Join(" ", cuaSoHienTai);
                    danhSachCumTu.Add(cumTuGhep);
                }
            }
        }
        return danhSachCumTu;
    }

    public List<string> LocBienCumTu(List<string> danhSachCumTu)
    {
        if (danhSachCumTu == null || danhSachCumTu.Count == 0)
            return new List<string>();

        List<string> danhSachDaLoc = new List<string>();

        foreach (var cumTu in danhSachCumTu)
        {
            if (string.IsNullOrWhiteSpace(cumTu)) continue;

            string[] cacTu = cumTu.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int soTu = cacTu.Length;
            if (soTu == 0) continue;

            bool biLoai = false;

            string cumKiemTraDau = "";
            for (int i = 0; i < soTu; i++)
            {
                cumKiemTraDau = (i == 0) ? cacTu[i] : cumKiemTraDau + " " + cacTu[i];
                if (StopwordsBien.Contains(cumKiemTraDau))
                {
                    biLoai = true;
                    break;
                }
            }

            if (biLoai) continue;

            string cumKiemTraCuoi = "";
            for (int i = soTu - 1; i >= 0; i--)
            {
                cumKiemTraCuoi = (i == soTu - 1) ? cacTu[i] : cacTu[i] + " " + cumKiemTraCuoi;
                if (StopwordsBien.Contains(cumKiemTraCuoi))
                {
                    biLoai = true;
                    break;
                }
            }

            if (biLoai) continue;

            danhSachDaLoc.Add(cumTu);
        }

        return danhSachDaLoc; 
    }

    public List<string> XuLyCumChaCon(List<string> danhSachCumDaLocBien)
    {
        if (danhSachCumDaLocBien == null || danhSachCumDaLocBien.Count == 0) 
            return new List<string>();

        var dataDict = danhSachCumDaLocBien
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        var danhSachSapXep = dataDict.OrderByDescending(x => x.Key.Length).ToList();
        var listXoa = new HashSet<string>();

        for (int i = 0; i < danhSachSapXep.Count; i++)
        {
            string cha = danhSachSapXep[i].Key;
            if (listXoa.Contains(cha)) continue;

            for (int j = i + 1; j < danhSachSapXep.Count; j++)
            {
                string con = danhSachSapXep[j].Key;
                if (listXoa.Contains(con)) continue;

                if ($" {cha} ".Contains($" {con} "))
                {
                    if (dataDict[con] == dataDict[cha])
                    {
                        listXoa.Add(con);
                    }
                }
            }
        }

        return danhSachSapXep
            .Where(x => !listXoa.Contains(x.Key))
            .Select(x => new { 
                CumTu = x.Key, 
                SoLuongTu = x.Key.Split(' ').Length,
                TanSuat = dataDict[x.Key]
            })
            .OrderBy(x => x.SoLuongTu)          
            .ThenByDescending(x => x.TanSuat)
            .Select(x => x.CumTu)
            .ToList();
    }

    public List<string> TaoDanhSachCumTuTimKiem(string vanBanTho)
    {
        if (string.IsNullOrWhiteSpace(vanBanTho))
            return new List<string>();

        string[] danhSachVe = TachVeCau(vanBanTho);
        if (danhSachVe.Length == 0)
            return new List<string>();

        List<string> danhSachCum = SinhCumTuTho(danhSachVe);
        List<string> danhSachCumLocBien = LocBienCumTu(danhSachCum);
        List<string> danhSachCumDaXuLy = XuLyCumChaCon(danhSachCumLocBien);

        if (danhSachCumDaXuLy.Any())
            return danhSachCumDaXuLy;

        var danhSachTuDon = danhSachVe
            .SelectMany(ve => ve.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            .Where(tu => tu.Length >= 2 && !StopwordsBien.Contains(tu))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(tu => tu.Length)
            .ToList();

        return danhSachTuDon;
    }
}