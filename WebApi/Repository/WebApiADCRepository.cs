using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using WebApi.Models.ADC;

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

        public int updateMedicalOrder()
        {
            try
            {
                DynamicParameters p = new DynamicParameters();
                string sql = @"
UPDATE SYSTEM.ME_DOCD d 
SET 
d.APVQTY = d.APVQTY - 1
WHERE d.DOCNO = '330300111080109495709';

";

                //p.Add("DOCNO", input.DOCNO);
                //p.Add("FRWH", input.FRWH);
                //p.Add("SDate", input.SDate.ToString("yyyy/MM/dd"));
                //p.Add("EDate", input.EDate.ToString("yyyy/MM/dd"));
                return DBWork.Connection.Execute(sql, p);

            }
            catch (Exception)
            {

                throw;
            }




        }
    }
}