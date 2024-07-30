using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS.Models
{
    /// <summary>
    /// 介接HIS STKDMIT(庫存基本資料表)資料表所有欄位
    /// </summary>
    public class BASSTKAModels
    {
        public string SKORDERCODE { get; set; } //1.藥品藥材代碼
        public string DCMASSAGECODE { get; set; } //2.藥品停用通知
        public string DMITDCCODE { get; set; } //3.藥品進貨異動
        public string MANUFACTURER { get; set; } //4.原製造商
        public string WCOSTAMOUNT { get; set; } //5.移動平均加權價weighted
        public string PUBLICDRUGCODE { get; set; } //6.公藥分類(三總藥學專用欄位)
        public string STOCKUSECODE { get; set; } //7.扣庫規則分類(三總藥學專用欄位)
        public string SPECNUNIT { get; set; } //8.規格量及單位
        public string COMPONENTNUNIT { get; set; } //9.成份量及單位
        public string YEARARMYNO { get; set; } //10.軍聯項次年號 (M) 0960205
        public string ITEMARMYNO { get; set; } //11.軍聯項次號
        public string GROUPARMYNO { get; set; } //12.軍聯項次分類
        public string CONTRACTEFFECTIVEDATE { get; set; } //13.合約效期
        public string MULTIPRESCRIPTIONCODE { get; set; } //14.藥品單複方
        public string DRUGCLASS { get; set; } //15.藥品類別
        public string DRUGCLASSIFY { get; set; } //16.藥品性質欄位(僅做藥品之分類，線上並無作用)
        public string DRUGFORM { get; set; } //17.藥品劑型
        public string COMMITTEEMEMO { get; set; } //18.藥委會註記 (M)0951027 增加欄位長度
        public string COMMITTEECODE { get; set; } //19.藥委會品項 (M)0951204 reName 修改定義
        public string INVENTORYFLAG { get; set; } //20.盤點品項 Y/N
        public string APPLYUNIT { get; set; } //21.院內單位
        public string PURCHASECASETYPE { get; set; } //22.藥品採購案別
        public string MAXCURECONSISTENCY { get; set; } //23.TDM 合理治療濃度上限
        public string MINCURECONSISTENCY { get; set; } //24.TDM 合理治療濃度下限
        public string PEARBEGIN { get; set; } //25.TDM 合理PEAK起
        public string PEAREND { get; set; } //26.TDM 合理PEAK迄
        public string TROUGHBEGIN { get; set; } //27.TDM 合理 Trough 起
        public string TROUGHEND { get; set; } //28.TDM 合理 Trough 迄
        public string DANGERBEGIN { get; set; } //29.TDM 危急值 起
        public string DANGEREND { get; set; } //30.TDM 危急值 迄
        public string TDMMEMO1 { get; set; } //31.TDM 備註1 (M) 0951222
        public string TDMMEMO2 { get; set; } //32.TDM 備註2 (M) 0951222
        public string TDMMEMO3 { get; set; } //33.TDM備 註3 (M) 0951222
        public string APPLYTRANSQTY { get; set; } //34.單位轉換量(院內/最小)
        public string PURCHASETRANSQTY { get; set; } //35.單位轉換量(採購/院內)
        public string CHINATTENTION { get; set; } //36.注意事項(中文) (M) 0951222
        public string ENGATTENTION { get; set; } //37.注意事項(英文) (M) 0951222
        public string DRUGMEMO { get; set; } //38.處方集
        public string CHINSIDEEFFECT { get; set; } //39.主要副作用(中文)
        public string ENGSIDEEFFECT { get; set; } //40.主要副作用(英文)
        public string WARN { get; set; } //41.警語
        public string DOHSYMPTOM { get; set; } //42.衛生署核准適應症
        public string FDASYMPTOM { get; set; } //43.FDA核准適應症
        public string SUCKLESECURITY { get; set; } //44.授乳安全性
        public string PREGNANTGRADE { get; set; } //45.懷孕分級
        public string DRUGEXTERIOR { get; set; } //46.藥品外觀 (M) 0951222
        public string PURCHASEUNIT { get; set; } //47.採購單位 (衛材系統叫 申請單位)
        public string STOCKSOURCECODE { get; set; } //48.來源代碼
        public string MAMAGEFLAG { get; set; } //49.是否收管理費
        public string MAMAGERATE { get; set; } //50.管理費%
        public string DRUGFLAG { get; set; } //51.藥品衛材別
        public string FIRSTPURCHASEDATE { get; set; } //52.第一次進貨日期
        public string CREATEDATETIME { get; set; } //53.記錄建立日期/時間
        public string CREATEOPID { get; set; } //54.記錄建立人員
        public string PROCDATETIME { get; set; } //55.記錄處理日期/時間
        public string PROCOPID { get; set; } //56.記錄處理人員
        public string DCMASSAGEMEMO { get; set; } //57.藥品異動備註
        public string DRUGLEAFLETLINK { get; set; } //58.藥品仿單檔名(A) 0951027
        public string DRUGPICTURELINK { get; set; } //59.藥品圖片檔名(A) 0951027
        public string COMPONENTNUNIT2 { get; set; } //60.成份量及單位2(A) 0951027
        public string COMPONENTNUNIT3 { get; set; } //61.成份量及單位3(A) 0951027
        public string COMPONENTNUNIT4 { get; set; } //62.成份量及單位4(A) 0951027
        public string DRUGENGEXTERIOR { get; set; } //63.藥品外觀(英文)(A) 0951027 (M) 0951222
        public string DRUGAPPLYTYPE { get; set; } //64.藥品請領類別(A)
        public string ARMYORDERCODE { get; set; } //65.軍品院內碼(A)0951205
        public string PARENTCODE { get; set; } //66.母藥註記 (A)0951222
        public string PARENTORDERCODE { get; set; } //67.母藥院內碼 (A)0951222
        public string SONTRANSQTY { get; set; } //68.子藥轉換量 (A)0951222
        public string CLASSIFIEDARMYNO { get; set; } //69.軍聯項次組別
        public string DRUGTOTALAMOUNT { get; set; } //70
        public string DRUGTOTALAMOUNTUNIT { get; set; } //71
        public string DRUGPACKAGE { get; set; } //72
    }
}
