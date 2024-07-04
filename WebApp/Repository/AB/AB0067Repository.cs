using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AB
{
    public class AB0067Repository : JCLib.Mvc.BaseRepository
    {
        public AB0067Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<COMBO_MODEL> GetApplyWH_NOComboOne()
        {
            var p = new DynamicParameters();

            string sql = @"select WH_NO ||'_'|| WH_NAME as COMBITEM, WH_NO as VALUE,
                            WH_NAME as TEXT
                            from MI_WHMAST
                            where WH_KIND='0'
                            order by WH_GRADE, WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWriteOffWH_NOComboOne()
        {
            var p = new DynamicParameters();

            string sql = @"select WH_NO ||'_'|| WH_NAME as COMBITEM, WH_NO as VALUE,
                            WH_NAME as TEXT
                            from MI_WHMAST
                            where WH_KIND='0' and WH_GRADE in ('1','2')
                            order by WH_GRADE, WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0067> SearchReportData(string CloseDate_Start, string CloseDate_End, string ApplyWH_NO, string WriteOffWH_NO)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                    B.NRCode,   --病房號
                    B.BedNo,    --床位號
                    B.MedNo,    --病歷號
                    B.ChinName, --病人姓名
                    TWN_DATE(B.acktime) acktime,  --日期
                    C.mmname_e, --藥品名稱
                    B.appqty,   --數量
                    B.apvqty,   --補量
                    CASE WHEN B.GTAPL_RESON in ('01','02','03') THEN 'X' ELSE ' ' END RESON1, --破損
                    CASE WHEN B.GTAPL_RESON='04' THEN 'X' ELSE ' ' END RESON2, --未收到
                    CASE WHEN B.GTAPL_RESON='05' THEN 'X' ELSE ' ' END RESON3, --其它
                    CASE WHEN B.GTAPL_RESON='06' THEN 'X' ELSE ' ' END RESON4, --破損
                    CASE WHEN B.GTAPL_RESON='07' THEN 'X' ELSE ' ' END RESON5, --過效期
                    CASE WHEN B.GTAPL_RESON='08' THEN 'X' ELSE ' ' END RESON6, --變質
                    B.APLYITEM_NOTE,  --備註
                    (SELECT AVG_PRICE FROM MI_WHCOST WHERE DATA_YM = SUBSTR(TWN_DATE(A.POST_TIME),1,5) AND MMCODE = B.MMCODE) AVG_PRICE, --移動平均價
                    B.appqty * (SELECT AVG_PRICE FROM MI_WHCOST WHERE DATA_YM = SUBSTR(TWN_DATE(A.POST_TIME),1,5) AND MMCODE = B.MMCODE) MONEY,  --金額
                    B.FRWH_D, --銷帳庫別
                    (SELECT DATA_DESC FROM PARAM_D WHERE DATA_NAME = 'CONFIRMSWITCH' AND DATA_VALUE = B.ConfirmSwitch) ConfirmSwitch,   --補發類別
                    USER_NAME(A.APPID) APPID,  --申請人
                    TWN_DATE(A.CREATE_TIME) CREATE_TIME,  --建立日期
                    TWN_DATE(A.UPDATE_TIME) UPDATE_TIME   --處理日期
                    FROM ME_DOCM A, ME_DOCD B, MI_MAST C
                    WHERE A.DOCTYPE = 'RR'
                    AND A.DOCNO = B.DOCNO
                    AND B.MMCODE = C.MMCODE
                    AND A.FLOWID ='1399'
                    AND TRUNC(B.APVTIME) BETWEEN TWN_TODATE(:CloseDate_Start) AND TWN_TODATE(:CloseDate_End) ";

            p.Add(":CloseDate_Start", string.Format("{0}", CloseDate_Start));
            p.Add(":CloseDate_End", string.Format("{0}", CloseDate_End));

            if (!string.IsNullOrEmpty(ApplyWH_NO))
            {
                sql += "AND A.FRWH = :ApplyWH_NO";
                p.Add(":ApplyWH_NO", string.Format("{0}", ApplyWH_NO));
            }
            if (!string.IsNullOrEmpty(WriteOffWH_NO))
            {
                sql += "AND B.FRWH_D = :WriteOffWH_NO";
                p.Add(":WriteOffWH_NO", string.Format("{0}", WriteOffWH_NO));
            }

            return DBWork.Connection.Query<AB0067>(sql, p);
        }
    }
}