using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using JCLib.DB.Tool;
using System.Configuration;
using System.IO;

using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Newtonsoft.Json;

namespace CALLSP
{
    class Program
    {
        // -- 01.呼叫SP(不帶參數) ---------------------------------------------------
        //drop table flylon_sp
        //create table flylon_sp(dt date)
        //insert into flylon_sp values(sysdate-100)
        //select* from flylon_sp

        //CREATE OR REPLACE PROCEDURE TEST_PROC1
        //AS
        //BEGIN
        //    update flylon_sp set dt=sysdate;
        //    DBMS_OUTPUT.PUT_LINE('call StoredProcedure ok ');
        //END TEST_PROC1;

        // -- 02.呼叫SP(帶O_RETID回傳參數) --------------------------------------------------
        //CREATE OR REPLACE PROCEDURE TEST_PROC2
        //(O_RETID OUT VARCHAR2)
        //AS
        //BEGIN
        //update flylon_sp set dt = sysdate;
        //O_RETID:='456'; -- 如果上面宣告(NO_IN IN NUMBER) 不能再指定值給NO_IN, 宣告成 NO_IN OUT NUMBER就可以指定
        //END TEST_PROC2;

        // http://trufflepenne.blogspot.com/2011/09/coracle-function.html
        // https://dahao.blogspot.com/2015/09/coracle-stored-procedure.html

        // -- 03.呼叫SP(帶IN與Out 回傳參數)
        //CREATE OR REPLACE PROCEDURE TEST_PROC3
        //(A IN VARCHAR2, B OUT VARCHAR2)   
        //AS
        //BEGIN
        //    update flylon_sp set dt=sysdate;
        //    select A || ' ' || to_char(dt,'yyyy/mm/dd hh24:mi:ss') into B from flylon_sp;
        //    -- B:=A; -- 如果上面宣告(NO_IN IN NUMBER) 不能再指定值給NO_IN, 宣告成 NO_IN OUT NUMBER就可以指定
        //END TEST_PROC3;

        //set serveroutput on
        //declare 
        //    A varchar2(100):='現在時刻';
        //    B varchar2(100):='';
        //begin
        //    TEST_PROC3(A,B);
        //    dbms_output.put_line(B);
        //end;


        // -- 04.呼叫範例 -- 
        //void 呼叫範例()
        //{
        //    String s = "";
        //    if (args.Length > 0)
        //    {
        //        String storedProcedureName = args[0];
        //        String dbmsg = "";
        //        CallDBtools_Oracle t = new CallDBtools_Oracle();
        //        // -- 01.呼叫SP(不帶參數) --
        //        //t.CallExecSPWithoutRtn( 
        //        //    storedProcedureName,
        //        //    null,
        //        //    "oracle",
        //        //    ref dbmsg
        //        //);

        //        // -- 02.呼叫SP(帶O_RETID參數OUT) --
        //        //String sRtn = t.CallExecSpReturnOneVarchar2(
        //        //    storedProcedureName,
        //        //    "O_RETID",
        //        //    null,
        //        //    "oracle",
        //        //    ref dbmsg
        //        //);

        //        // -- 03.呼叫SP(帶IN與Out 回傳參數)
        //        SP sp = new SP();
        //        SpItem spi = null;

        //        //spi = new SpItem();
        //        //spi.name = "a";
        //        //spi.direction = "in";
        //        //spi.type = "varchar2";
        //        //spi.value = "123";
        //        //sp.items.Add(spi);

        //        //spi = new SpItem();
        //        //spi.name = "O_RETID";
        //        //spi.direction = "out";
        //        //spi.type = "varchar2";
        //        //spi.value = "";
        //        //sp.items.Add(spi);


        //        //CREATE OR REPLACE PROCEDURE TEST_PROC2
        //        //(O_RETID IN VARCHAR2, B OUT VARCHAR2)
        //        //AS
        //        //BEGIN
        //        //update flylon_sp set dt = sysdate;
        //        //B:= '456';
        //        //END TEST_PROC2;

        //        spi = new SpItem();
        //        spi.name = "O_RETID";
        //        spi.direction = "in";
        //        spi.type = "varchar2";
        //        spi.value = "zxc";
        //        sp.items.Add(spi);

        //        spi = new SpItem();
        //        spi.name = "B";
        //        spi.direction = "out";
        //        spi.type = "varchar2";
        //        spi.value = "";
        //        sp.items.Add(spi);

        //        string json = JsonConvert.SerializeObject(sp);
        //        SP nSp = JsonConvert.DeserializeObject<SP>(json);

        //        SP rSp = t.CallExecSpInOutParameter(
        //            storedProcedureName,
        //            nSp,
        //            "oracle",
        //            ref dbmsg
        //        );

        //        if (dbmsg == "")
        //        {
        //            s += "執行完成,資料庫回傳:";
        //            if (rSp != null)
        //            {
        //                foreach (SpItem i in rSp.items)
        //                {
        //                    if (i.direction.ToLower().Equals("out"))
        //                        s += "[" + i.name + "]=" + i.value + sBr;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            s += "程式執行錯誤，錯誤訊息:" + dbmsg;
        //        }
        //    }
        //    else
        //    {
        //        s += "沒傳入【StoredProcedure名稱】的參數哦" + sBr;
        //        s += "呼叫範例如下" + sBr;
        //        s += "  CallSP.exe 某StoredProcedure名稱" + sBr;

        //    }
        //    Console.Write(s);
        //} // 



        String sBr = "\r\n";
        FL fl = new FL("CALLSP");
        L l = new L("CALLSP");
        static void Main(string[] args)
        {
            Program p = new Program();
            p.run(args);
        }

        void run(string[] args)
        {
            l.lg("run()", "args.Length=" + args.Length);
            String s = "";
            if (args.Length > 1)
            {
                String storedProcedureName = args[0];
                String json = args[1];
                s += "storedProcedureName=" + storedProcedureName + sBr;
                s += "json=" + json + sBr;
                String dbmsg = "";
                CallDBtools_Oracle t = new CallDBtools_Oracle();
                SP nSp = JsonConvert.DeserializeObject<SP>(json);

                SP rSp = t.CallExecSpInOutParameter(
                    storedProcedureName,
                    nSp,
                    "oracle",
                    ref dbmsg
                );

                if (dbmsg == "")
                {
                    s += "執行完成,資料庫回傳:" + sBr;
                    if (rSp != null)
                    {
                        foreach (SpItem i in rSp.items)
                        {
                            if (i.direction.ToLower().Equals("out"))
                                s += "[" + i.name + "]=" + i.value + sBr;
                        }
                    }
                }
                else
                {
                    s += "程式執行錯誤，錯誤訊息:" + dbmsg;
                }
                l.lg("run()", s);
            }
            else
            {
                s += "傳入參數不正確，請重新確認參數!!!" + sBr;
                s += "" + sBr;
                s += "呼叫範例說明" + sBr;
                s += "  CallSP.exe [Stored Procedure name] [json Format] " + sBr;
                s += "" + sBr;
                s += "  [json Format] " + sBr;
                s += "例如" + sBr;
                s += "CREATE OR REPLACE PROCEDURE TEST_PROC2" + sBr;
                s += "(A IN VARCHAR2, B OUT VARCHAR2)" + sBr;
                s += "AS" + sBr;
                s += "BEGIN" + sBr;
                s += "B:='hello' || ' ' || A; -- 如果上面宣告(NO_IN IN NUMBER) 不能再指定值給NO_IN, 宣告成 NO_IN OUT NUMBER就可以指定" + sBr;
                s += "END TEST_PROC2;" + sBr;
                s += sBr;
                s += "需輸入以下Json格式" + sBr;
                s += "{" + sBr;
                s += "	\"items\":" + sBr;
                s += "	[" + sBr;
                s += "		{\"name\":\"a\",\"direction\":\"in\",\"type\":\"varchar2\",\"value\":\"123\"}," + sBr;
                s += "		{\"name\":\"b\",\"direction\":\"out\",\"type\":\"varchar2\",\"value\":\"\"}" + sBr;
                s += "	]" + sBr;
                s += "}" + sBr;
                s += "" + sBr;
                s += "呼叫範例01" + sBr;
                s += "CALLSP.exe \"TEST_PROC2\" \"{\\\"items\\\":[{\\\"name\\\":\\\"A\\\",\\\"direction\\\":\\\"in\\\",\\\"type\\\":\\\"varchar2\\\",\\\"value\\\":\\\"flylon\\\"},{\\\"name\\\":\\\"B\\\",\\\"direction\\\":\\\"out\\\",\\\"type\\\":\\\"varchar2\\\",\\\"value\\\":\\\"\\\"}]}\"" + sBr;
                s += sBr;
                s += "呼叫範例02" + sBr;
                s += "CALLSP.exe \"GEN_PR_MR12\" \"{\\\"items\\\":[{\\\"name\\\":\\\"O_RETID\\\",\\\"direction\\\":\\\"out\\\",\\\"type\\\":\\\"varchar2\\\",\\\"value\\\":\\\"\\\"}]}\"" + sBr;
            }
            fl.sendMailToAdmin("三總排程-CALLSP執行", s.Replace("\r\n","<BR>"));
            Console.Write(s);
        } // 

        

    } // en
} // ec
