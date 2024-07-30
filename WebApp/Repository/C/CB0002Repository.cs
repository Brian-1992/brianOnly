using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp.Repository.C
{
    public class CB0002Repository : JCLib.Mvc.BaseRepository
    {
        public CB0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<CB0002M> GetAllM(string MAT_CLASS, string MMCODE, string MMNAME_C, string STATUS, string BARCODE, string MMNAME_E, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT (SELECT MAT_CLSNAME
                                FROM MI_MATCLASS
                                WHERE MAT_CLASS = MMT.MAT_CLASS) MAT_CLASS,
                               BBE.MMCODE,
                               MMT.MMNAME_C,
                               MMT.MMNAME_E,
                               BARCODE, TRATIO, STATUS
                        FROM BC_BARCODE BBE, MI_MAST MMT
                        WHERE BBE.MMCODE = MMT.MMCODE ";

            if (MAT_CLASS != "")
            {
                sql += @" AND MMT.MAT_CLASS = :p0 ";
                p.Add(":p0", string.Format("{0}", MAT_CLASS));
            }

            if (MMCODE != "")
            {
                sql += @" AND BBE.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", MMCODE));
            }

            if (MMNAME_C != "")
            {
                sql += @" AND MMT.MMNAME_C LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", MMNAME_C));
            }
            if (STATUS != "")
            {
                sql += @" AND STATUS=:STATUS ";
                p.Add(":STATUS", STATUS);
            }
            if (BARCODE != "")
            {
                sql += @" AND BBE.BARCODE LIKE :p4 ";
                p.Add(":p4", string.Format("{0}%", BARCODE));
            }

            if (MMNAME_E != "")
            {
                sql += @" AND MMT.MMNAME_E LIKE :p5 ";
                p.Add(":p5", string.Format("{0}%", MMNAME_E));
            }

            sql += @" ORDER BY  MAT_CLASS,MMT.MMCODE";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CB0002M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //新增
        public int Create(CB0002M BC_BARCODE)
        {
            var sql = @"INSERT INTO BC_BARCODE (
                           MMCODE, BARCODE, STATUS , TRATIO , CREATE_TIME,
                           CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        VALUES (
                           :MMCODE, :BARCODE, :STATUS , :TRATIO ,  SYSDATE,
                           :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, BC_BARCODE, DBWork.Transaction);
        }

        //修改
        public int Update(CB0002M BC_BARCODE)
        {
            var sql = @"UPDATE BC_BARCODE SET 
                           BARCODE = :BARCODE,
                           STATUS = :STATUS, TRATIO = :TRATIO, 
                           UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE MMCODE = :MMCODE AND BARCODE = :BARCODE_OLD ";
            return DBWork.Connection.Execute(sql, BC_BARCODE, DBWork.Transaction);
        }
        //刪除
        public int Delete(CB0002M BC_BARCODE)
        {
            var sql = @"delete BC_BARCODE 
                        WHERE MMCODE = :MMCODE AND BARCODE = :BARCODE_OLD ";
            return DBWork.Connection.Execute(sql, BC_BARCODE, DBWork.Transaction);
        }
        public IEnumerable<CB0002M> Get(string mmcode, string barcode)
        {
            var sql = @"SELECT (SELECT MAT_CLSNAME
                                FROM MI_MATCLASS
                                WHERE MAT_CLASS = MMT.MAT_CLASS) MAT_CLASS,
                               MMT.MAT_CLASS MAT_CLASS_CODE,
                               BBE.MMCODE,
                               MMT.MMNAME_C,
                               MMT.MMNAME_E,
                               BARCODE, TRATIO, STATUS
                        FROM BC_BARCODE BBE, MI_MAST MMT
                        WHERE BBE.MMCODE = MMT.MMCODE 
                        AND BBE.MMCODE =:MMCODE AND BBE.BARCODE = :BARCODE ";
            return DBWork.Connection.Query<CB0002M>(sql, new { MMCODE = mmcode, BARCODE = barcode}, DBWork.Transaction);
        }
        public bool CheckExists(CB0002M BC_BARCODE)
        {
            string sql = @"SELECT 1 FROM BC_BARCODE
                            WHERE BARCODE  = :BARCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, BC_BARCODE, DBWork.Transaction) == null);
        }
 
        public IEnumerable<ComboItemModel> GetCLSNAME()
        {
            string sql = @"  SELECT MAT_CLASS VALUE,
                                    MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                             FROM MI_MATCLASS where mat_class >='01' and  mat_class <='08'
                             ORDER BY MAT_CLASS";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<CB0002M> Print(string MMCODE, string MMNAME_C, string MMNAME_E, string BARCODE, string STATUS, string MAT_CLASS)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT (SELECT MAT_CLSNAME
                                        FROM MI_MATCLASS
                                        WHERE MAT_CLASS = MMT.MAT_CLASS) MAT_CLASS,
                                (SELECT MAT_CLSNAME
                                        FROM MI_MATCLASS MMS
                                        WHERE MMS.MAT_CLASS = MMT.MAT_CLASS) MAT_CLSNAME,
                                (SELECT AGEN_NO || '-' || AGEN_NAMEC
                                         FROM PH_VENDER PVR
                                         WHERE PVR.AGEN_NO = MMT.M_AGENNO) VENDER,
                               BBE.MMCODE,
                               MMT.MMNAME_C,
                               MMT.MMNAME_E,
                               BBE.BARCODE                            
                        FROM BC_BARCODE BBE, MI_MAST MMT
                        WHERE BBE.MMCODE = MMT.MMCODE
                        ";

            if (MMCODE != "")
            {
                sql += @" AND BBE.MMCODE LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", MMCODE));
            }

            if (MMNAME_C != "")
            {
                sql += @" AND MMT.MMNAME_C LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", MMNAME_C));
            }

            if (MMNAME_E != "")
            {
                sql += @" AND MMT.MMNAME_E LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", MMNAME_E));
            }
            if (BARCODE != "")
            {
                sql += @" AND BBE.BARCODE LIKE :p3 ";
                p.Add(":p3", string.Format("{0}%", BARCODE));
            }
            if (STATUS != "")
            {
                sql += @" AND STATUS=:STATUS ";
                p.Add(":STATUS", STATUS);
            }
            if (MAT_CLASS != "")
            {
                sql += @" AND MMT.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            sql += @" ORDER BY  MMT.MAT_CLASS,MMT.MMCODE";

            Barcode b = new Barcode();
            b.IncludeLabel = true;

            List<CB0002M> list = new List<CB0002M>();
            foreach (CB0002M _CB0002M in DBWork.Connection.Query<CB0002M>(sql, p, DBWork.Transaction))
            {
                BarcodeLib.TYPE type = BarcodeLib.TYPE.UNSPECIFIED;

                type = BarcodeLib.TYPE.CODE128;
                #region 條碼編碼
                //switch (_CB0002M.XCATEGORY)
                //{
                //    case "UPCA": type = BarcodeLib.TYPE.UPCA; break;
                //    case "UPCE": type = BarcodeLib.TYPE.UPCE; break;
                //    case "UPC 2 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_2DIGIT; break;
                //    case "UPC 5 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_5DIGIT; break;
                //    case "EAN13": type = BarcodeLib.TYPE.EAN13; break;
                //    case "JAN-13": type = BarcodeLib.TYPE.JAN13; break;
                //    case "EAN8": type = BarcodeLib.TYPE.EAN8; break;
                //    case "ITF-14": type = BarcodeLib.TYPE.ITF14; break;
                //    case "Codabar": type = BarcodeLib.TYPE.Codabar; break;
                //    case "PostNet": type = BarcodeLib.TYPE.PostNet; break;
                //    case "Bookland/ISBN": type = BarcodeLib.TYPE.BOOKLAND; break;
                //    case "Code 11": type = BarcodeLib.TYPE.CODE11; break;
                //    case "CODE39": type = BarcodeLib.TYPE.CODE39; break;
                //    case "Code 39 Extended": type = BarcodeLib.TYPE.CODE39Extended; break;
                //    case "Code 39 Mod 43": type = BarcodeLib.TYPE.CODE39_Mod43; break;
                //    case "CODE93": type = BarcodeLib.TYPE.CODE93; break;
                //    case "LOGMARS": type = BarcodeLib.TYPE.LOGMARS; break;
                //    case "MSI": type = BarcodeLib.TYPE.MSI_Mod10; break;
                //    case "Interleaved 2 of 5": type = BarcodeLib.TYPE.Interleaved2of5; break;
                //    case "Standard 2 of 5": type = BarcodeLib.TYPE.Standard2of5; break;
                //    case "CODE128": type = BarcodeLib.TYPE.CODE128; break;
                //    case "Code 128-A": type = BarcodeLib.TYPE.CODE128A; break;
                //    case "Code 128-B": type = BarcodeLib.TYPE.CODE128B; break;
                //    case "Code 128-C": type = BarcodeLib.TYPE.CODE128C; break;
                //    case "Telepen": type = BarcodeLib.TYPE.TELEPEN; break;
                //    case "FIM": type = BarcodeLib.TYPE.FIM; break;
                //    case "Pharmacode": type = BarcodeLib.TYPE.PHARMACODE; break;
                //    default: type = BarcodeLib.TYPE.CODE128; break;
                //}//switch
                #endregion

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_CB0002M.BARCODE))
                        { _CB0002M.BARCODE_IMAGE_STR = ""; }
                        else
                        {
                            Bitmap image = (Bitmap)b.Encode(type, _CB0002M.BARCODE,200,40);

                            image.Save(ms, ImageFormat.Bmp);
                            byte[] byteImage = new Byte[ms.Length];
                            byteImage = ms.ToArray();
                            string strB64 = Convert.ToBase64String(byteImage);
                            _CB0002M.BARCODE_IMAGE_STR = strB64;
                        }
                    }
                    catch (FormatException ex)
                    {
                        _CB0002M.BARCODE_IMAGE_STR = "";
                    }
                }
                list.Add(_CB0002M);
            }
                return list;
        }

 
        public IEnumerable<MI_MAST> GetMmcodeCombo(string p0, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} MMCODE, MMNAME_C, MMNAME_E
                            from MI_MAST where mat_class=:mat_class ";
            p.Add(":mat_class", string.Format("{0}", mat_class));
            if (p0 == null) p0 = "";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10 + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMNAME_E_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql += " OR MMNAME_E LIKE :MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmcodeData(string mmcode)
        {
            var p = new DynamicParameters();
            string sql = @"select  MMCODE, MMNAME_C, MMNAME_E
                            from MI_MAST where MMCODE =:MMCODE";
            p.Add(":MMCODE", mmcode);
            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }
        public DataTable GetExcel(string MAT_CLASS, string MMCODE, string MMNAME_C, string STATUS, string BARCODE, string MMNAME_E)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT (SELECT MAT_CLSNAME 
                                FROM MI_MATCLASS
                                WHERE MAT_CLASS = MMT.MAT_CLASS) 物料分類,
                               BBE.MMCODE as 院內碼,
                               MMT.MMNAME_C as 中文品名,
                               MMT.MMNAME_E as 英文品名,
                               BARCODE as 條碼, TRATIO as 轉換率, 
                               case when STATUS='Y' then '使用中' when STATUS='N' then '停用' else '' end as 使用代碼
                        FROM BC_BARCODE BBE, MI_MAST MMT
                        WHERE BBE.MMCODE = MMT.MMCODE ";

            if (MAT_CLASS != "")
            {
                sql += @" AND MMT.MAT_CLASS = :p0 ";
                p.Add(":p0", string.Format("{0}", MAT_CLASS));
            }

            if (MMCODE != "")
            {
                sql += @" AND BBE.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", MMCODE));
            }

            if (MMNAME_C != "")
            {
                sql += @" AND MMT.MMNAME_C LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", MMNAME_C));
            }
            if (STATUS != "")
            {
                sql += @" AND STATUS=:STATUS ";
                p.Add(":STATUS", STATUS);
            }
            if (BARCODE != "")
            {
                sql += @" AND BBE.BARCODE LIKE :p4 ";
                p.Add(":p4", string.Format("{0}%", BARCODE));
            }

            if (MMNAME_E != "")
            {
                sql += @" AND MMT.MMNAME_E LIKE :p5 ";
                p.Add(":p5", string.Format("{0}%", MMNAME_E));
            }

            sql += @" ORDER BY  MAT_CLASS,MMT.MMCODE";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);
            return dt;
        }
        public int ChkMmcodeCnt(string mmcode)  //檢查 院內碼個數, 刪除後至少留一筆
        {
            var sql = @" select count(*) from BC_BARCODE
                         where mmcode= :MMCODE ";
            return DBWork.Connection.QueryFirst<int>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }
        #region 2020-05-11 新增: 檢查院內碼是否存在
        public bool CheckMmcodeExists(string mmcode)
        {
            string sql = @"SELECT 1 FROM MI_MAST
                            WHERE mmcode  = :mmcode";
            return !(DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode}, DBWork.Transaction) == null);
        }
        #endregion
    }
}
