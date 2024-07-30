using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.AB
{
    public class AB0040Repository : JCLib.Mvc.BaseRepository
    {
        public AB0040Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢
        public IEnumerable<AB0040> GetAll(string ordercode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                        M.ORDERCODE,
                        (SELECT DATA_VALUE||'_'||DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_STKDMIT'AND DATA_NAME='PUBLICDRUGCODE'AND DATA_VALUE=T.PUBLICDRUGCODE)PUBLICDRUGCODE,  --預設公藥分類
                        (SELECT PATHNO||'_'||PATHNAME FROM HIS_BASPATH WHERE PATHNO=M.PATHNO)PATHNO,  --預設給藥途徑
                        M.DOSE,  --預設劑量
                        (SELECT FREQNO||'_'||FREQNAME FROM HIS_BASFREQ WHERE FREQNO=M.FREQNOI)FREQNOI,  --住院預設頻率
                        (SELECT DATA_VALUE||'_'||DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM'AND DATA_NAME='TAKEKIND'AND DATA_VALUE=M.TAKEKIND)TAKEKIND,  --服用藥別(排序)
                        T.DRUGFORM,  --藥品劑型
                        (SELECT DATA_VALUE||'_'||DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM'AND DATA_NAME='TAKEKIND'AND DATA_VALUE=M.SPECIALORDERKIND)SPECIALORDERKIND,  --外審(健保專案)碼
                        M.HOSPEXAMINEFLAG,  --內審用藥
                        M.HospExamineQtyFlag,  --限制用量
                        M.ORDERABLEDRUGFORM,  --藥品劑型規格(電子簽章)
                        (SELECT DATA_VALUE||'_'||DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM'AND DATA_NAME='RESTRICTCODE'AND DATA_VALUE=M.RESTRICTCODE)RESTRICTCODE,  --管制用藥
                        M.FIXPATHNOFLAG,  --限制途徑
                        M.HOSPEXAMINEQTYFLAG HOSPEXAMINEQTYFLAG1,  --限制劑量
                        (SELECT FREQNO||'_'||FREQNAME FROM HIS_BASFREQ WHERE FREQNO=M.FREQNOO)FREQNOO,  --門診預設頻率
                        T.DRUGCLASS,  --藥品類別
                        (SELECT DATA_VALUE||'_'||DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM'AND DATA_NAME='ANTIBIOTICSCODE'AND DATA_VALUE=M.ANTIBIOTICSCODE)ANTIBIOTICSCODE,  --抗生素等級
                        M.LIMITFLAG,  --開立限制設定
                        M.RETURNDRUGFLAG,  --CDC藥品
                        M.SINGLEITEMFLAG,  --獨立處方箋
                        M.INSUOFFERFLAG,  --保險給付
                        M.PUBLICDRUGFLAG,  --公藥
                        (SELECT DATA_VALUE||'_'||DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM'AND DATA_NAME='HEPATITISCODE'AND DATA_VALUE=M.HEPATITISCODE)HEPATITISCODE,  --BC肝用藥註記
                        T.INVENTORYFLAG,  --盤點品項
                        M.RESEARCHDRUGFLAG,  --研究用藥
                        M.RAREDISORDERFLAG,  --罕見疾病用藥
                        M.HIGHPRICEFLAG,  --高價用藥
                        M.VACCINE,  --疫苗註記
                        (SELECT DATA_VALUE||'_'||DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM'AND DATA_NAME='VACCINECLASS'AND DATA_VALUE=M.VACCINECLASS)VACCINECLASS,
                        M.RESTRICTTYPE,  --限制重覆醫令
                        M.MAXTAKETIMES,  --限制次數
                        M.MAXQTYO,  --門診開立數量
                        M.MAXDAYSO,  --門診開立日數
                        M.VALIDDAYSO,  --門診效期日數
                        M.MAXQTYI,  --住院開立數量
                        M.MAXDAYSI,  --住院開立日數
                        M.VALIDDAYSI,  --住院效期日數
                        D.InsuOrderCode,
                        M.OrderHospName,
                        M.OrderEasyName,
                        X.MMNAME_C,
                        X.MMNAME_E,
                        M.ScientificName
                        FROM HIS_BASORDM M 
                        JOIN HIS_BASORDD D ON D.ORDERCODE = M.ORDERCODE
                        LEFT OUTER JOIN HIS_STKDMIT T ON T.SKORDERCODE = M.ORDERCODE
                        LEFT OUTER JOIN MI_MAST X ON X.MMCODE = M.ORDERCODE 
                        WHERE TWN_SYSDATE <= D.ENDDATE
                        AND  TWN_SYSDATE >= D.BEGINDATE 
                        ";

            if (ordercode != "")
            {
                sql += " AND M.ORDERCODE = :p0 ";
                p.Add(":p0", string.Format("{0}", ordercode));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0040>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        
        //院內碼下拉式選單
        public IEnumerable<COMBO_MODEL> GetOrdercodeCombo()
        {
            string sql = @"SELECT M.ORDERCODE AS VALUE, M.ORDERCODE AS TEXT, M.ORDERCODE AS COMBITEM 
                            FROM HIS_BASORDM M
                            WHERE ORDERTYPE LIKE 'D%' 
                              AND EXISTS (SELECT 1 FROM HIS_BASORDD D WHERE D.ORDERCODE = M.ORDERCODE AND TWN_SYSDATE <= D.ENDDATE AND  TWN_SYSDATE >= D.BEGINDATE )
                            ORDER BY ORDERCODE ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} B.MMCODE, B.MMNAME_C, B.MMNAME_E 
                        FROM HIS_BASORDM A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.ORDERCODE
                        WHERE A.ORDERTYPE LIKE 'D%' 
                              AND EXISTS (SELECT 1 FROM HIS_BASORDD D WHERE D.ORDERCODE = A.ORDERCODE AND TWN_SYSDATE <= D.ENDDATE AND  TWN_SYSDATE >= D.BEGINDATE )
                          ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(B.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(B.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(B.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (B.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR B.MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR B.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY B.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
    }
}