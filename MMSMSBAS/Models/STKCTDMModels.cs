using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    public class STKCTDMModels
    {
        public string STOCKCODE { get; set; } //1.庫別代碼
        public string SKORDERCODE { get; set; } //2.藥品藥材代碼
        public string STOCKQTY { get; set; } //3.現有存量
        public string AVGCONSUMPTION { get; set; } //4.日平均消耗量
        public string SAVEDAYS { get; set; } //5.安全存量天數
        public string SKWORKDAYS { get; set; } //6.作業量天數
        public string SAFEQTY { get; set; } //7.安全存量
        public string FLOORQTY { get; set; } //8.最低庫存量
        public string STOCKPLACE { get; set; } //9.儲位
        public string STOCKRECEIPTNO { get; set; } //10.進藥批號
        public string EFFECTDATE { get; set; } //11.最新效期(年月)
        public string NOWCONSUMEFLAG { get; set; } //12.撥發直接消耗
        public string RESERVEFLAG { get; set; } //13.備用藥
        public string PARENTSTOCKCODE { get; set; } //14.上級庫
        public string MINPACKQTY { get; set; } //15.最小包裝量
        public string CTDMDCCODE { get; set; } //16.各庫停用碼
        public string ADDITIONCOMPUTEFLAG { get; set; } //17.附帶衛材加做是否計價
        public string CHANGECOMPUTEFLAG { get; set; } //18.是否開放線上衛材設定計價
        public string APPLYQTY { get; set; } //19.已申請量(預留欄位，三總無用)
        public string LASTUSEDATE { get; set; } //20.最後使用(消耗)日
        public string LASTAPPLYDATE { get; set; } //21.最後請領日
        public string CREATEDATETIME { get; set; } //22.記錄建立日期/時間
        public string CREATEOPID { get; set; } //23.記錄建立人員
        public string PROCDATETIME { get; set; } //24.記錄處理日期/時間
        public string PROCOPID { get; set; } //25.記錄處理人員
        public string OPENDATE { get; set; } //26.系統啟用日期
        public string SURPLUSINSUQTY { get; set; } //27.小單位健保剩餘量
        public string SURPLUSHOSPQTY { get; set; } //28.小單位自費剩餘量
        public string UDPLACE { get; set; } //29.UP儲位
        public string FIRSTPLACE { get; set; } //30.首日區儲位
        public string FIXEDSTOCK_FLAG { get; set; } //
    }
}
