using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JCLib.DB.Tool;

namespace BDS007
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        // 用 view V_MMCODE_PRICE　更新　MM_PO_INREC[PO_PRICE]、[DISC_CPRICE]
        // 每日 07:10, 12:10 執行
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string msg_oracle = "error";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                string sql_oracle = @" update MM_PO_INREC a
                           set PO_PRICE =(select CONT_PRICE from V_MMCODE_PRICE where mmcode=a.mmcode and  begindate <= twn_date(accountdate) and twn_date(accountdate) <= enddate and rownum=1 ), 
                               DISC_CPRICE=(select DISC_CPRICE from V_MMCODE_PRICE where mmcode=a.mmcode and  begindate <= twn_date(accountdate) and twn_date(accountdate) <= enddate and rownum=1 ),  
                               UPDATE_TIME =SYSDATE, UPDATE_USER='AUTO',
                               PO_PRICE_OLD=(case when PO_PRICE_OLD is null then PO_PRICE else PO_PRICE_OLD end),
                               DISC_CPRICE_OLD=(case when DISC_CPRICE_OLD is null then DISC_CPRICE else DISC_CPRICE_OLD end) ,
                               isWilling = (select c.ISWILLING
                                              from MI_MAST b, MILMED_JBID_LIST c
                                             where a.MMCODE=b.MMCODE
                                               and substr(b.E_YRARMYNO,1,3)=c.JBID_STYR
                                               and b.E_ITEMARMYNO=c.BID_NO
                                           ),
                               discount_qty = (select c.discount_qty
                                                 from MI_MAST b, MILMED_JBID_LIST c
                                                where a.MMCODE=b.MMCODE
                                                  and substr(b.E_YRARMYNO,1,3)=c.JBID_STYR
                                                  and b.E_ITEMARMYNO=c.BID_NO
                                              ),
                               disc_cost_uprice = (select c.disc_cost_uprice
                                                     from MI_MAST b, MILMED_JBID_LIST c
                                                    where a.MMCODE=b.MMCODE
                                                      and substr(b.E_YRARMYNO,1,3)=c.JBID_STYR
                                                      and b.E_ITEMARMYNO=c.BID_NO
                                                  )
                         where TWN_YYYMM(accountdate) = TWN_YYYMM(sysdate)";
                callDbtools_oralce.CallExecSQL(sql_oracle, null, "oracle", ref msg_oracle);
                if (msg_oracle != "")
                {

                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS007 ", "程式錯誤:");
            }
            this.Close();
        }
    }
}
