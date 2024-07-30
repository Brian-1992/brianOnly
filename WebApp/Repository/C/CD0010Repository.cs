using System;
using System.Data;
using JCLib.DB;
using JCLib.DB.Tool;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;

using System.Linq;
using System.Web;

using System.Text;
using System.IO;
using System.Web.Mail;
using System.Net;
using System.Net.Sockets;

namespace WebApp.Repository.C
{
    public class CD0010ReportMODEL1 : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public int F9 { get; set; }
        public int F10 { get; set; }
        public int F11 { get; set; }
        public int F12 { get; set; }
        public int F13 { get; set; }
        public int F14 { get; set; }
        public int F15 { get; set; }
        public int F16 { get; set; }
        public string F17 { get; set; }
        public string F18 { get; set; }
        public string F19 { get; set; }
    }
    public class CD0010ReportMODEL2 : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public int F9 { get; set; }
        public int F10 { get; set; }
        public int F11 { get; set; }
        public int F12 { get; set; }
        public int F13 { get; set; }
        public int F14 { get; set; }
        public int F15 { get; set; }
        public string F16 { get; set; }
        public string F17 { get; set; }
        public string F18 { get; set; }
        public int F19 { get; set; }
        public string F20 { get; set; }
    }
    public class CD0010ReportMODEL3 : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public int F9 { get; set; }
        public int F10 { get; set; }
        public int F11 { get; set; }
        public int F12 { get; set; }
        public int F13 { get; set; }
        public int F14 { get; set; }
        public int F15 { get; set; }
        public string F16 { get; set; }
        public string F17 { get; set; }
        public string F18 { get; set; }
        public int F19 { get; set; }
        public string F20 { get; set; }
    }
    public class CD0010Repository : JCLib.Mvc.BaseRepository
    {
        
        String sBr = "\r\n";
        //L l = new L("CD0010Respository");
        FL l = new FL("CD0010Respository");

        public CD0010Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        
        public IEnumerable<CD0010> GetAllM(CD0010 v, string INID, int page_index, int page_size, string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();

            var sql = "";
            sql += sBr + "select ";
            sql += sBr + "docno,";
            sql += sBr + "(select flowname from me_flow where flowid=(select flowid from ME_DOCM where docno=b.docno) and doctype=(select doctype from ME_DOCM where docno=b.docno)) as flowid_des, "; //單據狀態
            sql += sBr + "(select una from UR_ID where tuser=b.pick_userid) as pick_userid, ";
            sql += sBr + "(select appdept from me_docm where docno=b.docno) as appdept,";
            sql += sBr + "(  select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno = b.docno)  ) as appdeptname, ";
            sql += sBr + "(select una from UR_ID where tuser=(select appid from ME_DOCM where docno=b.docno)) as appname, ";
            sql += sBr + "sum(item_cnt) as item_sum,";
            sql += sBr + "sum(appqty) as appqty_sum,";
            sql += sBr + "(";
            sql += sBr + "	select lot_no from BC_WHPICKDOC where 1=1 ";
            if (v.WH_NO != "")
            {
                sP = ":p" + iP++;
                sql += " and wh_no = " + sP + " ";
                p.Add(sP, string.Format("{0}", v.WH_NO));
            }
            sql += sBr + "	and docno=b.docno";
            sql += sBr + ") as lot_no, ";
            sql += sBr + "nvl(sum(act_pick_item),0) as act_pick_item_sum, "; //揀貨項數 
            //sql += sBr + "sum(act_pick_qty) as act_pick_qty_sum,";
            sql += sBr + "nvl(sum(act_pick_qty),0) as act_pick_qty_sum, "; //補上NVL
            sql += sBr + "nvl(sum(diffitem),0) as diffitem_sum, "; //差異品項數
            //sql += sBr + "sum(diffqty) as diffqty_sum "; 
            sql += sBr + "nvl(sum(diffqty),0) as diffqty_sum ";  //補上NVL
            sql += sBr + "from (";
            sql += sBr + "	select ";
            sql += sBr + "	docno,";
            sql += sBr + "	1 as item_cnt, ";
            sql += sBr + "	appqty, pick_userid, ";
            sql += sBr + "  (select 1 from dual where act_pick_qty>0) as act_pick_item, "; //補上FROM
            sql += sBr + "	act_pick_qty, ";
            sql += sBr + "	(select 1 from dual where act_pick_qty<>appqty) as diffitem, "; //補上FROM
            sql += sBr + "	(act_pick_qty-appqty) as diffqty ";
            sql += sBr + "	from BC_WHPICK a";
            sql += sBr + "	where 1=1 ";
            if (v.WH_NO != "")
            {
                sP = ":p" + iP++;
                sql += " and wh_no = " + sP + " ";
                p.Add(sP, string.Format("{0}", v.WH_NO));
            }


            String pick_date_start = l.toYmd(v.PICK_DATE_START);
            String pick_date_end = l.toYmd(v.PICK_DATE_END);
            if (pick_date_end != "") // 結束揀貨日期+1
            {
                DateTime pick_date_end_add_1_day = DateTime.Parse(v.PICK_DATE_END).AddDays(1);
                pick_date_end = pick_date_end_add_1_day.ToString("yyyy/MM/dd");
            }
            if (
                pick_date_start != "" &&
                pick_date_end != ""
            )
            {
                //sql += sBr + "and pick_date>={開始揀貨日期} and pick_date<{結束揀貨日期}+1";
                sP = ":p" + iP++;
                sql += sBr + " and pick_date >= to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, string.Format("{0}", pick_date_start));

                sP = ":p" + iP++;
                sql += sBr + "     pick_date < to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", pick_date_end));
            }
            else if (pick_date_start != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and pick_date >= to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, string.Format("{0}", pick_date_start));
            }
            else if (pick_date_end != "")
            {
                sP = ":p" + iP++;
                sql += sBr + "     pick_date < to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", pick_date_end));
            }


            //sql += sBr + "--有輸入開始出庫日期條件時加這2個條件";
            //sql += sBr + "--and has_shipout='Y' ";
            //sql += sBr + "--and (select shipout_date from BC_WHPICK_SHIPOUT";
            //sql += sBr + "--      where wh_no={目前庫房號碼} ";
            //sql += sBr + "--        and docno=a.docno and seq=a.seq)>={開始出庫日期}";
            //sql += sBr + "--有輸入結束出庫日期條件時加這2個條件";
            //sql += sBr + "--and has_shipout='Y' ";
            //sql += sBr + "--and (select shipout_date from BC_WHPICK_SHIPOUT";
            //sql += sBr + "--      where wh_no={目前庫房號碼} ";
            //sql += sBr + "--        and docno=a.docno and seq=a.seq)<{結束出庫日期}+1 ";
            String shopout_date_start = l.toYmd(v.SHOPOUT_DATE_START);
            String shopout_date_end = l.toYmd(v.SHOPOUT_DATE_END); 
            if (shopout_date_end != "") // {結束出庫日期}+1
            {
                DateTime shopout_date_end_add_1_day = DateTime.Parse(v.SHOPOUT_DATE_END).AddDays(1);
                shopout_date_end = shopout_date_end_add_1_day.ToString("yyyy/MM/dd");
            }
            if (
                shopout_date_start != "" &&
                shopout_date_end != ""
            )
            {
                sql += sBr + "and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " >=to_date(" + sP + ",'yyyy/mm/dd') "; // {開始出庫日期}
                p.Add(sP, string.Format("{0}", shopout_date_start));


                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));

                sP = ":p" + iP++;
                sql += sBr + " <to_date(" + sP + ",'yyyy/mm/dd') "; // {結束出庫日期}+1
                p.Add(sP, string.Format("{0}", shopout_date_end));
            }
            else if (shopout_date_start != "")
            {
                sql += sBr + "and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " >=to_date(" + sP + ",'yyyy/mm/dd') "; // {開始出庫日期}
                p.Add(sP, string.Format("{0}", shopout_date_start));
            }
            else if (shopout_date_end != "")
            {
                sql += sBr + " and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " <to_date(" + sP + ",'yyyy/mm/dd') "; // {結束出庫日期}+1
                p.Add(sP, string.Format("{0}", shopout_date_end));
            }


            if (v.DOCNO != "")
            {
                //sql += sBr + "--有輸入申請單號條件時加這個條件";
                //sql += sBr + "--and docno={輸入的申請單號}";
                sP = ":p" + iP++;
                sql += sBr + "and docno=" + sP + " "; // {輸入的申請單號}
                p.Add(sP, string.Format("{0}", v.DOCNO));
            }

            if (v.APPDEPT != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and (select APPDEPT from ME_DOCM where DOCNO=a.DOCNO)=" + sP + " ";
                p.Add(sP, string.Format("{0}", v.APPDEPT));
            }

            if (v.MMCODE != "")
            {
                //sql += sBr + "--有輸入院內碼條件時加這個條件";
                //sql += sBr + "--and mmcode={輸入的院內碼}";
                sP = ":p" + iP++;
                sql += sBr + " and mmcode=" + sP + " "; // {輸入的院內碼}
                p.Add(sP, string.Format("{0}", v.MMCODE));
            }


            if (v.PICK_USERID != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and pick_userid=" + sP + " ";
                p.Add(sP, string.Format("{0}", v.PICK_USERID));
            }

            if (v.ACT_PICK_USERID != "")
            {
                //sql += sBr + "--有拉選揀貨人員條件時加這個條件";
                //sql += sBr + "--and act_pick_userid={拉選的揀貨人員帳號}";
                sP = ":p" + iP++;
                sql += sBr + " and act_pick_userid=" + sP + " ";
                p.Add(sP, string.Format("{0}", v.ACT_PICK_USERID));
            }


            if (v.HAS_APPQTY == "Y")
            {
                //sql += sBr + "	--揀貨差異條件有勾選無差異時加這個條件";
                //sql += sBr + "	--or appqty=act_pick_qty";
                sql += sBr + " and appqty=act_pick_qty  ";
            }
            else if (v.HAS_APPQTY == "N")
            {
                //sql += sBr + "	--揀貨差異條件有勾選有差異時加這個條件";
                //sql += sBr + "	--or act_pick_qty is null or appqty<>act_pick_qty ";
                sql += sBr + "and ( ";
                sql += sBr + "        act_pick_qty is null or ";
                sql += sBr + "        appqty<>act_pick_qty ";
                sql += sBr + ") ";
            }


            if (v.HAS_CONFIRMED == "Y")
            {
                //sql += sBr + "	--確認狀態條件有勾選已確認時加這個條件";
                //sql += sBr + "	--or has_confirmed='Y'";                
                sql += sBr + "and has_confirmed='Y' ";
            }
            else if (v.HAS_CONFIRMED == "N")
            {
                //sql += sBr + "	--確認狀態條件有勾選待確認時加這個條件";
                //sql += sBr + "	--or has_confirmed is null";
                sql += sBr + "and has_confirmed is null ";
            }


            if (v.HAS_SHOPOUT == "Y")
            {
                //sql += sBr + "	--出庫狀態條件有勾選已出庫時加這個條件";
                //sql += sBr + "	--or has_shipout='Y'";
                sql += sBr + "and has_shipout='Y' ";
            }
            else if (v.HAS_SHOPOUT == "N")
            {
                //sql += sBr + "	--出庫狀態條件有勾選待出庫時加這個條件";
                //sql += sBr + "	--or has_shipout is null";
                sql += sBr + "and has_shipout is null ";
            }

            sql += sBr + ") b group by docno, pick_userid ";
            sql += sBr + "";

            l.lg("GetAllM()", l.getDebugSql(sql, p));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0010>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        

        public IEnumerable<CD0010> GetAllD(CD0010 v, int page_index, int page_size, string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();

            var sql = "";
            sql += sBr + "select ";
            sql += sBr + "seq,";
            sql += sBr + "mmcode,";
            sql += sBr + "mmname_c,";
            sql += sBr + "mmname_e,";
            sql += sBr + "appqty,";
            sql += sBr + "base_unit,";
            sql += sBr + "store_loc,";
            sql += sBr + "(select TWN_DATE(apvtime) from ME_DOCD where docno=a.docno and seq=a.seq) as apvdate, "; //核撥日
            sql += sBr + "(select una from ur_id where tuser=a.act_pick_userid) as act_pick_username,";
            sql += sBr + "act_pick_qty,";
            sql += sBr + "(act_pick_qty - appqty) as diffqty, "; //差異件數
            sql += sBr + "has_confirmed,";
            sql += sBr + "boxno,";
            sql += sBr + "has_shipout";
            sql += sBr + "from BC_WHPICK a";
            sql += sBr + "where 1=1 ";

            if (v.WH_NO != "")
            {
                sP = ":p" + iP++;
                sql += " and wh_no = " + sP + " "; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO)); 
            }
            

            String pick_date_start = l.toYmd(v.PICK_DATE_START);
            String pick_date_end = l.toYmd(v.PICK_DATE_END);
            if (pick_date_end != "") // 結束揀貨日期+1
            {
                DateTime pick_date_end_add_1_day = DateTime.Parse(v.PICK_DATE_END).AddDays(1);
                pick_date_end = pick_date_end_add_1_day.ToString("yyyy/MM/dd");
            }
            if (
                pick_date_start != "" &&
                pick_date_end != ""
            )
            {
                //sql += sBr + "and pick_date>={開始揀貨日期} and pick_date<{結束揀貨日期}+1";
                sP = ":p" + iP++;
                sql += sBr + " and pick_date >= to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, string.Format("{0}", pick_date_start));

                sP = ":p" + iP++;
                sql += sBr + "     pick_date < to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", pick_date_end));
            }
            else if (pick_date_start != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and pick_date >= to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, string.Format("{0}", pick_date_start));
            }
            else if (pick_date_end != "")
            {
                sP = ":p" + iP++;
                sql += sBr + "     pick_date < to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", pick_date_end));
            }


            //sql += sBr + "--有輸入開始出庫日期條件時加這2個條件";
            //sql += sBr + "--and has_shipout='Y' ";
            //sql += sBr + "--and (select shipout_date from BC_WHPICK_SHIPOUT";
            //sql += sBr + "--      where wh_no={目前庫房號碼} ";
            //sql += sBr + "--        and docno=a.docno and seq=a.seq)>={開始出庫日期}";
            //sql += sBr + "--有輸入結束出庫日期條件時加這2個條件";
            //sql += sBr + "--and has_shipout='Y' ";
            //sql += sBr + "--and (select shipout_date from BC_WHPICK_SHIPOUT";
            //sql += sBr + "--      where wh_no={目前庫房號碼} ";
            //sql += sBr + "--        and docno=a.docno and seq=a.seq)<{結束出庫日期}+1 ";
            String shopout_date_start = l.toYmd(v.SHOPOUT_DATE_START);
            String shopout_date_end = l.toYmd(v.SHOPOUT_DATE_END);
            if (shopout_date_end != "") // {結束出庫日期}+1
            {
                DateTime shopout_date_end_add_1_day = DateTime.Parse(v.SHOPOUT_DATE_END).AddDays(1);
                shopout_date_end = shopout_date_end_add_1_day.ToString("yyyy/MM/dd");
            }
            if (
                shopout_date_start != "" &&
                shopout_date_end != ""
            )
            {
                sql += sBr + "and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " >=to_date(" + sP + ",'yyyy/mm/dd') "; // {開始出庫日期}
                p.Add(sP, string.Format("{0}", shopout_date_start));


                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));

                sP = ":p" + iP++;
                sql += sBr + " <to_date(" + sP + ",'yyyy/mm/dd') "; // {結束出庫日期}+1
                p.Add(sP, string.Format("{0}", shopout_date_end));
            }
            else if (shopout_date_start != "")
            {
                sql += sBr + "and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " >=to_date(" + sP + ",'yyyy/mm/dd') "; // {開始出庫日期}
                p.Add(sP, string.Format("{0}", shopout_date_start));
            }
            else if (shopout_date_end != "")
            {
                sql += sBr + "and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " <to_date(" + sP + ",'yyyy/mm/dd') "; // {結束出庫日期}+1
                p.Add(sP, string.Format("{0}", shopout_date_end));
            }


            if (v.DOCNO != "")
            {
                //sql += sBr + "and docno={點選的申請單號rs2.docno}";
                sP = ":p" + iP++;
                sql += sBr + "and docno=" + sP + " "; // {輸入的申請單號}
                p.Add(sP, string.Format("{0}", v.DOCNO));
            }


            if (v.MMCODE != "")
            {
                //sql += sBr + "--有輸入院內碼條件時加這個條件";
                //sql += sBr + "--and mmcode={輸入的院內碼}";
                sP = ":p" + iP++;
                sql += sBr + "and mmcode=" + sP + " "; // {輸入的院內碼}
                p.Add(sP, string.Format("{0}", v.MMCODE));
            }


            if (v.ACT_PICK_USERID != "")
            {
                //sql += sBr + "--有拉選揀貨人員條件時加這個條件";
                //sql += sBr + "--and act_pick_userid={拉選的揀貨人員帳號}";
                sP = ":p" + iP++;
                sql += sBr + "and act_pick_userid=" + sP + " "; // {輸入的院內碼}
                p.Add(sP, string.Format("{0}", v.ACT_PICK_USERID));
            }

            if (v.HAS_APPQTY == "Y")
            {
                //sql += sBr + "	--揀貨差異條件有勾選無差異時加這個條件";
                //sql += sBr + "	--or appqty=act_pick_qty";
                sql += sBr + "and appqty=act_pick_qty  ";
            }
            else if (v.HAS_APPQTY == "N")
            {
                //sql += sBr + "	--揀貨差異條件有勾選有差異時加這個條件";
                //sql += sBr + "	--or act_pick_qty is null or appqty<>act_pick_qty";
                sql += sBr + "and ( ";
                sql += sBr + "        act_pick_qty is null or ";
                sql += sBr + "        appqty<>act_pick_qty ";
                sql += sBr + ") ";
            }

            if (v.HAS_CONFIRMED == "Y")
            {
                //sql += sBr + "	--確認狀態條件有勾選已確認時加這個條件";
                //sql += sBr + "	--or has_confirmed='Y'";                
                sql += sBr + "and has_confirmed='Y' ";
            }
            else if (v.HAS_CONFIRMED == "N")
            {
                //sql += sBr + "	--確認狀態條件有勾選待確認時加這個條件";
                //sql += sBr + "	--or has_confirmed is null";
                sql += sBr + "and has_confirmed is null ";
            }


            if (v.HAS_SHOPOUT == "Y")
            {
                //sql += sBr + "	--出庫狀態條件有勾選已出庫時加這個條件";
                //sql += sBr + "	--or has_shipout='Y'";
                sql += sBr + "and has_shipout='Y' ";
            }
            else if (v.HAS_SHOPOUT == "N")
            {
                //sql += sBr + "	--出庫狀態條件有勾選待出庫時加這個條件";
                //sql += sBr + "	--or has_shipout is null";
                sql += sBr + "and has_shipout is null ";
            }


            l.lg("GetAllD()", l.getDebugSql(sql, p));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0010>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public DataTable GetExcel(CD0010 v)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();
            var sql = "";
            sql += " select " + sBr;
            sql += " docno 申請單號," + sBr;
            sql += " (select appdept from ME_DOCM where docno=a.docno) 申請單位, " + sBr;
            sql += " (select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno=a.docno)) 申請單位名稱, " + sBr;
            sql += " seq 項次," + sBr;
            sql += " mmcode 院內碼," + sBr;
            sql += " mmname_c 中文品名," + sBr;
            sql += " mmname_e 英文品名," + sBr;
            //sql += " appqty 申請數," + sBr;
            sql += " appqty 核撥數," + sBr;
            sql += " base_unit 撥補單位," + sBr;
            sql += " store_loc 儲位," + sBr;
            sql += " (select una from ur_id where tuser=a.act_pick_userid) 揀貨人員, " + sBr;
            //sql += " act_pick_qty 揀貨數," + sBr;
            sql += " act_pick_qty 已揀件數," + sBr;
            sql += " (act_pick_qty - appqty) 差異件數," + sBr; //新增差異件數
            sql += " has_confirmed 已確認," + sBr;
            sql += " boxno 物流箱號," + sBr;
            sql += " has_shipout 已出庫" + sBr;
            sql += " from BC_WHPICK a" + sBr;
            sql += " where 1=1 " + sBr;

            if (v.WH_NO != "")
            {
                sP = ":p" + iP++;
                sql += " and wh_no = " + sP + " "; // wh_no={目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
            }

            // and pick_date>={開始揀貨日期} and pick_date<{結束揀貨日期}+1 
            String pick_date_start = l.toYmd(v.PICK_DATE_START);
            String pick_date_end = l.toYmd(v.PICK_DATE_END);
            if (pick_date_end != "") // 結束揀貨日期+1
            {
                DateTime pick_date_end_add_1_day = DateTime.Parse(v.PICK_DATE_END).AddDays(1);
                pick_date_end = pick_date_end_add_1_day.ToString("yyyy/MM/dd");
            }
            if (
                pick_date_start != "" &&
                pick_date_end != ""
            )
            {
                //sql += sBr + "and pick_date>={開始揀貨日期} and pick_date<{結束揀貨日期}+1";
                sP = ":p" + iP++;
                sql += sBr + " and pick_date >= to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, string.Format("{0}", pick_date_start));

                sP = ":p" + iP++;
                sql += sBr + "     pick_date < to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", pick_date_end));
            }
            else if (pick_date_start != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and pick_date >= to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, string.Format("{0}", pick_date_start));
            }
            else if (pick_date_end != "")
            {
                sP = ":p" + iP++;
                sql += sBr + "     pick_date < to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", pick_date_end));
            }


            String shopout_date_start = l.toYmd(v.SHOPOUT_DATE_START);
            String shopout_date_end = l.toYmd(v.SHOPOUT_DATE_END);
            if (shopout_date_end != "") // {結束出庫日期}+1
            {
                DateTime shopout_date_end_add_1_day = DateTime.Parse(v.SHOPOUT_DATE_END).AddDays(1);
                shopout_date_end = shopout_date_end_add_1_day.ToString("yyyy/MM/dd");
            }
            if (
                shopout_date_start != "" &&
                shopout_date_end != ""
            )
            {
                sql += sBr + "and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " >=to_date(" + sP + ",'yyyy/mm/dd') "; // {開始出庫日期}
                p.Add(sP, string.Format("{0}", shopout_date_start));


                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));

                sP = ":p" + iP++;
                sql += sBr + " <to_date(" + sP + ",'yyyy/mm/dd') "; // {結束出庫日期}+1
                p.Add(sP, string.Format("{0}", shopout_date_end));
            }
            else if (shopout_date_start != "")
            {
                sql += sBr + "and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " >=to_date(" + sP + ",'yyyy/mm/dd') "; // {開始出庫日期}
                p.Add(sP, string.Format("{0}", shopout_date_start));
            }
            else if (shopout_date_end != "")
            {
                sql += sBr + "and has_shipout='Y' ";

                sP = ":p" + iP++;
                sql += sBr + "and (select shipout_date from BC_WHPICK_SHIPOUT where docno=a.docno and seq=a.seq and wh_no=" + sP + " )"; // {目前庫房號碼}
                p.Add(sP, string.Format("{0}", v.WH_NO));
                sP = ":p" + iP++;
                sql += sBr + " <to_date(" + sP + ",'yyyy/mm/dd') "; // {結束出庫日期}+1
                p.Add(sP, string.Format("{0}", shopout_date_end));
            }


            if (v.DOCNO != "")
            {
                //sql += sBr + "and docno={點選的申請單號rs2.docno}";
                sP = ":p" + iP++;
                sql += sBr + "and docno=" + sP + " "; // {輸入的申請單號}
                p.Add(sP, string.Format("{0}", v.DOCNO));
            }


            if (v.MMCODE != "")
            {
                //sql += sBr + "--有輸入院內碼條件時加這個條件";
                //sql += sBr + "--and mmcode={輸入的院內碼}";
                sP = ":p" + iP++;
                sql += sBr + "and mmcode=" + sP + " "; // {輸入的院內碼}
                p.Add(sP, string.Format("{0}", v.MMCODE));
            }


            if (v.ACT_PICK_USERID != "")
            {
                //sql += sBr + "--有拉選揀貨人員條件時加這個條件";
                //sql += sBr + "--and act_pick_userid={拉選的揀貨人員帳號}";
                sP = ":p" + iP++;
                sql += sBr + "and act_pick_userid=" + sP + " "; // {輸入的院內碼}
                p.Add(sP, string.Format("{0}", v.ACT_PICK_USERID));
            }


            if (v.HAS_APPQTY == "Y")
            {
                //sql += sBr + "	--揀貨差異條件有勾選無差異時加這個條件";
                //sql += sBr + "	--or appqty=act_pick_qty";
                sql += sBr + "and appqty=act_pick_qty  ";
            }
            else if (v.HAS_APPQTY == "N")
            {
                //sql += sBr + "	--揀貨差異條件有勾選有差異時加這個條件";
                //sql += sBr + "	--or act_pick_qty is null or appqty<>act_pick_qty";
                sql += sBr + "and ( ";
                sql += sBr + "        act_pick_qty is null or ";
                sql += sBr + "        appqty<>act_pick_qty ";
                sql += sBr + ") ";
            }


            if (v.HAS_CONFIRMED == "Y")
            {
                //sql += sBr + "	--確認狀態條件有勾選已確認時加這個條件";
                //sql += sBr + "	--or has_confirmed='Y'";                
                sql += sBr + "and has_confirmed='Y' ";
            }
            else if (v.HAS_CONFIRMED == "N")
            {
                //sql += sBr + "	--確認狀態條件有勾選待確認時加這個條件";
                //sql += sBr + "	--or has_confirmed is null";
                sql += sBr + "and has_confirmed is null ";
            }


            if (v.HAS_SHOPOUT == "Y")
            {
                //sql += sBr + "	--出庫狀態條件有勾選已出庫時加這個條件";
                //sql += sBr + "	--or has_shipout='Y'";
                sql += sBr + "and has_shipout='Y' ";
            }
            else if (v.HAS_SHOPOUT == "N")
            {
                //sql += sBr + "	--出庫狀態條件有勾選待出庫時加這個條件";
                //sql += sBr + "	--or has_shipout is null";
                sql += sBr + "and has_shipout is null ";
            }
            sql += sBr + "order by 項次 ";
            l.lg("GetExcel()", l.getDebugSql(sql, p));


            DataTable dt = new DataTable();
            //var sql2 = @"select Trim(INID_NAME(USER_INID(:USRID))) as INID_NAME_USER from PARAM_D where rownum = 1";
            //var title = DBWork.Connection.ExecuteScalar(sql2, new { USRID = v.USERID }, DBWork.Transaction).ToString()
            //        + v.APPTIME_START + "至" + v.APPTIME_END + "繳回入帳明細報表";
            //dt.Columns.Add(title);


            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        } // end of GetExcel(CD0010 v)

        public bool CheckUserPh1s(string id)
        {
            string sql = @"select 'Y' as IS_MEDWH_USER 
                            from BC_WHID a
                           where wh_no in ('PH1S','PH1X') and wh_userid=:WH_USERID ";

            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction) == null);
            //string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            //return rtn;
        }
        public string GetUridInid(string id)
        {
            string sql = @"SELECT INID FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public IEnumerable<COMBO_MODEL> GetAppdeptCombo(string wh_no)
        {
            string sql = @"
                            select distinct
                            APPDEPT as VALUE, 
                            APPDEPT || ' ' || (select INID_NAME from UR_INID where INID=a.APPDEPT) as TEXT,
                            '' COMBITEM
                            from ME_DOCM a
                            where A.FRWH=:WH_NO
                            and A.APPDEPT is not null
                            order by APPDEPT ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { WH_NO = wh_no });
        }

        public IEnumerable<COMBO_MODEL> GetActPickUseridCombo(string wh_no)
        {
            string sql = @"
                            select 
                            wh_userid VALUE, 
                            (select una from UR_ID where tuser=a.wh_userid) as TEXT,
                            '' COMBITEM
                            from BC_WHID a
                            where 1=1 
                            and exists(select 1 from ur_id where  tuser=a.wh_userid) -- 去除a.wh_userid不存在ur_id.tuser的資料
                            and WH_NO=:wh_no
                            order by TEXT";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { WH_NO = wh_no });
        }


        public IEnumerable<COMBO_MODEL> GetDocnopkCombo(string p0, string p1)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO as COMBITEM
                        FROM MM_PACK_M 
                        WHERE 1=1 AND DOCTYPE = 'MR1' 
                        AND MAT_CLASS=:MAT_CLASS
                        AND APPDEPT=:APPDEPT
                        ORDER BY DOCNO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MAT_CLASS = p0, APPDEPT = p1 });
        }
        public IEnumerable<COMBO_MODEL> GetDocpknoteCombo(string p0, string p1)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, APPLY_NOTE as TEXT,
                        APPLY_NOTE as COMBITEM
                        FROM MM_PACK_M 
                        WHERE 1=1 AND DOCTYPE = 'MR1' 
                        AND MAT_CLASS=:MAT_CLASS
                        AND APPDEPT=:APPDEPT
                        ORDER BY DOCNO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MAT_CLASS = p0, APPDEPT = p1 });
        }
        public IEnumerable<COMBO_MODEL> GetDocnoCombo()
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO as COMBITEM
                        FROM BC_WHPICK 
                        WHERE 1=1 
                        ORDER BY DOCNO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetApplyKindCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string p2, string docno, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS AVG_APLQTY ,
                        ( SELECT HIGH_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS HIGH_QTY,
                        NVL(E.TOT_APVQTY,0) TOT_APVQTY 
                        FROM MI_MAST A 
                        INNER JOIN ME_DOCM C ON C.DOCNO=:DOCNO
                        LEFT OUTER JOIN MI_WINVCTL D ON D.MMCODE = A.MMCODE AND D.WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')
                        LEFT OUTER JOIN V_MM_TOTAPL E ON E.DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and E.MMCODE=A.MMCODE
                        WHERE 1=1 
                          AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_STOREID = '1'
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":MAT_CLASS", p1);
            p.Add(":TOWH", p2);
            p.Add(":DOCNO", docno);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO ) AS INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO ) AS AVG_APLQTY 
                        FROM MI_MAST A WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_STOREID = '1'
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' ";

            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }
            p.Add(":MAT_CLASS", query.MAT_CLASS);
            p.Add(":WH_NO", query.WH_NO);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A, ME_DOCD B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":DOCNO", p1);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetTowhCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM 
                        FROM MI_WHMAST A,UR_ID B
                        WHERE A.INID=B.INID 
                        AND A.WH_KIND = '1' 
                        AND TUSER=:TUSER 
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='3'   
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetReasonCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='GTAPL_REASON' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<CD0010> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        :UPDATE_IP,
                        (SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 ) MR2,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_EDAY')),'N') MR3,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4 
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<CD0010>(sql, new { TUSER = id, UPDATE_IP = ip });
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }


        public IEnumerable<CD0010ReportMODEL1> GetPrintData1(string wh_no, string date1 , string date2 , string[] docno)
        {
            var p = new DynamicParameters();

            var sql = @"select wh_no F1,(select wh_name from MI_WHMAST where wh_no=a.wh_no) as F2,docno F3,
                        (select appdept from ME_DOCM where docno=a.docno)||'_'||(select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno=a.docno)) as F4,
                        (select TWN_TIME(apptime) from ME_DOCM where docno=a.docno) as F5,
                        mmcode F6,mmname_e F7,base_unit F8,
                        (select safe_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode) as F9,
                        (select oper_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode) as F10,
                        NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0) as F11,
                        NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual where a.wh_no='PH1S')||(select whno_1x('1') as wh_no from dual where a.wh_no<>'PH1S') and mmcode='004'||substr(a.mmcode,4,10)),0) as F12,
                        NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0)+NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual where a.wh_no='PH1S')||(select whno_1x('1') as wh_no from dual where a.wh_no<>'PH1S') and mmcode='004'||substr(a.mmcode,4,10)),0) as F13,
                        0 as F14,
                        NVL(appqty,0) F15,NVL(act_pick_qty,0) F16,store_loc F17,
                        (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as F18
                                from BC_WHPICK a 
                        where 1 = 1 ";
            if (!string.IsNullOrEmpty(wh_no))
            {
                sql += @" and a.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }
            if (date1 != "" & date2 != "")
            {
                sql += @" AND TWN_DATE(a.pick_date) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", date1));
                p.Add(":d1", string.Format("{0}", date2));
            }
            if (docno.Length > 0)
            {
                sql += @" AND a.DOCNO IN :DOCNO ";
                p.Add("DOCNO", docno);
            }
            sql += @"order by docno,seq ";

            //先將查詢結果暫存在tmp_CD0010ReportMODEL1，接著產生BarCode的資料
            IEnumerable<CD0010ReportMODEL1> tmp_CD0010ReportMODEL1 = DBWork.Connection.Query<CD0010ReportMODEL1>(sql, p);
            //================================產生BarCode的資料=======================================
            Barcode tmp_BarCode = new Barcode();

            foreach (CD0010ReportMODEL1 tmp_CD0010Data in tmp_CD0010ReportMODEL1)
            {
                TYPE type = TYPE.CODE128;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        Bitmap img_BarCode = (Bitmap)tmp_BarCode.Encode(type, tmp_CD0010Data.F3, 550, 45);

                        img_BarCode.Save(ms, ImageFormat.Jpeg);
                        byte[] byteImage = new Byte[ms.Length];
                        byteImage = ms.ToArray();
                        string strB64 = Convert.ToBase64String(byteImage);
                        tmp_CD0010Data.F19 = strB64;
                    }
                    catch (FormatException ex)
                    {
                        tmp_CD0010Data.F19 = null;
                    }
                }
            }
            //================================產生BarCode的資料=======================================

            //return DBWork.Connection.Query<CD0010ReportMODEL1>(sql, p);
            return tmp_CD0010ReportMODEL1;
            //return DBWork.Connection.Query<CD0010ReportMODEL1>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CD0010ReportMODEL2> GetPrintData2(string wh_no, string date1, string date2, string docno)
        {
            var p = new DynamicParameters();

            var sql = @"select wh_no F1,
                        (select wh_name from MI_WHMAST where wh_no=a.wh_no) as F2,docno F3,
                        (select appdept from ME_DOCM where docno=a.docno)||'_'||(select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno=a.docno)) as F4,
                        (select TWN_TIME(apptime) from ME_DOCM where docno=a.docno) as F5,mmcode F6,mmname_e F7,base_unit F8,
                        (select safe_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode) as F9,
                        (select oper_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode) as F10,
                        NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0) as F11,
                        NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual where a.wh_no='PH1S')||(select whno_1x('1') as wh_no from dual where a.wh_no<>'PH1S') and mmcode='004'||substr(a.mmcode,4,10)),0) as F12,
                        NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0)+NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual where a.wh_no='PH1S')||(select whno_1x('1') as wh_no from dual where a.wh_no<>'PH1S') and mmcode='004'||substr(a.mmcode,4,10)),0) as F13,
                        NVL(appqty,0)F14,NVL(act_pick_qty,0)F15,store_loc F16,
                        (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as F17,
                        (select una from UR_ID where tuser=(select appid from ME_DOCM where docno=a.docno)) as F18,
                        (select use_box_qty from BC_WHPICKDOC where docno=a.docno) as F19
                         from BC_WHPICK a 
                        where 1 = 1 and (act_pick_qty is null or act_pick_qty=0) ";
            if (!string.IsNullOrEmpty(wh_no))
            {
                sql += @" and a.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }
            if (date1 != "" & date2 != "")
            {
                sql += @" AND TWN_DATE(a.pick_date) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", date1));
                p.Add(":d1", string.Format("{0}", date2));
            }
            if (docno.Length > 0)
            {
                sql += @" AND a.DOCNO IN :DOCNO ";
                p.Add("DOCNO", docno);
            }
            sql += @"order by substr(store_loc,3,18) ";

            //先將查詢結果暫存在tmp_CD0010ReportMODEL2，接著產生BarCode的資料
            IEnumerable<CD0010ReportMODEL2> tmp_CD0010ReportMODEL2 = DBWork.Connection.Query<CD0010ReportMODEL2>(sql, p);
            //================================產生BarCode的資料=======================================
            Barcode tmp_BarCode = new Barcode();

            foreach (CD0010ReportMODEL2 tmp_CD0010Data in tmp_CD0010ReportMODEL2)
            {
                TYPE type = TYPE.CODE128;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        Bitmap img_BarCode = (Bitmap)tmp_BarCode.Encode(type, tmp_CD0010Data.F3, 550, 45);

                        img_BarCode.Save(ms, ImageFormat.Jpeg);
                        byte[] byteImage = new Byte[ms.Length];
                        byteImage = ms.ToArray();
                        string strB64 = Convert.ToBase64String(byteImage);
                        tmp_CD0010Data.F20 = strB64;
                    }
                    catch (FormatException ex)
                    {
                        tmp_CD0010Data.F20 = null;
                    }
                }
            }
            //================================產生BarCode的資料=======================================

            //return DBWork.Connection.Query<CD0010ReportMODEL2>(sql, p);
            return tmp_CD0010ReportMODEL2;
            //return DBWork.Connection.Query<CD0010ReportMODEL1>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CD0010ReportMODEL3> GetPrintData3(string wh_no, string date1, string date2, string docno)
        {
            var p = new DynamicParameters();

            var sql = @"select wh_no F1,(select wh_name from MI_WHMAST where wh_no=a.wh_no) as F2,docno F3,
                        (select appdept from ME_DOCM where docno=a.docno)||'_'||(select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno=a.docno)) as F4,
                        (select TWN_TIME(apptime) from ME_DOCM where docno=a.docno) as F5,mmcode F6,mmname_e F7,base_unit F8,
                        NVL((select safe_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode),0) as F9,
                        NVL((select oper_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode),0) as F10,
                        NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0) as F11,
                        NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual where a.wh_no='PH1S')||(select whno_1x('1') as wh_no from dual where a.wh_no<>'PH1S') and mmcode='004'||substr(a.mmcode,4,10)),0) as F12,
                        NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0)+NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual where a.wh_no='PH1S')||(select whno_1x('1') as wh_no from dual where a.wh_no<>'PH1S') and mmcode='004'||substr(a.mmcode,4,10)),0) as F13,
                        NVL(appqty,0) F14,
                        NVL(act_pick_qty,0) F15,
                        store_loc F16,
                        (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as F17,
                        (select una from UR_ID where tuser=(select appid from ME_DOCM where docno=a.docno)) as F18,
                        (select use_box_qty from BC_WHPICKDOC where docno=a.docno) as F19 
                         from BC_WHPICK a 
                        where 1 = 1 and act_pick_userid is not null ";
            if (!string.IsNullOrEmpty(wh_no))
            {
                sql += @" and a.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }
            if (date1 != "" & date2 != "")
            {
                sql += @" AND TWN_DATE(a.pick_date) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", date1));
                p.Add(":d1", string.Format("{0}", date2));
            }
            if (docno.Length > 0)
            {
                sql += @" AND a.DOCNO IN :DOCNO ";
                p.Add("DOCNO", docno);
            }
            sql += @"order by seq ";

            //先將查詢結果暫存在tmp_CD0010ReportMODEL3，接著產生BarCode的資料
            IEnumerable<CD0010ReportMODEL3> tmp_CD0010ReportMODEL3 = DBWork.Connection.Query<CD0010ReportMODEL3>(sql, p);
            //================================產生BarCode的資料=======================================
            Barcode tmp_BarCode = new Barcode();

            foreach (CD0010ReportMODEL3 tmp_CD0010Data in tmp_CD0010ReportMODEL3)
            {
                TYPE type = TYPE.CODE128;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        Bitmap img_BarCode = (Bitmap)tmp_BarCode.Encode(type, tmp_CD0010Data.F3, 550, 45);

                        img_BarCode.Save(ms, ImageFormat.Jpeg);
                        byte[] byteImage = new Byte[ms.Length];
                        byteImage = ms.ToArray();
                        string strB64 = Convert.ToBase64String(byteImage);
                        tmp_CD0010Data.F20 = strB64;
                    }
                    catch (FormatException ex)
                    {
                        tmp_CD0010Data.F20 = null;
                    }
                }
            }
            //================================產生BarCode的資料=======================================

            //return DBWork.Connection.Query<CD0010ReportMODEL3>(sql, p);
            return tmp_CD0010ReportMODEL3;
            //return DBWork.Connection.Query<CD0010ReportMODEL1>(sql, p, DBWork.Transaction);
        }
        public string getUserName(string userId)
        {
            string sql = @" select UNA from UR_ID where TUSER = :userID ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = userId }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }
        public int UpdateBoxqty(string docno, int qty)
        {
            var sql = @"UPDATE BC_WHPICKDOC SET USE_BOX_QTY = :USE_BOX_QTY WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, USE_BOX_QTY = qty }, DBWork.Transaction);
        }
        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM BC_WHPICKDOC WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public string GetUse_box_qty(string docno, string whno)
        {
            string sql = @" SELECT USE_BOX_QTY FROM BC_WHPICKDOC WHERE DOCNO = :DOCNO AND WH_NO=:WH_NO";

            var str = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, WH_NO = whno }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }

    }
}
