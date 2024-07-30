using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace His2ConsumeTransfer
{
    public class His2ConsumeTransferRepository
    {
        // 2022-10-06: 只抓急診，增加判斷visit_kind=3，
        #region HIS2

        public string GetReadflagN() {
            string sql = @"
                select 1 from HIS2USER2.HIS_CONSUME_D
                 where readflag = 'N'
                   and readdatetime is null
                   -- and visit_kind = '3'
            ";
            return sql;
        }

        public string UpdateReadflagP() {
            string sql = @"
                update HIS2USER2.HIS_CONSUME_D
                   set readflag = 'P'
                 where readflag = 'N'
                   and readdatetime is null
                   -- and visit_kind = '3'
            ";

            return sql;
        }

        public string GetReadflagP() {
            string sql = @"
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
                 where readflag = 'P'
                   and readdatetime is null
            ";
            return sql;
        }

        public string UpdateReadflag() {
            string sql = string.Format(@"
                update HIS2USER2.HIS_CONSUME_D
                   set readflag = 'Y',
                       readdatetime = sysdate
                 where id = :ID
            ");
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
                values (  :ID, to_date(:DATA_DATE, 'yyyy-mm-dd HH24:mi:ss')
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
