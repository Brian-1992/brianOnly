using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace His2ConsumeTransferByDatetimeRange
{
    public class His2ConsumeTransferRepository
    {
        // 2022-10-06: 只抓急診，增加判斷visit_kind=3，
        #region HIS2

        public string GetDateRange(string startTime, string endTime) {
            string sql = string.Format(@"
                select ID, to_char(DATA_DATE, 'yyyy-mm-dd HH24:mi:ss') as data_date, ORDERCODE, STOCKCODE, STOCKFLAG         
                         , ORDERDR, MEDNO
                         , to_char(VISITDATE, 'yyyy-mm-dd HH24:mi:ss') as VISITDATE, RID 
                         , to_char(WORKDATETIME, 'yyyy-mm-dd HH24:mi:ss') as WORKDATETIME
                         , to_char(MODIFYDATETIME, 'yyyy-mm-dd HH24:mi:ss') as MODIFYDATETIME
                         , to_char(CANCELDATETIME, 'yyyy-mm-dd HH24:mi:ss') as CANCELDATETIME, SECTIONNO, VSDR , INSULOOKSEQ       
                         , DOSE, FREQNO, PATHNO, DAYS, SUMQTY            
                         , EMGFLAG, PAYFLAG, COMPUTECODE, ADDRATIO1, ADDRATIO2         
                         , ADDRATIO3, INSUCHARGEID , HOSPCHARGEID , INSUAMOUNT, PAYAMOUNT         
                         , CANCELFLAG, CANCELOPID, PROCOPID 
                         , to_char(PROCDATETIME, 'yyyy-mm-dd HH24:mi:ss') as PROCDATETIME , CREATEOPID        
                         , to_char(CREATEDATETIME, 'yyyy-mm-dd HH24:mi:ss') as CREATEDATETIME , DRUGNO, SLOWCARD , BAGSEQNO 
                         , to_char(SENDDATE, 'yyyy-mm-dd HH24:mi:ss')  as SENDDATE, TOTALQTY          
                         , ORDERUNIT , ATTACHUNIT, READFLAG , READDATETIME, VISIT_KIND        
                         , CHARNO , BATCHNUM , EXPIREDDATE , PARENT_ORDERCODE, PARENT_CONSUME_QTY, REGION 
                  from HIS2USER2.HIS_CONSUME_D
                 where DATA_DATE >= to_date('{0}', 'yyyy-mm-dd HH24:mi:ss')
                   and DATA_DATE <= to_date('{1}', 'yyyy-mm-dd HH24:mi:ss')
                   and visit_kind='3'
            ", startTime, endTime);

            return sql;
        }
        
        #endregion

        #region MMSMS

        public string CheckIdExists(string id) {
            string sql = string.Format(@"
                select 1 from HIS_CONSUME_D_HIS2
                 where id = '{0}'
            ", id);
            return sql;
        }

        public string InsertMMSMS() {
            string sql = @"
                insert into HIS_CONSUME_D_HIS2
                       (  ID, DATA_DATE
                        , ORDERCODE, STOCKCODE, STOCKFLAG         
                        , ORDERDR, MEDNO
                        , VISITDATE, RID 
                        , WORKDATETIME
                        , MODIFYDATETIME, CANCELDATETIME, SECTIONNO, VSDR , INSULOOKSEQ       
                        , DOSE, FREQNO, PATHNO, DAYS, SUMQTY            
                        , EMGFLAG, PAYFLAG, COMPUTECODE, ADDRATIO1, ADDRATIO2         
                        , ADDRATIO3, INSUCHARGEID , HOSPCHARGEID , INSUAMOUNT, PAYAMOUNT         
                        , CANCELFLAG, CANCELOPID, PROCOPID 
                        , PROCDATETIME , CREATEOPID        
                        , CREATEDATETIME , DRUGNO, SLOWCARD , BAGSEQNO 
                        , SENDDATE, TOTALQTY          
                        , ORDERUNIT , ATTACHUNIT, VISIT_KIND        
                        , CHARNO , BATCHNUM , EXPIREDDATE , PARENT_ORDERCODE, PARENT_CONSUME_QTY, REGION
                        )
                values (  :ID, to_date(to_char(sysdate, 'yyyy-mm-dd') || ' ' || '00:01:00', 'yyyy-mm-dd HH24:mi:ss')
                        , :ORDERCODE, :STOCKCODE, :STOCKFLAG         
                        , :ORDERDR, :MEDNO
                        , to_date(:VISITDATE, 'yyyy-mm-dd HH24:mi:ss'), :RID 
                        , to_date(:WORKDATETIME, 'yyyy-mm-dd HH24:mi:ss')
                        , to_date(:MODIFYDATETIME, 'yyyy-mm-dd HH24:mi:ss')
                        , to_date(:CANCELDATETIME, 'yyyy-mm-dd HH24:mi:ss')
                        , :SECTIONNO, :VSDR , :INSULOOKSEQ       
                        , :DOSE, :FREQNO, :PATHNO, :DAYS, :SUMQTY            
                        , :EMGFLAG, :PAYFLAG, :COMPUTECODE, :ADDRATIO1, :ADDRATIO2         
                        , :ADDRATIO3, :INSUCHARGEID , :HOSPCHARGEID , :INSUAMOUNT, :PAYAMOUNT         
                        , :CANCELFLAG, :CANCELOPID, :PROCOPID 
                        , to_date(:PROCDATETIME, 'yyyy-mm-dd HH24:mi:ss') , :CREATEOPID        
                        , to_date(:CREATEDATETIME, 'yyyy-mm-dd HH24:mi:ss') , :DRUGNO, :SLOWCARD , :BAGSEQNO 
                        , to_date(:SENDDATE, 'yyyy-mm-dd HH24:mi:ss'), :TOTALQTY          
                        , :ORDERUNIT , :ATTACHUNIT, :VISIT_KIND        
                        , :CHARNO , :BATCHNUM , :EXPIREDDATE , :PARENT_ORDERCODE, :PARENT_CONSUME_QTY, :REGION
                       )
            ";
            return sql;
        }

        #endregion
    }
}
