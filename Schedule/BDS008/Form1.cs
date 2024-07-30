using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using JCLib.DB.Tool;
using Tsghmm.Utility;
using BDS008.WebSvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace BDS008
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //衛材處理
            // MatClassNotEqual_01();
            //藥品衛材比照藥品的範本統一處理
            MatClassEqual_01();
            //衛材新訊息通知廠商處理 PH_MSGMAIL
            //DO_MSGMAIL();
            this.Close();
        }

        //衛材處理
        private void MatClassNotEqual_01()
        {
            try
            {
                string msg_oracle = "error";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " select PO_NO,MAT_CLASS,AGEN_NO, ISCR from MM_PO_M where PO_STATUS='84' and MAT_CLASS <> '01' ";
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_oralce.Rows.Count; i++) //有幾筆資料 就是要寄幾封信
                    {
                        string mailType = "衛材訂單";
                        string mailSubject = GetHospName() + "衛材訂購單";
                        if (!dt_oralce.Rows[i]["MAT_CLASS"].ToString().Equals("02"))
                        {
                            mailType = "一般物品訂單";
                            mailSubject = GetHospName() + "一般物品訂購單";
                        }
                        int rowsAffected_oracle = -1;
                        //撈出信件基本資料
                        DataTable dtM = new DataTable();
                        dtM = GetMailBasicData(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                        //撈出信件訂單內容
                        DataTable dtD1 = new DataTable();
                        dtD1 = GetMailDt1_MatClassNotEqual_01(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                        //撈出信件單位非庫備品申請明細
                        DataTable dtD2 = new DataTable();
                        // dtD2 = GetMailDt2_MatClassNotEqual_01(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                        //組出信件內容
                        string mailBodyAll = GetMailContent_MatClassNotEqual_01(dtM, dtD1, dtD2);
                        string[] mailBodtAllArray = Regex.Split(mailBodyAll, "BDS008信件內容分割", RegexOptions.IgnoreCase);
                        string mailContent = mailBodtAllArray[0];
                        string mailContentLog = mailBodtAllArray[1];
                        SendMail sendmail = new SendMail();
                        CallDBtools calldbtools = new CallDBtools();
                        bool sendMailFlag = false;
                        // 針對 廠商 000,300,990,999 不寄mail
                        // if ("000,300,990,999".IndexOf(dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()) > -1)
                        //     sendMailFlag = true;
                        // else
                        //sendMailFlag = sendmail.Send_Mail(dt_oralce.Rows[i]["PO_NO"].ToString().Trim(), calldbtools.GetDefaultMailSender(), dtM.Rows[0]["EMAIL"].ToString().Trim(), calldbtools.GetDefaultMailCC(), mailType, mailSubject + dtM.Rows[0]["PO_NO"].ToString().Trim(), mailContent, dtM.Rows[0]["UPDATE_USER"].ToString().Trim(), mailContentLog, new List<string>());
                        if (sendMailFlag) //寄件成功
                        {
                            //if (dtM.Rows[0]["ISCOPY"].ToString().Trim() == "N")
                            //{
                            //    //將 MM_PO_M,MM_PO_D 複製到MS資料庫 WB_MM_PO_M,WB_MM_PO_D
                            //    SelectOracleMM_POintoMsWB_MM_PO(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                            //    //將 oracle 非庫備品申請單位資料 寫入 oracle PH_PO_N
                            //    InsertPH_PO_N(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                            //    //若 isCR='Y'，則 增加insert 外網的WB_REPLY
                            //    if (dt_oralce.Rows[i]["ISCR"].ToString().Trim() == "Y")
                            //    {
                            //        InsertWB_REPLY(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                            //    }
                            //}
                            //更新狀態
                            sql_oracle = " update MM_PO_M set PO_STATUS = '82', ISCOPY = 'Y', update_time=sysdate ";
                            sql_oracle += " where PO_NO=:po_no ";
                            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", dt_oralce.Rows[i]["PO_NO"].ToString().Trim()));
                            rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                            if (msg_oracle != "")
                                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)STEP12-更新MM_PO_M失敗:" + dt_oralce.Rows[i]["PO_NO"].ToString().Trim(), "AUTO");
                        }
                        else //寄件失敗
                        {
                            callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)STEP5-寄件失敗:" + dt_oralce.Rows[i]["PO_NO"].ToString().Trim(), "AUTO");
                        }
                    }
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)STEP1-取得MM_PO_M失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS008", "(衛材)程式錯誤:");
            }
        }

        //藥材處理
        private void MatClassEqual_01()
        {
            try
            {
                // 設定排程時的預設路徑會在Windows/System32底下,若沒有最高權限會無法存取路徑(如北投環境),可考慮指定一個有權限的路徑
                // string filePath = "C://BDS008//";
                string filePath = "BDS008//";
                string fileFullPath = "";
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string msg_oracle = "error";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();
                //string sql_oracle = " select PO_NO,PO_STATUS,AGEN_NO,M_CONTID, iscr from MM_PO_M where PO_STATUS in ('84','88') ";
                string sql_oracle = " select seq, detail_value as PO_NO, send_user_email, receive_email, send_user, user_name(send_user) as send_user_name, mail_type from SEND_MAIL_LOG where is_send ='N' ";
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_oralce.Rows.Count; i++) //有幾筆資料 就是要寄幾封信
                    {
                        switch (dt_oralce.Rows[i]["mail_type"].ToString())
                        {
                            case "1": // 訂單
                                {
                                    int rowsAffected_oracle = -1;
                                    //撈出信件基本資料
                                    DataTable dtM = new DataTable();
                                    dtM = GetMailBasicData_01(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                                    //撈出信件訂單內容
                                    DataTable dtD1 = new DataTable();
                                    dtD1 = GetMailDt1_MatClassEqual_01(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                                    //取得信件文字內容(先顯示後再加上訂單內容html)
                                    DataTable dtMC = new DataTable();
                                    dtMC = GetMailContentByMatClass(dtM.Rows[0]["MAT_CLASS"].ToString(), dt_oralce.Rows[i]["send_user"].ToString());
                                    //組出信件內容
                                    string mailBodyAll = GetMailContent_MatClassEqual_01(dtM, dtD1, dtMC);
                                    string[] mailBodtAllArray = Regex.Split(mailBodyAll, "BDS008信件內容分割", RegexOptions.IgnoreCase);
                                    string mailContent = mailBodtAllArray[0];
                                    string mailContentLog = mailBodtAllArray[1];
                                    string hosp_name = GetHospName();
                                    string mailSubject = hosp_name + "訂購單"; //"藥材訂單合約" + dtM.Rows[0]["AGEN_NO"].ToString().Trim()
                                    if (dtM.Rows[0]["PO_STATUS"].ToString().Trim() == "88")
                                    {
                                        mailSubject += "-補發" + dtM.Rows[0]["PO_NO"].ToString().Trim();
                                    }
                                    else
                                    {
                                        mailSubject += dtM.Rows[0]["PO_NO"].ToString().Trim();
                                    }
                                    SendMail sendmail = new SendMail();
                                    CallDBtools calldbtools = new CallDBtools();

                                    fileFullPath = filePath + dtM.Rows[0]["PO_NO"].ToString().Trim() + ".xls";
                                    CreateEmailAttFile(fileFullPath, dtM, dtD1);
                                    List<string> attFiles = new List<string>();
                                    attFiles.Add(fileFullPath);

                                    bool sendMailFlag = false;
                                    //針對 廠商 000,300,990,999 不寄mail
                                    if ("000,300,990,999".IndexOf(dtM.Rows[0]["AGEN_NO"].ToString().Trim()) > -1)
                                    {
                                        sendMailFlag = true;
                                    }
                                    else
                                    {
                                        //sendMailFlag = sendmail.Send_Mail(dt_oralce.Rows[i]["PO_NO"].ToString().Trim(), calldbtools.GetDefaultMailSender(), dtM.Rows[0]["EMAIL"].ToString().Trim(), calldbtools.GetDefaultMailCC(), "藥材訂單", mailSubject, mailContent, dtM.Rows[0]["UPDATE_USER"].ToString().Trim(), mailContentLog, attFiles);
                                        sendMailFlag = sendmail.Send_Mail(dt_oralce.Rows[i]["seq"].ToString(), dt_oralce.Rows[i]["PO_NO"].ToString().Trim(), GetHospName()
                                            , dt_oralce.Rows[i]["send_user_name"].ToString(), dt_oralce.Rows[i]["send_user_email"].ToString(), dtM.Rows[0]["EMAIL"].ToString().Trim()
                                            , calldbtools.GetDefaultMailCC(), "藥材訂單", mailSubject, mailContent, dtM.Rows[0]["UPDATE_USER"].ToString().Trim(), mailContentLog, attFiles);
                                    }
                                    if (sendMailFlag) //寄件成功
                                    {
                                        //if (dtM.Rows[0]["ISCOPY"].ToString().Trim() == "N")
                                        //{
                                        //    //將 MM_PO_M,MM_PO_D 複製到MS資料庫 WB_MM_PO_M,WB_MM_PO_D
                                        //    SelectOracleMM_POintoMsWB_MM_PO(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                                        //}
                                        //else //已 copy 用 update WB_MM_PO_D[PO_QTY]
                                        //{
                                        //    UpdateWB_MM_PO_D(dt_oralce.Rows[i]["PO_NO"].ToString().Trim());
                                        //}
                                        //更新狀態
                                        sql_oracle = " update MM_PO_M set PO_STATUS = '82', ISCOPY = 'Y', update_time=sysdate ";
                                        sql_oracle += " where PO_NO=:po_no ";
                                        List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", dt_oralce.Rows[i]["PO_NO"].ToString().Trim()));
                                        rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                                        if (msg_oracle != "")
                                            callDbtools_oralce.I_ERROR_LOG("BDS008", "(藥材)STEP12-更新MM_PO_M失敗:" + dt_oralce.Rows[i]["PO_NO"].ToString().Trim(), "AUTO");
                                    }
                                    else //寄件失敗
                                    {
                                        callDbtools_oralce.I_ERROR_LOG("BDS008", "(藥材)STEP4-寄件失敗:" + dt_oralce.Rows[i]["PO_NO"].ToString().Trim(), "AUTO");
                                    }

                                    if (File.Exists(fileFullPath))
                                    {
                                        File.Delete(fileFullPath);
                                    }
                                }
                                break;
                            case "2": // 匯款單
                                break;
                            case "3": // 對帳單
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS008", "(藥材)STEP1-取得MM_PO_M失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS008", "(藥材)程式錯誤:");
            }
        }

        //衛材新訊息通知廠商處理 PH_MSGMAIL
        private void DO_MSGMAIL()
        {
            try
            {
                #region 2021-12-30新增 刪除已下載檔案
                // 檢查資料夾是否存在
                string base_filename = @"C:\\BDS008_files";
                if (!Directory.Exists(base_filename))
                {
                    Directory.CreateDirectory(base_filename);
                }
                // 清空資料夾
                DirectoryInfo di = new DirectoryInfo(base_filename);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                #endregion

                string msg_oracle = "error";
                int rowsAffected_oracle = -1;
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();

                string sql_oracle = @"select a.DOCID, THEME, MSG, b.AGEN_NO, b.UPDATE_USER,
                                         case when a.UPDATE_USER is null then(select UNA from UR_ID where tuser = a.create_user)
                                         else  (select UNA from UR_ID where tuser = a.update_user)  end as USERNAME,
                                         c.EMAIL
                                       from PH_MSGMAIL_M a,  PH_MSGMAIL_AGEN b, PH_VENDER c
                                       where a.docid=b.docid and a.STATUS = '84' and b.STATUS= '84' 
                                             and b.agen_no=c.agen_no
                                        ";
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_oralce.Rows.Count; i++) //有幾筆資料 就是要寄幾封信
                    {
                        // 取得附件
                        DataTable d_dt_oralce = new DataTable();
                        List<string> attachmentNames = new List<string>();


                        sql_oracle = @"select b.* 
                                         from PH_MSGMAIL_D a, UR_UPLOAD b
                                        where a.docid = :docid
                                          and a.filename = b.uk";
                        List<CallDBtools_Oracle.OracleParam> paraList2 = new List<CallDBtools_Oracle.OracleParam>();
                        paraList2.Add(new CallDBtools_Oracle.OracleParam(1, ":docid", "VarChar", dt_oralce.Rows[i]["DOCID"].ToString()));
                        string msg_oracle_1 = string.Empty;
                        d_dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList2, "oracle", "T1", ref msg_oracle_1);
                        if (msg_oracle_1 == "" && d_dt_oralce.Rows.Count > 0)
                        {
                            IFileService fs_client = new FileServiceClient();

                            RemoteFileInfo info = new RemoteFileInfo();

                            for (int j = 0; j < d_dt_oralce.Rows.Count; j++)
                            {
                                DownloadRequest req = new DownloadRequest(d_dt_oralce.Rows[j]["FG"].ToString());

                                info = fs_client.DownloadFile(req);

                                string filename = string.Format("{0}\\{1}", base_filename, d_dt_oralce.Rows[j]["FN"].ToString());
                                using (Stream file = File.Create(filename))
                                {
                                    CopyStream(info.FileByteStream, file);
                                }

                                attachmentNames.Add(filename);
                            }

                        }

                        //組出信件內容
                        string mailContent = GetMailContent_MSGMAIL(dt_oralce);
                        string mailSubject = GetHospName() + "中央庫房通知-" + dt_oralce.Rows[i]["THEME"].ToString();
                        SendMail sendmail = new SendMail();
                        CallDBtools calldbtools = new CallDBtools();
                        bool sendMailFlag = false;
                        if (dt_oralce.Rows[i]["EMAIL"] == null || dt_oralce.Rows[i]["EMAIL"].ToString().Trim() == "")
                        {
                            callDbtools_oralce.I_ERROR_LOG("BDS008", "STEP4-新訊息通知廠商MSGMAIL失敗寄件失敗:沒有mail address--" + dt_oralce.Rows[i]["DOCID"].ToString().Trim() + "-" + dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim(), "AUTO");
                        }
                        else
                        {
                            //sendMailFlag = sendmail.Send_Mail(dt_oralce.Rows[i]["DOCID"].ToString().Trim(), calldbtools.GetDefaultMailSender(), dt_oralce.Rows[i]["EMAIL"].ToString().Trim(), calldbtools.GetDefaultMailCC(), "新訊息通知廠商", mailSubject, mailContent, dt_oralce.Rows[i]["UPDATE_USER"].ToString().Trim(), "", attachmentNames);
                            if (sendMailFlag) //寄件成功
                            {
                                //更新狀態
                                sql_oracle = " update PH_MSGMAIL_AGEN set STATUS = '82', update_time=sysdate ";
                                sql_oracle += " where DOCID=:docid and AGEN_NO=:agen_no and STATUS = '84'";
                                List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                                paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":docid", "VarChar", dt_oralce.Rows[i]["DOCID"].ToString().Trim()));
                                paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                                rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                                if (msg_oracle != "")
                                    callDbtools_oralce.I_ERROR_LOG("BDS008", "STEP12-更新新訊息通知廠商PH_MSGMAIL失敗:" + dt_oralce.Rows[i]["DOCID"].ToString().Trim(), "AUTO");
                            }
                            else //寄件失敗
                            {
                                callDbtools_oralce.I_ERROR_LOG("BDS008", "STEP4-新訊息通知廠商MSGMAIL失敗寄件失敗:" + dt_oralce.Rows[i]["DOCID"].ToString().Trim() + "-" + dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim(), "AUTO");
                            }
                        }
                    }
                    //針對 1天以上尚未寄送完成mail，結案 status='86' --傳送失敗結案
                    sql_oracle = @" update PH_MSGMAIL_AGEN  set STATUS = '86', UPDATE_TIME=sysdate 
                                          where status='84' and sysdate-create_time > 0.25 ";
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, null, "oracle", ref msg_oracle);
                    //針對 PH_MSGMAIL_M，結案 status='82' --已傳MAIL
                    sql_oracle = " update PH_MSGMAIL_M a set STATUS = '82', UPDATE_TIME=sysdate ";
                    sql_oracle += " where a.status='84' and (select count(*) from PH_MSGMAIL_AGEN where docid=a.docid and status='84')=0 ";
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, null, "oracle", ref msg_oracle);
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS008", "STEP1-取得新訊息通知廠商 PH_MSGMAIL_M失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS008", "(藥材)程式錯誤:");
            }
        }

        //撈出信件基本資料(衛材/藥材) round(b.PO_PRICE*b.PO_QTY)
        private DataTable GetMailBasicData_01(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            //string sql_oracle = " select a.PO_NO, a.PO_STATUS, a.M_CONTID, a.AGEN_NO, c.AGEN_NAMEC, c.AGEN_TEL, c.AGEN_FAX, c.EMAIL, a.MAT_CLASS, ";
            //sql_oracle += "              a.MEMO, a.SMEMO, a.UPDATE_USER, a.ISCOPY, to_char(a.PO_TIME, 'yyyy/mm/dd') as PO_TIME, sum(round(b.PO_PRICE*b.PO_QTY))  as AMOUNT,";
            //sql_oracle += "              (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospRecAddr') as REC_ADDR, ";
            //sql_oracle += "              (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospContact') as CONTACT ";
            //sql_oracle += "         from MM_PO_M a, MM_PO_D b, PH_VENDER c ";
            //sql_oracle += "        where a.PO_NO=b.PO_NO and a.AGEN_NO=c.AGEN_NO and a.PO_NO=:po_no and b.STATUS<>'D' ";
            //sql_oracle += "        group by a.PO_NO,a.PO_STATUS,a.M_CONTID,a.AGEN_NO,c.AGEN_NAMEC,c.AGEN_TEL,c.AGEN_FAX,c.EMAIL, a.MAT_CLASS, a.MEMO,a.SMEMO, a.UPDATE_USER,a.ISCOPY, a.PO_TIME ";
            string sql_oracle = @" 
                SELECT
                    a.po_no,
                    a.po_status,
                    a.m_contid,
                    a.agen_no,
                    c.agen_namec,
                    c.agen_tel,
                    c.agen_fax,
                    c.email,
                    a.mat_class,
                    a.memo,
                    a.smemo,
                    a.update_user,
                    a.iscopy,
                    to_char(a.po_time, 'yyyy/mm/dd')  AS po_time,
                    SUM(round(b.po_price * b.po_qty)) AS amount,
                    (
                        SELECT
                            data_value
                        FROM
                            param_d
                        WHERE
                                grp_code = 'HOSP_INFO'
                            AND data_name = 'HospRecAddr'
                    )                                 AS rec_addr,
                    (
                        SELECT
                            data_value
                        FROM
                            param_d
                        WHERE
                                grp_code = 'HOSP_INFO'
                            AND data_name = 'HospContact'
                    )                                 AS contact
                FROM
                    mm_po_m   a,
                    mm_po_d   b,
                    ph_vender c
                WHERE
                        a.po_no = b.po_no
                    AND a.agen_no = c.agen_no
                    AND a.po_no = :po_no
                    AND b.status <> 'D'
                GROUP BY
                    a.po_no,
                    a.po_status,
                    a.m_contid,
                    a.agen_no,
                    c.agen_namec,
                    c.agen_tel,
                    c.agen_fax,
                    c.email,
                    a.mat_class,
                    a.memo,
                    a.smemo,
                    a.update_user,
                    a.iscopy,
                    a.po_time
                ";
            List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            if (msg_oracle != "")
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(藥材)STEP2-取得信件基本資料失敗:" + msg_oracle, "AUTO");
            return dt_oralce;
        }

        //撈出信件基本資料(衛材)  floor(b.PO_PRICE*b.PO_QTY)
        private DataTable GetMailBasicData(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = " select a.PO_NO, a.M_CONTID, a.AGEN_NO, c.AGEN_NAMEC, c.EMAIL, a.MAT_CLASS, ";
            sql_oracle += "              a.MEMO, a.SMEMO, a.UPDATE_USER, a.ISCOPY, sum(floor(b.PO_PRICE*b.PO_QTY))  as AMOUNT ";
            sql_oracle += "         from MM_PO_M a, MM_PO_D b, PH_VENDER c ";
            sql_oracle += "        where a.PO_NO=b.PO_NO and a.AGEN_NO=c.AGEN_NO and a.PO_NO=:po_no and b.STATUS<>'D' ";
            sql_oracle += "        group by a.PO_NO,a.M_CONTID,a.AGEN_NO,c.AGEN_NAMEC,c.EMAIL, a.MAT_CLASS, a.MEMO,a.SMEMO, a.UPDATE_USER,a.ISCOPY ";
            List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            if (msg_oracle != "")
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(藥材)STEP2-取得信件基本資料失敗:" + msg_oracle, "AUTO");
            return dt_oralce;
        }

        //撈出信件訂單內容(衛材)
        private DataTable GetMailDt1_MatClassNotEqual_01(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = " select a.MMCODE as 院內碼,b.MMNAME_C as 中文品名, b.MMNAME_E as 英文品名, a.M_AGENLAB as 廠牌, a.M_PURUN as 單位, ";
            sql_oracle += "               PO_PRICE as 單價, PO_QTY as 數量, a.unit_swap as 採購計量單位轉換率, b.e_orderunit as 院內最小計量單位, ";
            sql_oracle += "               floor(a.PO_QTY * a.PO_PRICE) as 金額, ";
            sql_oracle += "               a.M_DISCPERC as 折讓百分比, a.CASENO as 合約案號, ";
            sql_oracle += @"              (select '三聯單品項:三聯單編號'||listagg(CRDOCNO,',') within group (order by mmcode) as 備註
                                             from CR_DOC
                                            where PO_NO=:po_no
                                              and mmcode=a.MMCODE
                                            group by po_no
                                          ) as 備註
                                   ";
            sql_oracle += "         from MM_PO_D a,MI_MAST b ";
            sql_oracle += "        where a.MMCODE=b.MMCODE and a.PO_NO=:po_no ";
            List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            if (msg_oracle != "")
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)STEP3-取得(衛材)信件訂單內容失敗:" + msg_oracle, "AUTO");
            return dt_oralce;
        }

        //撈出信件訂單內容(藥材)
        private DataTable GetMailDt1_MatClassEqual_01(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            //string sql_oracle = " select rownum 項次, b.MMCODE as 藥材代碼,b.MMNAME_C as 藥材名稱, a.M_PURUN as 出貨單位, ";
            //sql_oracle += "               PO_QTY as 數量,  PO_PRICE as 單價, b.E_ORDERUNIT as 單位, ";
            //sql_oracle += "               round(a.PO_QTY * a.PO_PRICE) as 小計,  ";
            //sql_oracle += "               (case when b.E_SOURCECODE = 'P' then '買斷' when b.E_SOURCECODE = 'C' then '寄庫' else '' end) as 買斷寄庫,  ";
            //// sql_oracle += "               (select SELF_CONT_EDATE from MED_SELFPUR_DEF where MMCODE = a.MMCODE and twn_date(sysdate) >= SELF_CONT_BDATE and twn_date(sysdate) <= SELF_CONT_EDATE and rownum = 1) as 合約到期日,  ";
            ////  sql_oracle += "               (select SELF_CONTRACT_NO from MED_SELFPUR_DEF where MMCODE = a.MMCODE and twn_date(sysdate) >= SELF_CONT_BDATE and twn_date(sysdate) <= SELF_CONT_EDATE and rownum = 1) as 合約案號, ";
            //sql_oracle += "               twn_date(a.E_CODATE) as 合約到期日, a.caseno as 合約案號, ";
            //sql_oracle += "               '' as 分批交貨日期, a.PO_NO as 進貨單號, ";
            //sql_oracle += "               a.MEMO as 備註 ";
            //sql_oracle += "         from MM_PO_D a,MI_MAST b ";
            //sql_oracle += "        where a.MMCODE=b.MMCODE and a.PO_NO=:po_no and a.STATUS<>'D'";
            //sql_oracle += "        order by b.MMNAME_E ";
            string sql_oracle = @" 
                SELECT ROWNUM AS 項次
                       , b.MMCODE AS 藥材代碼
                       , b.MMNAME_C AS 藥材名稱
                       , a.UNITRATE || ' ' || a.BASE_UNIT || '/' || a.M_PURUN AS 出貨單位
                       , PO_QTY AS 數量
                       , PO_PRICE AS 單價
                       , b.BASE_UNIT AS 單位
                       , round(a.PO_QTY * a.PO_PRICE) AS 小計
                       , ( CASE
                             WHEN b.E_SOURCECODE = 'P' THEN '買斷'
                             WHEN b.E_SOURCECODE = 'C' THEN '寄庫'
                             ELSE ''
                           END ) AS 買斷寄庫
                       , twn_date(a.E_CODATE) AS 合約到期日
                       , a.CASENO AS 合約案號
                       , '' AS 分批交貨日期
                       , a.PO_NO AS 進貨單號
                       --, ( CASE
                       --      WHEN length(a.CHINNAME) = 2 THEN substr(a.CHINNAME, 1, 1)
                       --                                       ||'○'
                       --      ELSE substr(a.CHINNAME, 1, 1)
                       --           || replace(LPAD(' ', length(a.CHINNAME) - 1, '○'), ' ', '')
                       --           ||substr(a.CHINNAME, length(a.CHINNAME), 1)
                       --    END ) AS 病人姓名
                       , a.CHINNAME AS 病人姓名
                       , a.CHARTNO AS 病歷號
                       , a.MEMO AS 備註
                FROM   MM_PO_D a
                       , MI_MAST b
                WHERE  a.MMCODE = b.MMCODE
                       AND a.PO_NO = :po_no
                       AND a.STATUS <> 'D'
                ORDER  BY b.MMNAME_E 
                ";
            List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            if (msg_oracle != "")
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(藥材)STEP3-取得(藥材)信件訂單內容失敗:" + msg_oracle, "AUTO");
            return dt_oralce;
        }

        //撈出信件單位非庫備品申請明細(衛材)
        private DataTable GetMailDt2_MatClassNotEqual_01(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = @" select * from (
                            select a.MMCODE as 院內碼, d.INID_NAME as 單位名稱,   
                                 sum(floor(b.APPQTY / a.UNIT_SWAP)) as 申請量
                            from MM_PO_D a, ME_DOCD b, ME_DOCM c, UR_INID d, MM_PR_D e
                            where a.PR_NO = b.RDOCNO and a.MMCODE = b.MMCODE and b.DOCNO = c.DOCNO
                              and c.TOWH = d.INID and a.STOREID = '0' and a.PO_NO = :po_no
                                  and a.pr_no = e.pr_no and a.mmcode = e.mmcode
                              and((e.PR_QTY - e.SRC_PR_QTY = 0) or trim(e.SRC_PR_QTY) is null)
                            group  by a.MMCODE,INID_NAME
                        union
                              select a.MMCODE as 院內碼, d.INID_NAME as 單位名稱,   
                                 sum(floor(b.APPQTY / a.UNIT_SWAP)) as 申請量
                            from MM_PO_D a, ME_DOCD b, ME_DOCM c, UR_INID d, MM_PR_D e
                            where a.PR_NO = b.RDOCNO and a.MMCODE = b.MMCODE and b.DOCNO = c.DOCNO
                              and c.TOWH = d.INID and a.STOREID = '0'
                              and a.PO_NO = :po_no
                              and a.pr_no = e.pr_no and a.mmcode = e.mmcode
                              and e.PR_QTY - e.SRC_PR_QTY > 0 and e.SRC_PR_QTY > 0
                            group  by a.MMCODE,INID_NAME
                        union
                              select a.MMCODE as 院內碼, wh_name(whno_mm1) as 單位名稱,   
                                 sum(floor((e.PR_QTY - e.SRC_PR_QTY) / a.UNIT_SWAP)) as 申請量
                            from MM_PO_D a, ME_DOCD b, ME_DOCM c, UR_INID d, MM_PR_D e
                            where a.PR_NO = b.RDOCNO and a.MMCODE = b.MMCODE and b.DOCNO = c.DOCNO
                              and c.TOWH = d.INID and a.STOREID = '0'
                              and a.PO_NO = :po_no
                              and a.pr_no = e.pr_no and a.mmcode = e.mmcode
                              and e.PR_QTY - e.SRC_PR_QTY > 0
                            group  by a.MMCODE,INID_NAME
                    )
                    order by 院內碼 ";
            List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            if (msg_oracle != "")
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)STEP4-取得(衛材)信件單位非庫備品申請明細失敗:" + msg_oracle, "AUTO");
            return dt_oralce;
        }

        //取得信件文字內容(先顯示後再加上訂單內容html)
        private DataTable GetMailContentByMatClass(string mat_class, string send_user)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = @" select data_value from PARAM_D where grp_code = 'MM_PO_M' and data_remark = :send_user and data_name = :mat_class ";
            List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":send_user", "VarChar", send_user));
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":mat_class", "VarChar", "MAIL_CONTENT_" + mat_class));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            if (msg_oracle != "")
                callDbtools_oralce.I_ERROR_LOG("BDS008", "取得信件文字內容失敗:" + msg_oracle, "AUTO");
            return dt_oralce;
        }

        //組出信件內容(衛材)
        private string GetMailContent_MatClassNotEqual_01(DataTable dtM, DataTable dtD1, DataTable dtD2)
        {
            string mail_body_str = ""; //寫到資料庫log的mail 內容(不含html)
            string mail_body = ""; //信件內容(含html)
            string headerStr = "";
            if (dtM.Rows[0]["MAT_CLASS"].ToString().Equals("02"))
                headerStr = GetHospName() + " 衛材訂購單(" + dtM.Rows[0]["PO_NO"].ToString().Trim() + ")";
            else
                headerStr = GetHospName() + " 一般物品訂購單(" + dtM.Rows[0]["PO_NO"].ToString().Trim() + ")";
            //if (dtM.Rows[0]["M_CONTID"].ToString().Trim() == "0")
            //    headerStr += "(合約)";
            //else
            //    headerStr += "(非合約)";

            mail_body += "<P align=center><font size=4 face=標楷體 color=black>" + headerStr + " </font></p><br><br>";
            mail_body_str += headerStr;

            mail_body += "廠商編號：" + dtM.Rows[0]["AGEN_NO"].ToString().Trim() + " <br>";
            mail_body_str += "廠商編號：" + dtM.Rows[0]["AGEN_NO"].ToString().Trim();
            mail_body += "廠商名稱：" + dtM.Rows[0]["AGEN_NAMEC"].ToString().Trim() + " <br>";
            mail_body_str += "廠商名稱：" + dtM.Rows[0]["AGEN_NAMEC"].ToString().Trim();
            mail_body += "交貨日期：詳如備註" + " <br><br>";
            mail_body_str += "交貨日期：詳如備註";

            //回覆函 URL + 參數加密 串接
            //CallDBtools calldbtools = new CallDBtools();
            //string urlStr = calldbtools.GetInternetWebServerIP();
            //urlStr += "/Form/Show/B/BH0006?";
            //string TsghUrl = calldbtools.GetInternetWebServerIP();
            //參數加密格式 po_no=INV010712270196&agen_no=826
            //string enCodeStr = "po_no=" + dtM.Rows[0]["PO_NO"].ToString().Trim() + "&agen_no=" + dtM.Rows[0]["AGEN_NO"].ToString().Trim();
            //urlStr += EnCode(enCodeStr);

            //mail_body += "<font size=3 face=新細明體 color=red>謝謝您的合作,請按一下回覆----></font><a href='" + urlStr + "'>回覆函</a>" + " <br>";
            //mail_body_str += "謝謝您的合作,請按一下回覆---->回覆函" + urlStr;
            mail_body += "<table style=\"border-collapse: collapse; width: 100%;border:2px #000000 solid;\" border=\"1\">";
            mail_body += "  <tbody>";
            mail_body += "    <tr>";
            mail_body += "      <td style=\"width: auto;\">院內碼</td>";
            mail_body += "      <td style=\"width: auto;\">中文品名</td>";
            mail_body += "      <td style=\"width: auto;\">英文品名</td>";
            mail_body += "      <td style=\"width: auto;\">廠牌</td>";
            mail_body += "      <td style=\"width: auto;\">單位</td>";
            mail_body += "      <td style=\"width: auto;\">單價</td>";
            mail_body += "      <td style=\"width: auto;\">數量</td>";
            mail_body += "      <td style=\"width: auto;\">院內最小計量單位</td>";
            mail_body += "      <td style=\"width: auto;\">採購計量單位轉換率</td>";
            mail_body += "      <td style=\"width: auto;\">金額</td>";
            mail_body += "      <td style=\"width: auto;\">折讓百分比</td>";
            mail_body += "      <td style=\"width: auto;\">合約案號</td>";
            mail_body += "      <td style=\"width: auto;\">備註</td>";
            mail_body += "    </tr>";
            mail_body_str += "院內碼中文品名英文品名廠牌單位單價數量院內最小計量單位採購計量單位轉換率金額折讓百分比備註";
            foreach (DataRow datarow in dtD1.Rows)
            {
                mail_body += "    <tr>";
                mail_body += "      <td>" + datarow["院內碼"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["中文品名"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["英文品名"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["廠牌"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["單位"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["單價"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["數量"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["院內最小計量單位"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["採購計量單位轉換率"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["金額"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["折讓百分比"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["合約案號"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["備註"].ToString().Trim() + "</td>";
                mail_body += "    </tr>";
                mail_body_str += datarow["院內碼"].ToString().Trim() + datarow["中文品名"].ToString().Trim() + datarow["英文品名"].ToString().Trim() + datarow["廠牌"].ToString().Trim() + datarow["單位"].ToString().Trim();
                mail_body_str += datarow["單價"].ToString().Trim() + datarow["數量"].ToString().Trim() + datarow["院內最小計量單位"].ToString().Trim() + datarow["採購計量單位轉換率"].ToString().Trim() + datarow["金額"].ToString().Trim() + datarow["折讓百分比"].ToString().Trim();
            }
            mail_body += "  </tbody>";
            mail_body += "</table><br><br>";

            mail_body += "<p align=left><font size=4 face=新細明體 color=#ff0000>總價:$" + dtM.Rows[0]["AMOUNT"].ToString().Trim() + " </font></p><br><br>";
            mail_body_str += "總價:$" + dtM.Rows[0]["AMOUNT"].ToString().Trim();

            //if (dtD2.Rows.Count > 0)
            //{
            //    mail_body += "<p align=left><font color=blue size=4 face=標楷體>**單位非庫備品申請明細** </font></p><br>";
            //    mail_body += "<table style=\"border-collapse: collapse; width: 30%;border:2px #000000 solid;\" border=\"1\">";
            //    mail_body += "  <tbody>";
            //    mail_body += "    <tr>";
            //    mail_body += "      <td style=\"width: auto;\">院內碼</td>";
            //    mail_body += "      <td style=\"width: auto;\">單位名稱</td>";
            //    mail_body += "      <td style=\"width: auto;\">申請量</td>";
            //    mail_body += "    </tr>";
            //    mail_body_str += "**單位非庫備品申請明細**院內碼單位名稱申請量";
            //    foreach (DataRow datarow in dtD2.Rows)
            //    {
            //        mail_body += "    <tr>";
            //        mail_body += "      <td>" + datarow["院內碼"].ToString().Trim() + "</td>";
            //        mail_body += "      <td>" + datarow["單位名稱"].ToString().Trim() + "</td>";
            //        mail_body += "      <td>" + datarow["申請量"].ToString().Trim() + "</td>";
            //        mail_body += "    </tr>";
            //        mail_body_str += datarow["院內碼"].ToString().Trim() + datarow["單位名稱"].ToString().Trim() + datarow["申請量"].ToString().Trim();
            //    }
            //    mail_body += "  </tbody>";
            //    mail_body += "</table><br><br>";
            //}

            mail_body += GetMailContentRemarks(dtM.Rows[0]["MEMO"].ToString().Trim(), dtM.Rows[0]["SMEMO"].ToString().Trim());
            //mail_body += "<font color=red>新系統上線自即日起請務必於訂單網頁輸入資料----></font><a href='" + TsghUrl + "'</a>三總網站<br>";
            mail_body_str += dtM.Rows[0]["MEMO"].ToString().Trim() + dtM.Rows[0]["SMEMO"].ToString().Trim();
            mail_body += "<br><P align=left><font size=2 face=標楷體 color=black>請注意：當有指定[分批交貨日期]時，請準時於指定日期交貨</font></p><br>";
            mail_body_str += "請注意：當有指定[分批交貨日期]時，請準時於指定日期交貨";

            return mail_body + "BDS008信件內容分割" + mail_body_str;
        }

        //組出信件內容(藥材)
        private string GetMailContent_MatClassEqual_01(DataTable dtM, DataTable dtD1, DataTable dtMC)
        {
            string mail_body_str = ""; //寫到資料庫log的mail 內容(不含html)
            string mail_body = ""; //信件內容(含html)

            if (dtMC.Rows.Count > 0)
            {
                mail_body += dtMC.Rows[0]["data_value"] == DBNull.Value ? "" : dtMC.Rows[0]["data_value"].ToString() + "<br><br>";
                mail_body_str += dtMC.Rows[0]["data_value"] == DBNull.Value ? "" : dtMC.Rows[0]["data_value"].ToString();
            }

            mail_body += "<P align=center><font size=6 face=標楷體 color=black>" + GetHospName() + " 訂購單(編號：" + dtM.Rows[0]["PO_NO"].ToString().Trim() + ")</font></p><br><br>";
            mail_body_str += GetHospName() + " 訂購單(編號：" + dtM.Rows[0]["PO_NO"].ToString().Trim() + ")";

            string[] po_time = dtM.Rows[0]["PO_TIME"].ToString().Trim().Split('/');
            string po_time_str = (Convert.ToInt32(po_time[0]) - 1911) + "年" + po_time[1] + "月" + po_time[2] + "日";
            mail_body += "<table style=\"border-collapse: collapse; width: 100%;border:0px #000000 solid;\" border=\"0\">";
            mail_body += "  <tbody>";
            mail_body += "    <tr>";
            mail_body += "      <td style=\"width: 50%;\">訂購日期：" + po_time_str + "</td>";
            mail_body += "      <td style=\"width: 50%;\">廠商：" + dtM.Rows[0]["AGEN_NO"].ToString().Trim() + "&nbsp;&nbsp;&nbsp;" + dtM.Rows[0]["AGEN_NAMEC"].ToString().Trim() + "</td>";
            mail_body += "    </tr>";
            mail_body += "    <tr>";
            mail_body += "      <td style=\"width: 50%;\">訂購方式：電子郵件</td>";
            mail_body += "      <td style=\"width: 50%;\">電話：" + dtM.Rows[0]["AGEN_TEL"].ToString().Trim() + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;傳真：" + dtM.Rows[0]["AGEN_FAX"].ToString().Trim() + "</td>";
            mail_body += "    </tr>";
            mail_body += "  </tbody>";
            mail_body += "</table>";

            //備註內容
            string memo = dtM.Rows[0]["MEMO"].ToString().Trim();
            string smemo = dtM.Rows[0]["SMEMO"].ToString().Trim();
            memo = memo.Replace("\r", "<br>");
            memo = memo.Replace("\n", "<br>");
            memo = memo.Replace("<br><br>", "<br>");
            smemo = smemo.Replace("\r", "<br>");
            smemo = smemo.Replace("\n", "<br>");
            smemo = smemo.Replace("<br><br>", "<br>");
            //針對MEMO 檢查是否有紅色顯示需求
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
            var redstr = "";

            string sql_oracle = @" select reddisp from ( select distinct d.msgno, d.m_contid,  m.msgno as num, 
                            (select reddisp from PH_MAILSP_M a where  a.msgno=m.msgno and a.msgrecno=(
                              case when (select max(msgrecno) from PH_MAILSP_D  where agen_no=:agen_no and m_contid=:m_contid and msgno=a.msgno) is null then
                              (select max(msgrecno) from PH_MAILSP_D  where agen_no='*' and m_contid=:m_contid and msgno=a.msgno) 
                              else (select max(msgrecno) from PH_MAILSP_D  where agen_no=:agen_no and m_contid=:m_contid and msgno=a.msgno) 
                              end) ) as reddisp
                            from  PH_MAILSP_M m, PH_MAILSP_D d
                            where m.msgno=d.msgno and m.msgrecno=d.msgrecno and d.m_contid=:m_contid  and m.reddisp is not null )
                            order by num ";
            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dtM.Rows[0]["AGEN_NO"].ToString().Trim()));
            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":m_contid", "VarChar", dtM.Rows[0]["M_CONTID"].ToString().Trim()));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
            {
                for (int i = 0; i < dt_oralce.Rows.Count; i++)
                {
                    string[] dispstr = dt_oralce.Rows[i]["REDDISP"].ToString().Split(';');
                    for (int j = 0; j < dispstr.Length; j++)
                    {   //刪除換行 \n
                        if (redstr.IndexOf(dispstr[j]) < 0)
                        {        //判斷是否出現過

                            // 2020-09-22: 空字串無法取代，先判斷是否為空字串，不是的話再執行replace
                            string temp_dispstr = dispstr[j].Replace("\n", "");
                            if (temp_dispstr != "")
                            {
                                memo = memo.Replace(dispstr[j].Replace("\n", ""), "<font color=red><strong>" + dispstr[j] + "</strong> </font>");
                            }
                        }

                        redstr += dispstr[j].Replace("\n", "") + ";";
                    }
                }
            }

            mail_body += "備註：" + " <br>";
            mail_body += "<font color=red>" + smemo + " </font><br><b><u>" + memo + "</u></b>";
            // mail_body_str += dtM.Rows[0]["MEMO"].ToString().Trim() + dtM.Rows[0]["SMEMO"].ToString().Trim();

            mail_body += "<br>送貨地點：" + GetHospName() + " " + dtM.Rows[0]["REC_ADDR"].ToString().Trim() + "<br>";
            mail_body_str += "送貨地點：" + GetHospName() + dtM.Rows[0]["REC_ADDR"].ToString().Trim();
            mail_body += "聯絡電話：" + dtM.Rows[0]["CONTACT"].ToString().Trim() + "<br><br>";
            mail_body_str += "聯絡電話：" + dtM.Rows[0]["CONTACT"].ToString().Trim();
            string dateToday = (DateTime.Now.Year - 1911) + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日";
            mail_body += "列印日期：" + dateToday + "<br><br>";
            mail_body_str += "列印日期：" + dateToday;

            mail_body += "<table style=\"border-collapse: collapse; width: 100%;border:2px #000000 solid;\" border=\"1\">";
            mail_body += "  <tbody>";
            mail_body += "    <tr>";
            mail_body += "      <td style=\"width: auto;\">項次</td>";
            mail_body += "      <td style=\"width: auto;\">藥材代碼</td>";
            mail_body += "      <td style=\"width: auto;\">藥材名稱</td>";
            mail_body += "      <td style=\"width: auto;\">出貨單位</td>";
            mail_body += "      <td style=\"width: auto;\">數量</td>";
            mail_body += "      <td style=\"width: auto;\">單位</td>";
            mail_body += "      <td style=\"width: auto;\">單價</td>";
            mail_body += "      <td style=\"width: auto;\">小計</td>";
            mail_body += "      <td style=\"width: auto;\">買斷寄庫</td>";
            mail_body += "      <td style=\"width: auto;\">合約到期日</td>";
            mail_body += "      <td style=\"width: auto;\">合約案號</td>";
            mail_body += "      <td style=\"width: auto;\">分批交貨日期</td>";
            mail_body += "      <td style=\"width: auto;\">進貨單號</td>";
            mail_body += "      <td style=\"width: auto;\">病人姓名</td>";
            mail_body += "      <td style=\"width: auto;\">病歷號</td>";
            mail_body += "      <td style=\"width: auto;\">備註</td>";
            mail_body += "    </tr>";
            mail_body_str += "項次品名規格廠牌單位單價數量金額備註";
            int itemnum = 1;
            foreach (DataRow datarow in dtD1.Rows)
            {
                mail_body += "    <tr>";
                mail_body += "      <td>" + itemnum.ToString() + "</td>";
                mail_body += "      <td>" + datarow["藥材代碼"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["藥材名稱"].ToString().Trim() + "</td>";
                mail_body += "      <td align=center>" + datarow["出貨單位"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["數量"].ToString().Trim() + "</td>";
                mail_body += "      <td align=center>" + datarow["單位"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["單價"].ToString().Trim() + "</td>";
                mail_body += "      <td align=right>" + datarow["小計"].ToString().Trim() + "</td>";
                mail_body += "      <td align=center>" + datarow["買斷寄庫"].ToString().Trim() + "</td>";
                mail_body += "      <td align=center>" + datarow["合約到期日"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["合約案號"].ToString().Trim() + "</td>";
                mail_body += "      <td align=center>" + datarow["分批交貨日期"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["進貨單號"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["病人姓名"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["病歷號"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["備註"].ToString().Trim() + "</td>";
                mail_body += "    </tr>";
                mail_body_str += itemnum.ToString() + datarow["藥材代碼"].ToString().Trim() + datarow["藥材名稱"].ToString().Trim() + datarow["出貨單位"].ToString().Trim();
                mail_body_str += datarow["數量"].ToString().Trim() + datarow["單價"].ToString().Trim() + datarow["小計"].ToString().Trim() + datarow["買斷寄庫"].ToString().Trim();
                mail_body_str += datarow["合約到期日"].ToString().Trim() + datarow["合約到期日"].ToString().Trim() + datarow["合約案號"].ToString().Trim() + datarow["分批交貨日期"].ToString().Trim();
                mail_body_str += datarow["進貨單號"].ToString().Trim() + datarow["病人姓名"].ToString().Trim() + datarow["病歷號"].ToString().Trim() + datarow["備註"].ToString().Trim();
                itemnum++;
            }
            mail_body += "    <tr>";
            mail_body += "      <td>總價：</td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td align=right>" + dtM.Rows[0]["AMOUNT"].ToString().Trim() + "</td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "      <td></td>";
            mail_body += "    </tr>";
            mail_body += "  </tbody>";
            mail_body += "</table>";
            mail_body_str += "總價：" + dtM.Rows[0]["AMOUNT"].ToString().Trim();

            mail_body += "<br><P align=left><b><font size=4 face=標楷體 color=black>請注意：當有指定[分批交貨日期]時，請準時於指定日期交貨</font></b></p><br>";
            mail_body_str += "請注意：當有指定[分批交貨日期]時，請準時於指定日期交貨";

            //回覆函 URL + 參數加密 串接
            //CallDBtools calldbtools = new CallDBtools();
            //string urlStr = calldbtools.GetInternetWebServerIP();
            //urlStr += "/Form/Show/B/BH0006?";
            //string TsghUrl = calldbtools.GetInternetWebServerIP();
            //參數加密格式 po_no=INV010712270196&agen_no=826
            //string enCodeStr = "po_no=" + dtM.Rows[0]["PO_NO"].ToString().Trim() + "&agen_no=" + dtM.Rows[0]["AGEN_NO"].ToString().Trim();
            //urlStr += EnCode(enCodeStr);

            //mail_body += "謝謝您的合作,請按一下回覆----><a href='" + urlStr + "'>訂單回覆</a><br>";
            //mail_body += "<font color=red>新系統上線自即日起請務必於訂單網頁輸入資料----></font><a href='" + TsghUrl + "'</a>三總網站<br>";
            //mail_body_str += "謝謝您的合作,請按一下回覆---->訂單回覆" + urlStr;

            return mail_body + "BDS008信件內容分割" + mail_body_str;
        }

        //組出MSGMAIL內容
        private string GetMailContent_MSGMAIL(DataTable dtM)
        {
            //CallDBtools calldbtools = new CallDBtools();
            //string TsghUrl = calldbtools.GetInternetWebServerIP();
            string mail_body = ""; //信件內容(含html)
            mail_body += "*** " + GetHospName() + "庫房通知 ***<br>";
            mail_body += "主旨：" + dtM.Rows[0]["THEME"].ToString().Trim() + " <br>";
            mail_body += "通知內容：<br>";
            mail_body += "        " + dtM.Rows[0]["MSG"].ToString().Trim() + " <br>";
            mail_body += "寄信人：" + dtM.Rows[0]["USERNAME"].ToString().Trim() + " <br>";
            //mail_body += "<font color='Blue'>相關內容附件請連結三總相關網站查詢 </font><br>";
            //mail_body += "連結三總相關網站----><a href='" + TsghUrl + "'</a>三總網站<br>";
            return mail_body;
        }

        //參數值加密 po_no=INV010712270196&agen_no=826
        private string EnCode(string EnString)
        {
            byte[] data = Encoding.UTF8.GetBytes(EnString);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Padding = PaddingMode.PKCS7;
            DES.Key = ASCIIEncoding.ASCII.GetBytes("BH06BHS4");
            DES.IV = ASCIIEncoding.ASCII.GetBytes("TsghTsgh");
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
            return BitConverter.ToString(result);
        }

        //信件備註內容(衛材)
        private string GetMailContentRemarks(string memo, string smemo)
        {
            string mail_body = "<p align=left><font color=brown size=4 face=標楷體>備註內容 </font></p><br>";
            mail_body += "<p align=left><font color=red size=3 face=標楷體>***本郵件為系統自動發送，請勿直接回信*** </font></p><br>";
            memo = memo.Replace("\r", "<br><br>");
            memo = memo.Replace("\n", "<br><br>");
            smemo = smemo.Replace("\r", "<br><br>");
            smemo = smemo.Replace("\n", "<br><br>");
            mail_body += "<font size=3 face=標楷體 color=black>" + memo + " </font><br><br>";
            mail_body += "<font size=4 face=標楷體 color=red>" + smemo + " </font><br>";
            return mail_body;
        }

        //將 MM_PO_M,MM_PO_D 複製到MS資料庫 WB_MM_PO_M,WB_MM_PO_D
        private void SelectOracleMM_POintoMsWB_MM_PO(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = " select PO_NO,AGEN_NO,PO_TIME,M_CONTID,PO_STATUS, ";
            sql_oracle += " CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP, ";
            sql_oracle += " ISCONFIRM,ISBACK,PHONE,ISCOPY,isCR ";
            sql_oracle += " from MM_PO_M ";
            sql_oracle += " where PO_NO = :po_no ";
            List<CallDBtools_Oracle.OracleParam> paraList1 = new List<CallDBtools_Oracle.OracleParam>();
            paraList1.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList1, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "" && dt_oralce.Rows.Count > 0)
            {
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                string msgDB = "error", sql_msdb = "";
                int rowsAffected = -1;
                sql_msdb = "insert into WB_MM_PO_M(PO_NO,AGEN_NO,PO_TIME,M_CONTID,PO_STATUS, ";
                sql_msdb += "                      CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP, ";
                sql_msdb += "                      ISCONFIRM,ISBACK,PHONE,ISCOPY,isCR) ";
                sql_msdb += "               values(@po_no,@agen_no,@po_time,@m_contid,@po_status, ";
                sql_msdb += "                      @create_time,@create_user,@update_time,@update_user,@update_ip, ";
                sql_msdb += "                      @isconfirm,@isback,@phone,@iscopy, @isCR) ";
                List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_no", "VarChar", dt_oralce.Rows[0]["PO_NO"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@agen_no", "VarChar", dt_oralce.Rows[0]["AGEN_NO"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_time", "VarChar", dt_oralce.Rows[0]["PO_TIME"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_contid", "VarChar", dt_oralce.Rows[0]["M_CONTID"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_status", "VarChar", dt_oralce.Rows[0]["PO_STATUS"].ToString().Trim()));

                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_time", "VarChar", dt_oralce.Rows[0]["CREATE_TIME"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_user", "VarChar", dt_oralce.Rows[0]["CREATE_USER"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_time", "VarChar", dt_oralce.Rows[0]["UPDATE_TIME"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_user", "VarChar", dt_oralce.Rows[0]["UPDATE_USER"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_ip", "VarChar", dt_oralce.Rows[0]["UPDATE_IP"].ToString().Trim()));

                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@isconfirm", "VarChar", dt_oralce.Rows[0]["ISCONFIRM"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@isback", "VarChar", dt_oralce.Rows[0]["ISBACK"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@phone", "VarChar", dt_oralce.Rows[0]["PHONE"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@iscopy", "VarChar", dt_oralce.Rows[0]["ISCOPY"].ToString().Trim()));
                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@isCR", "VarChar", dt_oralce.Rows[0]["isCR"].ToString().Trim()));

                rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
                if (msgDB != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)(藥材)STEP7-新增WB_MM_PO_M失敗:" + msgDB, "AUTO");
                }
            }
            else if (msg_oracle != "")
            {
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)(藥材)STEP6-取得MM_PO_M失敗:" + msg_oracle, "AUTO");
            }

            sql_oracle = " select a.PO_NO,a.MMCODE,a.PO_QTY,a.PO_PRICE, ";
            sql_oracle += "       a.M_PURUN,a.M_AGENLAB,a.PO_AMT,a.M_DISCPERC,a.DELI_QTY, ";
            sql_oracle += "       a.BW_SQTY,a.DELI_STATUS,a.CREATE_TIME,a.CREATE_USER,a.UPDATE_TIME, ";
            sql_oracle += "       a.UPDATE_USER,a.UPDATE_IP,a.MEMO,a.PR_NO,a.UNIT_SWAP, ";
            sql_oracle += "       b.MMNAME_C,b.MMNAME_E,b.WEXP_ID,a.DISC_CPRICE, ";
            sql_oracle += "       a.ISWILLING, a.DISCOUNT_QTY, a.DISC_COST_UPRICE";
            sql_oracle += "  from MM_PO_D a, MI_MAST b ";
            sql_oracle += " where a.MMCODE=b.MMCODE and  a.PO_NO = :po_no and a.STATUS<>'D'";
            List<CallDBtools_Oracle.OracleParam> paraList2 = new List<CallDBtools_Oracle.OracleParam>();
            paraList2.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList2, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "" && dt_oralce.Rows.Count > 0)
            {
                for (int i = 0; i < dt_oralce.Rows.Count; i++)
                {
                    CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                    string msgDB = "error", sql_msdb = "";
                    int rowsAffected = -1;
                    sql_msdb = "insert into WB_MM_PO_D(PO_NO,MMCODE,PO_QTY,PO_PRICE, ";
                    sql_msdb += "                      M_PURUN,M_AGENLAB,PO_AMT,M_DISCPERC,DELI_QTY, ";
                    sql_msdb += "                      BW_SQTY,DELI_STATUS,CREATE_TIME,CREATE_USER,UPDATE_TIME, ";
                    sql_msdb += "                      UPDATE_USER,UPDATE_IP,MEMO,PR_NO,UNIT_SWAP, ";
                    sql_msdb += "                      MMNAME_C,MMNAME_E,WEXP_ID, DISC_CPRICE," +
                        "                              ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE) ";
                    sql_msdb += "               values(@po_no,@mmcode,@po_qty,@po_price, ";
                    sql_msdb += "                      @m_purun,@m_agenlab,@po_amt,@m_discperc,@deli_qty, ";
                    sql_msdb += "                      @bw_sqty,@deli_status,@create_time,@create_user,@update_time, ";
                    sql_msdb += "                      @update_user,@update_ip,@memo,@pr_no,@unit_swap, ";
                    sql_msdb += "                      @mmname_c,@mmname_e,@wexp_id, @disc_cprice, ";
                    sql_msdb += "                      @iswilling,@discount_qty,@disc_cost_uprice ) ";
                    List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_no", "VarChar", dt_oralce.Rows[i]["PO_NO"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                    if (dt_oralce.Rows[i]["PO_QTY"].ToString().Trim() != "")
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_qty", "VarChar", dt_oralce.Rows[i]["PO_QTY"].ToString().Trim()));
                    else
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_qty", "VarChar", DBNull.Value));
                    if (dt_oralce.Rows[i]["PO_PRICE"].ToString().Trim() != "")
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_price", "VarChar", dt_oralce.Rows[i]["PO_PRICE"].ToString().Trim()));
                    else
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_price", "VarChar", DBNull.Value));

                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_purun", "VarChar", dt_oralce.Rows[i]["M_PURUN"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_agenlab", "VarChar", dt_oralce.Rows[i]["M_AGENLAB"].ToString().Trim()));
                    if (dt_oralce.Rows[i]["PO_AMT"].ToString().Trim() != "")
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_amt", "VarChar", dt_oralce.Rows[i]["PO_AMT"].ToString().Trim()));
                    else
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_amt", "VarChar", DBNull.Value));
                    if (dt_oralce.Rows[i]["M_DISCPERC"].ToString().Trim() != "")
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_discperc", "VarChar", dt_oralce.Rows[i]["M_DISCPERC"].ToString().Trim()));
                    else
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_discperc", "VarChar", DBNull.Value));
                    if (dt_oralce.Rows[i]["DELI_QTY"].ToString().Trim() != "")
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@deli_qty", "VarChar", dt_oralce.Rows[i]["DELI_QTY"].ToString().Trim()));
                    else
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@deli_qty", "VarChar", DBNull.Value));

                    if (dt_oralce.Rows[i]["BW_SQTY"].ToString().Trim() != "")
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@bw_sqty", "VarChar", dt_oralce.Rows[i]["BW_SQTY"].ToString().Trim()));
                    else
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@bw_sqty", "VarChar", DBNull.Value));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@deli_status", "VarChar", dt_oralce.Rows[i]["DELI_STATUS"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_time", "VarChar", dt_oralce.Rows[i]["CREATE_TIME"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_user", "VarChar", dt_oralce.Rows[i]["CREATE_USER"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_time", "VarChar", dt_oralce.Rows[i]["UPDATE_TIME"].ToString().Trim()));

                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_user", "VarChar", dt_oralce.Rows[i]["UPDATE_USER"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_ip", "VarChar", dt_oralce.Rows[i]["UPDATE_IP"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@pr_no", "VarChar", dt_oralce.Rows[i]["PR_NO"].ToString().Trim()));
                    if (dt_oralce.Rows[i]["UNIT_SWAP"].ToString().Trim() != "")
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@unit_swap", "VarChar", dt_oralce.Rows[i]["UNIT_SWAP"].ToString().Trim()));
                    else
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@unit_swap", "VarChar", DBNull.Value));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmname_c", "VarChar", dt_oralce.Rows[i]["MMNAME_C"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmname_e", "VarChar", dt_oralce.Rows[i]["MMNAME_E"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@wexp_id", "VarChar", dt_oralce.Rows[i]["WEXP_ID"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@disc_cprice", "VarChar", dt_oralce.Rows[i]["DISC_CPRICE"].ToString().Trim()));
                    if (dt_oralce.Rows[i]["ISWILLING"].ToString().Trim() != "")
                    {
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@iswilling", "VarChar", dt_oralce.Rows[i]["ISWILLING"].ToString().Trim()));
                    }
                    else
                    {
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@iswilling", "VarChar", DBNull.Value));
                    }
                    if (dt_oralce.Rows[i]["DISCOUNT_QTY"].ToString().Trim() != "")
                    {
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@discount_qty", "VarChar", dt_oralce.Rows[i]["DISCOUNT_QTY"].ToString().Trim()));
                    }
                    else
                    {
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@discount_qty", "VarChar", DBNull.Value));
                    }
                    if (dt_oralce.Rows[i]["DISC_COST_UPRICE"].ToString().Trim() != "")
                    {
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@disc_cost_uprice", "VarChar", dt_oralce.Rows[i]["DISC_COST_UPRICE"].ToString().Trim()));
                    }
                    else
                    {
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@disc_cost_uprice", "VarChar", DBNull.Value));
                    }

                    rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
                    if (msgDB != "")
                    {
                        callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)(藥材)STEP9-新增WB_MM_PO_D失敗:" + msgDB, "AUTO");
                    }
                }
            }
            else if (msg_oracle != "")
            {
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)(藥材)STEP8-取得MM_PO_D,MI_MAST失敗:" + msg_oracle, "AUTO");
            }
        }

        //更新 WB_MM_PO_D[PO_QTY]
        private void UpdateWB_MM_PO_D(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = " select a.PO_NO, a.MMCODE, a.PO_QTY, a.UPDATE_TIME, a.UPDATE_USER, a.UPDATE_IP from MM_PO_D a, MM_PO_INREC b";
            sql_oracle += " where a.PO_NO=b.PO_NO and a.MMCODE=b.MMCODE and a.PO_NO = :po_no and b.MEMO like '%修改數量%' ";
            List<CallDBtools_Oracle.OracleParam> paraList1 = new List<CallDBtools_Oracle.OracleParam>();
            paraList1.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList1, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "" && dt_oralce.Rows.Count > 0)
            {
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                string msgDB = "error", sql_msdb = "";
                int rowsAffected = -1;
                for (int i = 0; i < dt_oralce.Rows.Count; i++)
                {
                    sql_msdb = " update WB_MM_PO_D  ";
                    sql_msdb += "  set PO_QTY=@po_qty, update_time=@update_time, ";
                    sql_msdb += "      update_user=@update_user, update_ip=@update_ip";
                    sql_msdb += " where PO_NO=@po_no and MMCODE=@mmcode ";
                    List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_no", "VarChar", dt_oralce.Rows[i]["PO_NO"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_qty", "VarChar", dt_oralce.Rows[i]["PO_QTY"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_time", "VarChar", dt_oralce.Rows[i]["UPDATE_TIME"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_user", "VarChar", dt_oralce.Rows[i]["UPDATE_USER"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_ip", "VarChar", dt_oralce.Rows[i]["UPDATE_IP"].ToString().Trim()));

                    rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
                    if (msgDB != "")
                    {
                        callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)(藥材)STEP7-更新WB_MM_PO_D失敗:" + msgDB, "AUTO");
                    }
                }
            }
            else if (msg_oracle != "")
            {
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)(藥材)STEP6-取得MM_PO_D失敗:" + msg_oracle, "AUTO");
            }
        }

        //將 oracle 非庫備品申請單位資料 寫入 oracle PH_PO_N
        // 2022-03-04: 是否非庫備判斷MM_PO_D，取代MI_MAST
        private void InsertPH_PO_N(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = " select distinct a.PO_NO, a.MMCODE, INID, floor(b.APPQTY/a.UNIT_SWAP) PO_QTY, b.DOCNO, b.APPQTY ";
            sql_oracle += "         from MM_PO_D a, ME_DOCD b, ME_DOCM c, UR_INID d ";
            sql_oracle += "        where a.PR_NO=b.RDOCNO and a.MMCODE=b.MMCODE ";
            sql_oracle += "          and b.DOCNO=c.DOCNO and c.TOWH=d.INID and a.STOREID='0' and a.PO_NO= :po_no ";
            List<CallDBtools_Oracle.OracleParam> paraList1 = new List<CallDBtools_Oracle.OracleParam>();
            paraList1.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList1, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
            {
                for (int i = 0; i < dt_oralce.Rows.Count; i++)
                {
                    int rowsAffected_oracle = -1;
                    sql_oracle = " insert into PH_PO_N(PO_NO, MMCODE, INID, PO_QTY, DOCNO, CREATE_TIME, APPQTY) ";
                    sql_oracle += "             values(:po_no, :mmcode, :inid, :po_qty, :docno, sysdate, :appqty) ";
                    List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                    paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", dt_oralce.Rows[i]["PO_NO"].ToString().Trim()));
                    paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                    paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":inid", "VarChar", dt_oralce.Rows[i]["INID"].ToString().Trim()));
                    paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":po_qty", "VarChar", dt_oralce.Rows[i]["PO_QTY"].ToString().Trim()));
                    paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":docno", "VarChar", dt_oralce.Rows[i]["DOCNO"].ToString().Trim()));
                    paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":appqty", "VarChar", dt_oralce.Rows[i]["APPQTY"].ToString().Trim()));
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                    if (msg_oracle != "")
                    {
                        callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)STEP11-新增PH_PO_N失敗:" + msg_oracle, "AUTO");
                    }
                }
            }
            else if (msg_oracle != "")
            {
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)STEP10-取得非庫備品申請單位資料失敗:" + msg_oracle, "AUTO");
            }
        }

        //若 isCR='Y'，則 增加insert 外網的WB_REPLY
        private void InsertWB_REPLY(string po_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = @" select a.PO_NO, (select AGEN_NO from MM_PO_M where PO_NO=a.PO_NO) as AGEN_NO,
                                          a.MMCODE, a.LOT_NO, a.EXP_DATE, a.INQTY,
                                          ('三聯單編號'||a.CRDOCNO) as MEMO, a.CRDOCNO
                                     from (select x.PO_NO, x.MMCODE, x.CRDOCNO, y.LOT_NO, y.EXP_DATE, y.INQTY
                                             from CR_DOC x,CR_DOC_D y
                                            where x.CRDOCNO=y.CRDOCNO
                                              and x.PO_NO=:po_no
                                          ) a
                                    order by a.PO_NO, a.MMCODE, a.CRDOCNO
                                   ";
            List<CallDBtools_Oracle.OracleParam> paraList1 = new List<CallDBtools_Oracle.OracleParam>();
            paraList1.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList1, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
            {
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                string msgDB = "error", sql_msdb = "";
                int rowsAffected = -1;
                string o_po_no = string.Empty;
                string o_mmcode = string.Empty;
                string o_crdocno = string.Empty;
                int v_maxdno = 0;
                for (int i = 0; i < dt_oralce.Rows.Count; i++)
                {
                    if (o_po_no != dt_oralce.Rows[i]["PO_NO"].ToString().Trim() || o_mmcode != dt_oralce.Rows[i]["MMCODE"].ToString().Trim())
                    {
                        v_maxdno = 1;
                        o_po_no = dt_oralce.Rows[i]["PO_NO"].ToString().Trim();
                        o_mmcode = dt_oralce.Rows[i]["MMCODE"].ToString().Trim();
                        o_crdocno = dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim();
                    }
                    else
                    {
                        if (o_crdocno != dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim())
                        {
                            v_maxdno = v_maxdno + 1;
                            o_crdocno = dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim();
                        }
                    }

                    sql_msdb = @" insert into WB_REPLY
                                    (PO_NO, AGEN_NO, MMCODE, DNO, DELI_DT, LOT_NO, EXP_DATE, INQTY, BW_SQTY,
                                     MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP,
                                     STATUS, FLAG)
                                  values
                                    (@PO_NO, @AGEN_NO, @MMCODE, @MAXDNO, getdate(), @LOT_NO, @EXP_DATE, @INQTY, 0,
                                     @MEMO, getdate(), '緊急醫療出貨', getdate(), '緊急醫療出貨','', 'A', 'A');
  ";
                    List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@PO_NO", "VarChar", dt_oralce.Rows[i]["PO_NO"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@AGEN_NO", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@MMCODE", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@MAXDNO", "VarChar", v_maxdno.ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@LOT_NO", "VarChar", dt_oralce.Rows[i]["LOT_NO"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@EXP_DATE", "VarChar", dt_oralce.Rows[i]["EXP_DATE"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@INQTY", "VarChar", dt_oralce.Rows[i]["INQTY"].ToString().Trim()));
                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@MEMO", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));


                    rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
                    if (msgDB != "")
                    {
                        callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)STEP13-新增WB_REPLY失敗:" + msg_oracle, "AUTO");
                    }
                }
            }
            else if (msg_oracle != "")
            {
                callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(衛材FAX)STEP12-取得三聯單點收資料失敗:" + msg_oracle, "AUTO");
            }
        }

        // 2021-12-30 附件資料寫入
        private Stream CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            return output;
        }

        // 取得醫院名稱
        private string GetHospName()
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            string str_oracle = "";
            string sql_oracle = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospName' ";
            str_oracle = callDbtools_oralce.CallExecScalar(sql_oracle, null, "oracle", ref msg_oracle);
            if (msg_oracle != "")
            {
                callDbtools_oralce.I_ERROR_LOG("BDS008", "取得醫院名稱資料失敗:" + msg_oracle, "AUTO");
            }
            return str_oracle;
        }

        // 建立信件附檔
        public void CreateEmailAttFile(string fileName, DataTable dtM, DataTable dtD)
        {
            if (dtM == null || dtD == null) return;

            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Sheet1");
            sheet.SetColumnWidth(0, 10 * 256); // 項次
            sheet.SetColumnWidth(1, 13 * 256); // 藥材代碼
            sheet.SetColumnWidth(2, 30 * 256); // 藥材名稱
            sheet.SetColumnWidth(3, 13 * 256); // 出貨單位
            sheet.SetColumnWidth(4, 10 * 256); // 數量
            sheet.SetColumnWidth(5, 10 * 256); // 單價
            sheet.SetColumnWidth(6, 10 * 256); // 單位
            sheet.SetColumnWidth(7, 10 * 256); // 小計
            sheet.SetColumnWidth(8, 13 * 256); // 買斷寄庫
            sheet.SetColumnWidth(9, 15 * 256); // 合約到期日
            sheet.SetColumnWidth(10, 13 * 256); // 合約案號
            sheet.SetColumnWidth(11, 19 * 256); // 分批交貨日期
            sheet.SetColumnWidth(12, 20 * 256); // 進貨單號
            sheet.SetColumnWidth(13, 10 * 256); // 備註

            // 文件標題
            ICellStyle titleStyle = workbook.CreateCellStyle();
            IFont titlefont = workbook.CreateFont();
            titlefont.IsBold = true;
            titlefont.FontHeightInPoints = 22;
            titleStyle.SetFont(titlefont);

            // 表格標題
            ICellStyle tableStyle = workbook.CreateCellStyle();
            IFont tablefont = workbook.CreateFont();
            tablefont.IsBold = true;
            tablefont.FontHeightInPoints = 14;
            tableStyle.SetFont(tablefont);

            // 內容
            ICellStyle contentStyle = workbook.CreateCellStyle();
            IFont contentfont = workbook.CreateFont();
            contentfont.FontHeightInPoints = 12;
            contentStyle.SetFont(contentfont);

            // row 0
            IRow exr = sheet.CreateRow(0);
            ICell exc = exr.CreateCell(2);
            exc.SetCellValue(GetHospName() + " 訂購單(編號：" + dtM.Rows[0]["PO_NO"].ToString().Trim() + ")");
            exc.CellStyle = titleStyle;

            // row 2
            IRow exr2 = sheet.CreateRow(2);
            ICell exc2_0 = exr2.CreateCell(0);
            string[] po_time = dtM.Rows[0]["PO_TIME"].ToString().Trim().Split('/');
            string po_time_str = (Convert.ToInt32(po_time[0]) - 1911) + "年" + po_time[1] + "月" + po_time[2] + "日";
            exc2_0.SetCellValue("訂購日期：" + po_time_str);
            exc2_0.CellStyle = contentStyle;

            ICell exc2_7 = exr2.CreateCell(7);
            exc2_7.SetCellValue("廠商：" + dtM.Rows[0]["AGEN_NO"].ToString().Trim() + "  " + dtM.Rows[0]["AGEN_NAMEC"].ToString().Trim());
            exc2_7.CellStyle = contentStyle;

            // row 3
            IRow exr3 = sheet.CreateRow(3);
            ICell exc3_0 = exr3.CreateCell(0);
            exc3_0.SetCellValue("訂購方式：電子郵件");
            exc3_0.CellStyle = contentStyle;

            ICell exc3_7 = exr3.CreateCell(7);
            exc3_7.SetCellValue("電話：" + dtM.Rows[0]["AGEN_TEL"].ToString().Trim());
            exc3_7.CellStyle = contentStyle;

            ICell exc3_10 = exr3.CreateCell(10);
            exc3_10.SetCellValue("傳真：" + dtM.Rows[0]["AGEN_FAX"].ToString().Trim());
            exc3_10.CellStyle = contentStyle;

            // row 4
            IRow exr4 = sheet.CreateRow(4);
            ICell exc4_0 = exr4.CreateCell(0);
            exc4_0.SetCellValue("備註：");
            exc4_0.CellStyle = contentStyle;

            // row 5
            IRow exr5 = sheet.CreateRow(5);
            ICell exc5_0 = exr5.CreateCell(0);
            exc5_0.SetCellValue(dtM.Rows[0]["MEMO"].ToString().Trim());

            // row 6
            IRow exr6 = sheet.CreateRow(6);
            ICell exc6_0 = exr6.CreateCell(0);
            exc6_0.SetCellValue(dtM.Rows[0]["SMEMO"].ToString().Trim());

            // row 7
            IRow exr7 = sheet.CreateRow(7);
            ICell exc7_0 = exr7.CreateCell(0);
            exc7_0.SetCellValue("送貨地點：" + GetHospName() + " " + dtM.Rows[0]["REC_ADDR"].ToString().Trim());
            exc7_0.CellStyle = contentStyle;

            // row 8
            IRow exr8 = sheet.CreateRow(8);
            ICell exc8_0 = exr8.CreateCell(0);
            exc8_0.SetCellValue("聯絡電話：" + dtM.Rows[0]["CONTACT"].ToString().Trim());
            exc8_0.CellStyle = contentStyle;

            // row 10
            IRow exr10 = sheet.CreateRow(10);
            ICell exc10_0 = exr10.CreateCell(0);
            string dateToday = (DateTime.Now.Year - 1911) + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日";
            exc10_0.SetCellValue("列印日期：" + dateToday);
            exc10_0.CellStyle = contentStyle;

            // row 12
            IRow exr12 = sheet.CreateRow(12);
            int ci = 0;
            foreach (DataColumn dc in dtD.Columns)
            {
                ICell excTT = exr12.CreateCell(ci);
                excTT.SetCellValue(dc.ColumnName);
                excTT.CellStyle = tableStyle;
                ci++;
            }
            ci = 0;

            // row 13+
            int ri = 0;
            foreach (DataRow dr in dtD.Rows)
            {
                IRow exrTbr = sheet.CreateRow(13 + ri);
                foreach (object v in dr.ItemArray)
                {
                    ICell excTbc = exrTbr.CreateCell(ci);
                    excTbc.SetCellValue(v.ToString());
                    excTbc.CellStyle = contentStyle;
                    ci++;
                }
                ri++;
                ci = 0;
            }

            // row 總價
            IRow exrS = sheet.CreateRow(13 + ri);
            ICell excS_0 = exrS.CreateCell(0);
            excS_0.SetCellValue("總價：");
            excS_0.CellStyle = contentStyle;
            ICell excS_7 = exrS.CreateCell(7);
            excS_7.SetCellValue(dtM.Rows[0]["AMOUNT"].ToString().Trim());
            excS_7.CellStyle = contentStyle;

            // row last
            IRow exrL = sheet.CreateRow(15 + ri);
            ICell excL_0 = exrL.CreateCell(0);
            excL_0.SetCellValue("請注意：當有指定[分批交貨日期]時，請準時於指定日期交貨");
            excL_0.CellStyle = contentStyle;

            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            workbook.Write(fs);
            fs.Dispose();
            workbook.Close();
        }
    }
}
