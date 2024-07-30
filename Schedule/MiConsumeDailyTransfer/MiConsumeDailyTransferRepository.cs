using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiConsumeDailyTransfer
{
    public class MiConsumeDailyTransferRepository : JCLib.Mvc.BaseRepository
    {
        public MiConsumeDailyTransferRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckMiMnSet() {
            string sql = @"
                select 1 from MI_MNSET
                 where set_status = 'N'
            ";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction) != null;
        }

        public bool CheckPreData() {
            string sql = @"
                select 1 from MI_WHINV_DAILY
                 where twn_date(data_date) = twn_date(sysdate-1)
            ";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction) != null;
        }

        public int DeleteOldData() {
            string sql = @"
                delete from MI_WHINV_DAILY
                 where twn_date(data_date) < twn_date(sysdate - 35)
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        public bool CheckSetCtimeToday() {
            string sql = @"
                select 1 from MI_MNSET
                 where twn_date(set_ctime) = twn_date(sysdate-1)
            ";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction) != null;
        }

        public int InsertNewData() {
            string sql = @"
                insert into MI_WHINV_DAILY(
                        data_date, wh_no, mmcode, INV_QTY, ONWAY_QTY, 
                        APL_INQTY, APL_OUTQTY, TRN_INQTY, TRN_OUTQTY,
                        ADJ_INQTY, ADJ_OUTQTY, BAK_INQTY, BAK_OUTQTY,
                        REJ_OUTQTY, DIS_OUTQTY, EXG_INQTY, EXG_OUTQTY,
                        MIL_INQTY, MIL_OUTQTY, INVENTORYQTY,use_qty,  back_qty
                       )
                select trunc(sysdate-1) as data_date,
                       a.wh_no, a.mmcode, a.INV_QTY, a.ONWAY_QTY,
                       a.APL_INQTY, a.APL_OUTQTY, a.TRN_INQTY, a.TRN_OUTQTY,
                       a.ADJ_INQTY, a.ADJ_OUTQTY, a.BAK_INQTY, a.BAK_OUTQTY ,
                       a.REJ_OUTQTY, a.DIS_OUTQTY, a.EXG_INQTY, a.EXG_OUTQTY,
                       a.MIL_INQTY, a.MIL_OUTQTY, a.INVENTORYQTY, a.use_qty, nvl(b.TR_INV_QTY, 0) as back_qty
                  from MI_WHINV a
                  left join  (
                            with mnset_data as (
                                 select twn_date(set_btime) as set_btime,
                                        twn_date(nvl(set_etime, set_ctime)) as set_etime
                                   from MI_MNSET
                                  where (sysdate-1) between set_btime and nvl(set_etime, set_ctime)
                            )
                            select wh_no, mmcode, sum(AF_TR_INVQTY - BF_TR_INVQTY) TR_INV_QTY
                              FROM MI_WHTRNS A, mnset_data b
                             WHERE TR_MCODE = 'BAKI' AND TR_DOCTYPE = 'RS'
                               and SUBSTR (TR_DOCNO, 1, 7) between b.set_btime and b.set_etime
                             group by wh_no, mmcode
                       ) b on (a.wh_no = b.wh_no and a.mmcode = b.mmcode)
                 where 1=1
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        public string GetSetYm() {
            string sql = @"
                select set_ym from MI_MNSET
                 where twn_date(set_ctime) = twn_date(sysdate -1)
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        public int InsertNewDataFromInvmon(string set_ym) {
            string sql = @"
                insert into MI_WHINV_DAILY(
                        data_date, wh_no, mmcode, INV_QTY, ONWAY_QTY, 
                        APL_INQTY, APL_OUTQTY, TRN_INQTY, TRN_OUTQTY,
                        ADJ_INQTY, ADJ_OUTQTY, BAK_INQTY, BAK_OUTQTY,
                        REJ_OUTQTY, DIS_OUTQTY, EXG_INQTY, EXG_OUTQTY,
                        MIL_INQTY, MIL_OUTQTY, INVENTORYQTY,use_qty,  back_qty
                       )
                select trunc(sysdate-1) as data_date,
                       a.wh_no, a.mmcode, a.INV_QTY, a.ONWAY_QTY,
                       a.APL_INQTY, a.APL_OUTQTY, a.TRN_INQTY, a.TRN_OUTQTY,
                       a.ADJ_INQTY, a.ADJ_OUTQTY, a.BAK_INQTY, a.BAK_OUTQTY ,
                       a.REJ_OUTQTY, a.DIS_OUTQTY, a.EXG_INQTY, a.EXG_OUTQTY,
                       a.MIL_INQTY, a.MIL_OUTQTY, a.INVENTORYQTY, a.use_qty, nvl(b.TR_INV_QTY, 0) as back_qty
                  from MI_WINVMON a
                  left join  (
                            with mnset_data as (
                                 select twn_date(set_btime) as set_btime,
                                        twn_date(nvl(set_etime, set_ctime)) as set_etime
                                   from MI_MNSET
                                  where (sysdate-1) between set_btime and nvl(set_etime, set_ctime)
                            )
                            select wh_no, mmcode, sum(AF_TR_INVQTY - BF_TR_INVQTY) TR_INV_QTY
                              FROM MI_WHTRNS A, mnset_data b
                             WHERE TR_MCODE = 'BAKI' AND TR_DOCTYPE = 'RS'
                               and SUBSTR (TR_DOCNO, 1, 7) between b.set_btime and b.set_etime
                             group by wh_no, mmcode
                       ) b on (a.wh_no = b.wh_no and a.mmcode = b.mmcode)
                 where 1=1
                   and a.data_ym = :set_ym
            ";
            return DBWork.Connection.Execute(sql, new { set_ym }, DBWork.Transaction);
        }
    }
}
