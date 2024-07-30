using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.AA
{
    public class AB0081ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string YYYMM { get; set; }
        public string MMCODE { get; set; } // 00.院內碼
        public string MMNAME_E { get; set; } // 01.商品名
        public string BASE_UNIT { get; set; } // 02.單位
        public string CHEMO_A { get; set; } // 03.內湖化療調配室_消耗
        public string CHEMO_I { get; set; } // 04.內湖化療調配室_結存 
        public string CHEMOT_A { get; set; } // 05.汀州化療調配室_消耗
        public string CHEMOT_I { get; set; } // 06.汀州化療調配室_結存 
        public string PH1A_A { get; set; } // 07.內湖住院藥局_消耗
        public string PH1A_I { get; set; } // 08.內湖住院藥局_結存 
        public string PH1C_A { get; set; } // 09.內湖門診藥局_消耗
        public string PH1C_I { get; set; } // 10.內湖門診藥局_結存 
        public string AVG_PRICE { get; set; } // 11.移動平圴加權量
        public string PH1R_A { get; set; } // 12.內湖急診藥局_消耗
        public string PH1R_I { get; set; } // 13.內湖急診藥局_結存 
        public string PHMC_A { get; set; } // 14.汀州藥局_消耗
        public string PHMC_I { get; set; } // 15.汀州藥局_結存 
        public string TPN_A { get; set; } // 16.製劑室_消耗
        public string TPN_I { get; set; } // 17.製劑室_結存 

        // -- 延伸
        public string SET_BTIME { get; set; } // 起始日期
        public string SET_ETIME { get; set; } // 結束日期
    }
    public class AB0081Repository : JCLib.Mvc.BaseRepository
    {
        public AB0081Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0081ReportMODEL> GetPrintData(string p0)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT B.MMCODE,C.MMNAME_E,C.BASE_UNIT,
                                  CHEMO_A,CHEMO_I,CHEMOT_A,CHEMOT_I,
                                  PH1A_A,PH1A_I,PH1C_A,PH1C_I,
                                  PH1R_A,PH1R_I,PHMC_A,PHMC_I,TPN_A,TPN_I,
                                  (SELECT AVG_PRICE FROM MI_WHCOST 
                                    WHERE DATA_YM=:DATA_YM AND MMCODE=B.MMCODE) AVG_PRICE
                           FROM (
                                  SELECT MMCODE,SUM(CHEMO_A) CHEMO_A,SUM(CHEMO_I) CHEMO_I,
                                         SUM(CHEMOT_A) CHEMOT_A,SUM(CHEMOT_I) CHEMOT_I,
                                         SUM(PH1A_A) PH1A_A,SUM(PH1A_I) PH1A_I,
                                         SUM(PH1C_A) PH1C_A,SUM(PH1C_I) PH1C_I,
                                         SUM(PH1R_A) PH1R_A,SUM(PH1R_I) PH1R_I,
                                         SUM(PHMC_A) PHMC_A,SUM(PHMC_I) PHMC_I,
                                         SUM(TPN_A) TPN_A,SUM(TPN_I) TPN_I
                                    FROM (
                                         SELECT A.MMCODE,
                                                DECODE(A.WH_NO,'CHEMO',USE_QTY,0) CHEMO_A,
                                                DECODE(A.WH_NO,'CHEMO',INV_QTY,0) CHEMO_I,
                                                DECODE(A.WH_NO,'CHEMOT',USE_QTY,0) CHEMOT_A,
                                                DECODE(A.WH_NO,'CHEMOT',INV_QTY,0) CHEMOT_I,
                                                DECODE(A.WH_NO,'PH1A',USE_QTY,0) PH1A_A,
                                                DECODE(A.WH_NO,'PH1A',INV_QTY,0) PH1A_I,
                                                DECODE(A.WH_NO,'PH1C',USE_QTY,0) PH1C_A,
                                                DECODE(A.WH_NO,'PH1C',INV_QTY,0) PH1C_I,
                                                DECODE(A.WH_NO,'PH1R',USE_QTY,0) PH1R_A,
                                                DECODE(A.WH_NO,'PH1R',INV_QTY,0) PH1R_I,
                                                DECODE(A.WH_NO,'PHMC',USE_QTY,0) PHMC_A,
                                                DECODE(A.WH_NO,'PHMC',INV_QTY,0) PHMC_I,
                                                DECODE(A.WH_NO,'TPN',USE_QTY,0) TPN_A,
                                                DECODE(A.WH_NO,'TPN',INV_QTY,0) TPN_I
                                           FROM MI_WINVMON A
                                          WHERE A.DATA_YM=:DATA_YM AND EXISTS
                                                (SELECT 1 FROM MI_WHMAST
                                                 WHERE WH_NO=A.WH_NO AND WH_KIND='0' AND WH_GRADE='2')
                                         )
                                   GROUP BY MMCODE
                                  ) B,
                                  MI_MAST C
                            WHERE B.MMCODE=C.MMCODE
                           ORDER BY B.MMCODE";
            
            
            return DBWork.Connection.Query<AB0081ReportMODEL>(sql, new { data_ym = p0 }, DBWork.Transaction);
        }

        public IEnumerable<AB0081ReportMODEL> GetMiMnsetData(string p0)
        {
            var p = new DynamicParameters();

            string sql = @"select twn_date(set_btime) as set_btime,
                                  twn_date(set_etime) as set_etime
                             from MI_MNSET
                            where set_ym = :p0";

            return DBWork.Connection.Query<AB0081ReportMODEL>(sql, new { p0 = p0}, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetYmCombo()
        {
            string sql = @"select set_ym as value,
                                  set_ym as TEXT,
                                  set_ym as COMBITEM
                             from MI_MNSET
                            where 1=1
                              and set_ym >= 10908
                              and set_status = 'C'
                            order by set_ym desc";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }


        //public IEnumerable<COMBO_MODEL> GetYmCombo()
        //{
        //    string sql = @"SELECT DISTINCT SUBSTR(TWN_DATE(APPTIME),1,5) AS VALUE 
        //                ,SUBSTR(TWN_DATE(APPTIME),1,5) AS TEXT
        //                ,SUBSTR(TWN_DATE(APPTIME),1,5) AS COMBITEM
        //                FROM ME_DOCM WHERE DOCTYPE='XR1' 
        //                ORDER BY 1 DESC ";

        //    return DBWork.Connection.Query<COMBO_MODEL>(sql);
        //}
        //public IEnumerable<COMBO_MODEL> GetMatclass2Combo()
        //{
        //    string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
        //                MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
        //                FROM MI_MATCLASS WHERE MAT_CLSID='2'  
        //                ORDER BY MAT_CLASS";

        //    return DBWork.Connection.Query<COMBO_MODEL>(sql);
        //}
        //public IEnumerable<COMBO_MODEL> GetMatclass3Combo()
        //{
        //    string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
        //                MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
        //                FROM MI_MATCLASS WHERE MAT_CLSID='3'   
        //                ORDER BY MAT_CLASS";

        //    return DBWork.Connection.Query<COMBO_MODEL>(sql);
        //}

        //public string GetTaskid(string id)
        //{
        //    string sql = @"SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=:WH_USERID 
        //                    AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')";
        //    string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
        //    return rtn;
        //}
        //public IEnumerable<COMBO_MODEL> GetTaskCombo()
        //{
        //    string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
        //                DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
        //                FROM PARAM_D
        //                WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='TASK_ID' 
        //                ORDER BY DATA_VALUE";

        //    return DBWork.Connection.Query<COMBO_MODEL>(sql);
        //}

        //public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        //{
        //    string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
        //                WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
        //                :UPDATE_IP,
        //                (SELECT COUNT(*) AS CNT FROM ME_DOCM 
        //                    WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
        //                    AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 ) MR2,
        //                NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
        //                    BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_BDAY')
        //                    AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_EDAY')),'N') MR3,
        //                NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
        //                    BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_BDAY')
        //                    AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4,
        //                NVL((SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=UR_ID.TUSER 
        //                    AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')),'2')TASK_ID
        //                FROM UR_ID
        //                WHERE UR_ID.TUSER=:TUSER";

        //    return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        //}
    }
}
