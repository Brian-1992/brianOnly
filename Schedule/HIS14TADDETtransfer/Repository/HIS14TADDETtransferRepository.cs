using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIS14TADDETtransfer
{
    class HIS14TADDETtransferRepository
    {
        public string Delete_PARAM_D()
        {
            string sql = @" delete from PARAM_D where GRP_CODE='HIS14_TADDET' ";
            return sql;
        }

        public string Get_PARAM_M_Query()
        {
            string sql = @" select 1 from MMSADM.PARAM_M where GRP_CODE='HIS14_TADDET' ";
            return sql;
        }

        public string Insert_PARAM_M()
        {
            string sql = @"
                    BEGIN
                        INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('HIS14_TADDET','國軍醫令扣庫TADDET藥局扣庫點對照檔');
                    END;
                ";

            return sql;
        }

        public string Insert_PARAM_D(List<PARAM_D> list_param_d)
        {
            string sql_sub = "";
            foreach (PARAM_D item in list_param_d)
            {
                sql_sub += string.Format(@"
                                        INSERT INTO MMSADM.PARAM_D
                                                    (GRP_CODE,
                                                     DATA_SEQ,
                                                     DATA_NAME,
                                                     DATA_VALUE,
                                                     DATA_DESC,
                                                     DATA_REMARK)
                                        SELECT '{0}' as GRP_CODE,
                                               '{1}' as DATA_SEQ,
                                               '{2}' AS DATA_NAME,
                                               '{3}' AS DATA_VALUE,
                                               '{4}' AS DATA_DESC,
                                               '{5}' AS DATA_REMARK
                                        FROM DUAL; 
                                    ", item.GRP_CODE, item.DATA_SEQ, item.DATA_NAME, item.DATA_VALUE, item.DATA_DESC, item.DATA_REMARK);
            };

            string sql = string.Format(@"
                                        BEGIN
                                            {0}
                                        END;
                                    ", sql_sub);
            return sql;
        }

        public string Get_HRBT_Query()
        {
            string sql = @" select to_char(SYSDATE,'HH24') as V_HOUR,
                        (case when to_char(SYSDATE,'HH24') = '00' then TWN_DATE(SYSDATE-1) else TWN_DATE(SYSDATE) end) as V_DATE,
                        TWN_DATE(SYSDATE) as V_DATE_TODAY,
                        (case when to_char(SYSDATE,'HH24') = '00' then '235959' else LPAD(TO_CHAR(TO_NUMBER(to_char(SYSDATE,'HH24'))-1),2,'0')||'5959' end) as V_ETIME
                        from dual ";
            return sql;
        }

        public string Delete_HIS14_TADDET(string v_date)
        {
            string sql = @" delete HIS14_TADDET
                    where DET_USEDATE='" + v_date + "' ";
            return sql;
        }

        public string Delete_HIS14_TADDET1(string v_date)
        {
            string sql = @" delete HIS14_TADDET1
                    where DET_USEDATE='" + v_date + "' ";
            return sql;
        }

        public string Get_TADDET_Query(string v_date, string hosp_id)
        {
            string tablePrefix = "";
            if (hosp_id == "805")
                tablePrefix = "HIS803."; // 805 使用者AIDC的Table權限由HIS803 grant

            string sql = @" select 
                  DET_USEDATE, DET_STKKIND, DET_KIND, DET_SKDIACODE, DET_MEDNO,
                  DET_UNIT, DET_COST, DET_ATTACHQTY, DET_ATTACHUNIT, DET_USEQTY,
                  DET_EMPNO, DET_SECTNO, DET_DEPTCENTER, DET_NRCODE, DET_BEDNO,
                  DET_PROCDATE, DET_PROCTIME, DET_PROCOPID
                from " + tablePrefix + @"TADDET
                where DET_USEDATE='" + v_date + @"'
                 ";
            return sql;
        }

        public string Get_TADDET1_Query(string v_date)
        {
            string sql = @" select 
                  DET_USEDATE, DET_STKKIND, DET_KIND, DET_SKDIACODE, DET_MEDNO,
                  DET_UNIT, DET_COST, DET_ATTACHQTY, DET_ATTACHUNIT, DET_USEQTY,
                  DET_EMPNO, DET_SECTNO, DET_DEPTCENTER, DET_NRCODE, DET_BEDNO,
                  DET_PROCDATE, DET_PROCTIME, DET_PROCOPID
                from TADDET
                where DET_USEDATE='" + v_date + @"'
                 ";
            return sql;
        }

        public string Get_HOSPCODE_Query()
        {
            string sql = @" select data_value as sHospCode
                  from param_d
                 where grp_code='HOSP_INFO'
                   and data_name='HospCode'
                 ";
            return sql;
        }

        public string Get_STOCKCODE_Query(string sHospCode, string stkkind, string det_kind)
        {
            string sql = @" select a.data_value as sSTOCKCODE
                  from param_d a
                 where a.grp_code='HIS14_TADDET'
                   and a.data_remark='" + sHospCode + stkkind + @"' 
                   and a.data_name=(case
                     when '"+ det_kind + @"' = '1' then '1'
                     when '" + det_kind + @"' = '2' then '3'
                     when '" + det_kind + @"' = '3' then '2'
                     when '" + det_kind + @"' = '4' then '2'
                     else '2' end)
             ";
            return sql;
        }

        public string Insert_HIS14_TADDET()
        {
            string sql = @"
                                        insert into HIS14_TADDET(
                                          DET_USEDATE, DET_STKKIND, DET_KIND, DET_SKDIACODE, DET_MEDNO,
                                          DET_UNIT, DET_COST, DET_ATTACHQTY, DET_ATTACHUNIT, DET_USEQTY,
                                          DET_EMPNO, DET_SECTNO, DET_DEPTCENTER, DET_NRCODE, DET_BEDNO,
                                          DET_PROCDATE, DET_PROCTIME, DET_PROCOPID,
                                          READTIME, ID, STOCKCODE)
                                        values(
                                          :DET_USEDATE,:DET_STKKIND,:DET_KIND,:DET_SKDIACODE,:DET_MEDNO,
                                          :DET_UNIT,:DET_COST,:DET_ATTACHQTY,:DET_ATTACHUNIT,:DET_USEQTY,
                                          :DET_EMPNO,:DET_SECTNO,:DET_DEPTCENTER,:DET_NRCODE,:DET_BEDNO,
                                          :DET_PROCDATE,:DET_PROCTIME,:DET_PROCOPID,
                                          sysdate, HIS14TADDET_SEQ.nextval, :STOCKCODE)
                                    ";
            return sql;
        }

        public string Insert_HIS14_TADDET1()
        {
            string sql = @"
                                        insert into HIS14_TADDET1(
                                          DET_USEDATE, DET_STKKIND, DET_KIND, DET_SKDIACODE, DET_MEDNO,
                                          DET_UNIT, DET_COST, DET_ATTACHQTY, DET_ATTACHUNIT, DET_USEQTY,
                                          DET_EMPNO, DET_SECTNO, DET_DEPTCENTER, DET_NRCODE, DET_BEDNO,
                                          DET_PROCDATE, DET_PROCTIME, DET_PROCOPID,
                                          READTIME, ID, STOCKCODE)
                                        values(
                                          :DET_USEDATE,:DET_STKKIND,:DET_KIND,:DET_SKDIACODE,:DET_MEDNO,
                                          :DET_UNIT,:DET_COST,:DET_ATTACHQTY,:DET_ATTACHUNIT,:DET_USEQTY,
                                          :DET_EMPNO,:DET_SECTNO,:DET_DEPTCENTER,:DET_NRCODE,:DET_BEDNO,
                                          :DET_PROCDATE,:DET_PROCTIME,:DET_PROCOPID,
                                          sysdate, HIS14TADDET1_SEQ.nextval, :STOCKCODE)
                                    ";
            return sql;
        }
    }
}
