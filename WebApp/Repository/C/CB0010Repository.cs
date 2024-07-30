using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using BarcodeLib;

namespace WebApp.Repository.CB
{
    public class CB0010Repository : JCLib.Mvc.BaseRepository
    {
        public CB0010Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_STLOC> GetAll(string WH_NO, string STORE_LOC, string FLAG, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * from BC_STLOC
                        where  WH_NO=:p0 
                        ";

            p.Add(":p0", string.Format("{0}", WH_NO));

            if (STORE_LOC != "")
            {
                sql += " AND STORE_LOC like :p1 ";
                p.Add(":p1", string.Format("{0}%", STORE_LOC));
            }
            if (FLAG != "")
            {
                sql += " AND FLAG=:p2 ";
                p.Add(":p2", string.Format("{0}", FLAG));
            }

            sql += " order by wh_no, store_loc ";



            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_STLOC>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BC_STLOC> Get(string wh_no, string store_loc)
        {
            var sql = @"SELECT * FROM BC_STLOC WHERE WH_NO = :WH_NO AND STORE_LOC = :STORE_LOC";
            return DBWork.Connection.Query<BC_STLOC>(sql, new { WH_NO = wh_no, STORE_LOC = store_loc }, DBWork.Transaction);
        }


        public int Create(BC_STLOC bC_STLOC)
        {
            var sql = @"INSERT INTO mmsadm.bc_stloc (
                        wh_no,    store_loc,    barcode,    xcategory,    create_user,    create_time,
                        update_ip,    memo,    flag )
                    VALUES(
                         :wh_no,   :store_loc,   :barcode,   :xcategory,   :create_user,   sysdate,
                         :update_ip,   :memo,   :flag
                    ) ";


            return DBWork.Connection.Execute(sql, bC_STLOC, DBWork.Transaction);
        }

        public int Update(BC_STLOC bC_STLOC)
        {
            var sql = @"UPDATE BC_STLOC 
                        SET WH_NO=:WH_NO,STORE_LOC=:STORE_LOC,BARCODE=:BARCODE , XCATEGORY = :XCATEGORY , FLAG = :FLAG ,MEMO=:MEMO,
                            UPDATE_USER = :UPDATE_USER , UPDATE_TIME = SYSDATE , UPDATE_IP = :UPDATE_IP
                        WHERE WH_NO = :WH_NO AND STORE_LOC = :STORE_LOC";

            return DBWork.Connection.Execute(sql, bC_STLOC, DBWork.Transaction);
        }
        public int Delete(BC_STLOC bC_STLOC)
        {
            var sql = @"Delete BC_STLOC
                        WHERE WH_NO = :WH_NO AND STORE_LOC = :STORE_LOC";

            return DBWork.Connection.Execute(sql, bC_STLOC, DBWork.Transaction);
        }
        
        public bool CheckExists(BC_STLOC bC_STLOC)
        {
            string sql = @"SELECT 1 FROM BC_STLOC WHERE WH_NO=:WH_NO AND STORE_LOC= :STORE_LOC";

            return !(DBWork.Connection.ExecuteScalar(sql, bC_STLOC, DBWork.Transaction)==null);
        }

        public bool CheckExistsMI_WLOCINV(BC_STLOC bC_STLOC)
        {
            string sql = @"select 1 from MI_WLOCINV 
                         Where  WH_NO=:WH_NO AND STORE_LOC= :STORE_LOC";

            return (DBWork.Connection.ExecuteScalar(sql, bC_STLOC, DBWork.Transaction)) == null;
        }


        public IEnumerable<ComboItemModel> GetXcategoryCombo()
        {
            string sql = @"select descript as TEXT, xcategory as VALUE  from bc_category";
            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }


        // 報表
        public IEnumerable<BC_STLOC> GetSmData(string WH_NO, string STORE_LOC, string FLAG)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * from BC_STLOC
                        where  WH_NO=:p0 
                        ";

            p.Add(":p0", string.Format("{0}", WH_NO));

            if (STORE_LOC != "")
            {
                sql += " AND STORE_LOC like :p1 ";
                p.Add(":p1", string.Format("{0}%", STORE_LOC));
            }
            if (FLAG != "")
            {
                sql += " AND FLAG=:p2 ";
                p.Add(":p2", string.Format("{0}", FLAG));
            }

            sql += " order by wh_no, store_loc ";



            Barcode b = new Barcode();
            b.IncludeLabel = true;
            //bool isTypeError = false;

            List<BC_STLOC> list = new List<BC_STLOC>();
            IEnumerable<BC_STLOC> temp_list = DBWork.Connection.Query<BC_STLOC>(sql, p, DBWork.Transaction);
            foreach (BC_STLOC _BC_STLOC in temp_list)
            {
                BarcodeLib.TYPE type = BarcodeLib.TYPE.UNSPECIFIED;
                switch (_BC_STLOC.XCATEGORY)
                {
                    case "UPCA": type = BarcodeLib.TYPE.UPCA; break;
                    case "UPCE": type = BarcodeLib.TYPE.UPCE; break;
                    case "UPC 2 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_2DIGIT; break;
                    case "UPC 5 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_5DIGIT; break;
                    case "EAN13": type = BarcodeLib.TYPE.EAN13; break;
                    case "JAN-13": type = BarcodeLib.TYPE.JAN13; break;
                    case "EAN8": type = BarcodeLib.TYPE.EAN8; break;
                    case "ITF-14": type = BarcodeLib.TYPE.ITF14; break;
                    case "Codabar": type = BarcodeLib.TYPE.Codabar; break;
                    case "PostNet": type = BarcodeLib.TYPE.PostNet; break;
                    case "Bookland/ISBN": type = BarcodeLib.TYPE.BOOKLAND; break;
                    case "Code 11": type = BarcodeLib.TYPE.CODE11; break;
                    case "CODE39": type = BarcodeLib.TYPE.CODE39; break;
                    case "Code 39 Extended": type = BarcodeLib.TYPE.CODE39Extended; break;
                    case "Code 39 Mod 43": type = BarcodeLib.TYPE.CODE39_Mod43; break;
                    case "CODE93": type = BarcodeLib.TYPE.CODE93; break;
                    case "LOGMARS": type = BarcodeLib.TYPE.LOGMARS; break;
                    case "MSI": type = BarcodeLib.TYPE.MSI_Mod10; break;
                    case "Interleaved 2 of 5": type = BarcodeLib.TYPE.Interleaved2of5; break;
                    case "Standard 2 of 5": type = BarcodeLib.TYPE.Standard2of5; break;
                    case "CODE128": type = BarcodeLib.TYPE.CODE128; break;
                    case "Code 128-A": type = BarcodeLib.TYPE.CODE128A; break;
                    case "Code 128-B": type = BarcodeLib.TYPE.CODE128B; break;
                    case "Code 128-C": type = BarcodeLib.TYPE.CODE128C; break;
                    case "Telepen": type = BarcodeLib.TYPE.TELEPEN; break;
                    case "FIM": type = BarcodeLib.TYPE.FIM; break;
                    case "Pharmacode": type = BarcodeLib.TYPE.PHARMACODE; break;
                    default: type = BarcodeLib.TYPE.CODE128; break;
                }//switch

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_BC_STLOC.BARCODE))
                        { _BC_STLOC.BARCODE_IMAGE_STR = ""; }
                        else
                        {
                            Bitmap image = (Bitmap)b.Encode(type, _BC_STLOC.BARCODE,150,50);


                            image.Save(ms, ImageFormat.Bmp);
                            byte[] byteImage = new Byte[ms.Length];
                            byteImage = ms.ToArray();
                            string strB64 = Convert.ToBase64String(byteImage);
                            _BC_STLOC.BARCODE_IMAGE_STR = strB64;
                        }
                    }
                    catch (FormatException ex)
                    {
                        _BC_STLOC.BARCODE_IMAGE_STR = "";
                    }
                }

                list.Add(_BC_STLOC);
            }

            return list;
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE
                        FROM MI_WHMAST A
                        WHERE INID=:inid";

            p.Add(":inid", DBWork.UserInfo.Inid);


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":WH_NO_I", p0);
                p.Add(":WH_NAME_I", p0);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", p0));

                sql += " OR A.WH_NAME LIKE :WH_NAME) ";
                p.Add(":WH_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.WH_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }
        public DataTable GetExcel(string WH_NO, string STORE_LOC, string FLAG)//查詢
        {
            var p = new DynamicParameters();
            var sql = @"SELECT WH_NO as 庫房代碼,
                        STORE_LOC as 儲位代碼,
                        BARCODE as 儲位條碼,
                        case when FLAG='Y' then '是' when FLAG='Y' then '否' else '' end  as 電子儲位,
                        XCATEGORY as 條碼分類代碼 from BC_STLOC
                        where  WH_NO=:p0  ";
            p.Add(":p0", string.Format("{0}", WH_NO));
            if (STORE_LOC != "")
            {
                sql += " AND STORE_LOC like :p1 ";
                p.Add(":p1", string.Format("{0}%", STORE_LOC));
            }
            if (FLAG != "")
            {
                sql += " AND FLAG=:p2 ";
                p.Add(":p2", string.Format("{0}", FLAG));
            }
            sql += " order by wh_no, store_loc ";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);
            return dt;
        }
    }
}