using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using WebApi.Models.ADC;
using WebApi.Models.ADC.GetMedicalCheckAccept;
using WebApi.Models.ADC.GetMedicalReturn;

namespace WebApi.Repository
{
    public class WebApiADCRepository : JCLib.Mvc.BaseRepository
    {
        public WebApiADCRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<getAdcMedOrdResult> getMedicalOrder(getAdcMedOrdRqBody input)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();
                string sql = @"
select 
d.DOCNO
,d.SEQ
,d.MMCODE
,d.APPQTY  
,d.APVQTY
,d.FRWH_D STOCKCODE
,d.CREATE_TIME APPLYTIME
,e.LOT_NO  LOTNO
,e.EXP_DATE EXPDATE 
from SYSTEM.ME_EXPM e 
inner join SYSTEM.ME_DOCD d on d.MMCODE = e.MMCODE
where  1=1 and d.DOCNO = :DOCNO
and APL_CONTIME between TO_DATE( :SDate,'YYYY/MM/DD') and  TO_DATE( :EDate,'YYYY/MM/DD')
and d.FRWH_D =:FRWH


";

                p.Add("DOCNO", input.DOCNO);
                p.Add("FRWH", input.FRWH);
                p.Add("SDate", input.SDate.ToString("yyyy/MM/dd"));
                p.Add("EDate", input.EDate.ToString("yyyy/MM/dd"));
                return DBWork.Connection.Query<getAdcMedOrdResult>(sql, p);



            }
            catch (Exception e)
            {

                throw;
            }


        }

        public int updateMedicalOrder(string docno,int seq)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();
                string sql = @"
UPDATE SYSTEM.ME_DOCD d 
SET 
d.APVQTY = d.APVQTY - 1
WHERE d.DOCNO = :DOCNO  AND d.SEQ=:SEQ 

";
                p.Add("DOCNO",docno);
                p.Add("SEQ",seq);

                return DBWork.Connection.Execute(sql, p);

            }
            catch (Exception)
            {

                throw;
            }




        }

        public IEnumerable<getMedicalAllocateResult> getMedicalAllocate(getMedicalAllocateRqBody input)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();
                string sql = $@"
select 
d.DOCNO
,d.SEQ
,d.MMCODE
,d.APPQTY  
,d.APVQTY
,d.FRWH_D STOCKCODE
,d.CREATE_TIME APPLYTIME
,e.LOT_NO  LOTNO
,e.EXP_DATE EXPDATE 
from SYSTEM.ME_EXPM e 
inner join SYSTEM.ME_DOCD d on d.MMCODE = e.MMCODE
where  1=1 
{(string.IsNullOrEmpty(input.DOCNO)?"":" and d.DOCNO = :DOCNO ")}
and APL_CONTIME between TO_DATE( :SDate,'YYYY/MM/DD') and  TO_DATE( :EDate,'YYYY/MM/DD')

";

                p.Add("DOCNO", input.DOCNO);
                p.Add("SDate", input.SDate.ToString("yyyy/MM/dd"));
                p.Add("EDate", input.EDate.ToString("yyyy/MM/dd"));
                return DBWork.Connection.Query<getMedicalAllocateResult>(sql, p);



            }
            catch (Exception e)
            {

                throw;
            }
        }

        public IEnumerable<dynamic> getINV_QTY(string docno,int seq) {
            try
            {
                DynamicParameters p = new DynamicParameters();
                string sql = $@"
select 
*
from SYSTEM.ME_EXPM e 
inner join SYSTEM.ME_DOCD d on d.MMCODE = e.MMCODE
where  d.DOCNO = :DOCNO
and d.SEQ = :SEQ

";

                p.Add("DOCNO", docno);
                p.Add("SEQ", seq);
                return DBWork.Connection.Query<dynamic>(sql, p);



            }
            catch (Exception e)
            {

                throw;
            }
        }

        public int updateMedicalQty(int executeType,string docno, int seq ,int qtyNum)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();

                string targetCol = executeType == 1 ? "d.APPQTY" : executeType == 2 ? "d.APVQTY" : executeType == 3 ? "d.ACKQTY" : "";
                if (targetCol=="")
                {
                    throw new Exception("請確認調撥狀態");
                }
                string sql = $@"
UPDATE SYSTEM.ME_DOCD d 
SET 
{targetCol} = :QTY
WHERE d.DOCNO = :DOCNO  AND d.SEQ=:SEQ 

";
                p.Add("DOCNO", docno);
                p.Add("SEQ", seq);
                p.Add("QTY", qtyNum);

                return DBWork.Connection.Execute(sql, p);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<GetMedicalReturnResult> getMedicalReturn(GetMedicalReturnRqBody input)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();
                string sql = $@"
select 
d.DOCNO
,d.SEQ
,d.MMCODE
,d.APPQTY  
,d.APVQTY
,d.FRWH_D STOCKCODE
,d.CREATE_TIME APPLYTIME
,e.LOT_NO  LOTNO
,e.EXP_DATE EXPDATE 
from SYSTEM.ME_EXPM e 
inner join SYSTEM.ME_DOCD d on d.MMCODE = e.MMCODE
where  1=1 
{(string.IsNullOrEmpty(input.DOCNO) ? "" : " and d.DOCNO = :DOCNO ")}
and APL_CONTIME between TO_DATE( :SDate,'YYYY/MM/DD') and  TO_DATE( :EDate,'YYYY/MM/DD')
and d.FRWH_D =:FRWH 
";

                p.Add("DOCNO", input.DOCNO);
                p.Add("FRWH", input.FRWH);
                p.Add("SDate", input.SDate.ToString("yyyy/MM/dd"));
                p.Add("EDate", input.EDate.ToString("yyyy/MM/dd"));
                return DBWork.Connection.Query<GetMedicalReturnResult>(sql, p);

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public int updateMedicalAPPQTY(string docno, int seq, int qtyNum)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();

                string sql = $@"
UPDATE SYSTEM.ME_DOCD d 
SET 
d.APPQTY= :QTY
WHERE d.DOCNO = :DOCNO  AND d.SEQ=:SEQ 

";
                p.Add("DOCNO", docno);
                p.Add("SEQ", seq);
                p.Add("QTY", qtyNum);

                return DBWork.Connection.Execute(sql, p);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<GetMedicalCheckAcceptResult> getMedicalCheckAccept(GetMedicalCheckAcceptRqBody input)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();
                string sql = $@"
select 
d.DOCNO
,d.SEQ
,d.MMCODE
,d.APPQTY  
,d.APVQTY
,d.FRWH_D STOCKCODE
,d.CREATE_TIME APPLYTIME
,e.LOT_NO  LOTNO
,e.EXP_DATE EXPDATE 
from SYSTEM.ME_EXPM e 
inner join SYSTEM.ME_DOCD d on d.MMCODE = e.MMCODE
where  1=1 
{(string.IsNullOrEmpty(input.DOCNO) ? "" : " and d.DOCNO = :DOCNO ")}
and d.APVTIME between TO_DATE( :SDate,'YYYY/MM/DD') and  TO_DATE( :EDate,'YYYY/MM/DD')
--and d.FRWH_D =:FRWH 
";

                p.Add("DOCNO", input.DOCNO);
                //p.Add("FRWH", input.TOWH);
                p.Add("SDate", input.SDate.ToString("yyyy/MM/dd"));
                p.Add("EDate", input.EDate.ToString("yyyy/MM/dd"));
                return DBWork.Connection.Query<GetMedicalCheckAcceptResult>(sql, p);

            }
            catch (Exception e)
            {

                throw;
            }

        }

        public IEnumerable<dynamic> getEXPT_DISTQTY_AND_ACKQTY(string docno, int seq)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();
                string sql = $@"
select 
*
from SYSTEM.ME_EXPM e 
inner join SYSTEM.ME_DOCD d on d.MMCODE = e.MMCODE
where  d.DOCNO = :DOCNO
and d.SEQ = :SEQ

";

                p.Add("DOCNO", docno);
                p.Add("SEQ", seq);
                return DBWork.Connection.Query<dynamic>(sql, p);



            }
            catch (Exception e)
            {

                throw;
            }

        }

        public int UpdateMedicalCheckAccept(string docno, int seq, int ackQty,string ackId)
        {
            try
            {
                DynamicParameters p = new DynamicParameters();

                string sql = $@"
UPDATE SYSTEM.ME_DOCD d 
SET 
d.ACKQTY= :ACKQTY,
d.ACKQTYT =d.ACKQTYT + :ACKQTY
d.ACKID = :ACKID
WHERE d.DOCNO = :DOCNO  AND d.SEQ=:SEQ 

";
                p.Add("DOCNO", docno);
                p.Add("SEQ", seq);
                p.Add("ACKQTY", ackQty);
                p.Add("ACKID", ackId);

                return DBWork.Connection.Execute(sql, p);

            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}