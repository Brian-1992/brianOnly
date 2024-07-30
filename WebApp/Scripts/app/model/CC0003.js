Ext.define('WEBAPP.model.CC0003', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'VACCINE', type: 'string' }, //疫苗(畫面)
        { name: 'CONTRACNO', type: 'string' }, //合約碼(畫面)
        { name: 'E_PURTYPE', type: 'string' }, //案別(畫面)
        { name: 'PURDATE', type: 'string' }, //採購日期(畫面)
        { name: 'PO_NO', type: 'string' }, //採購單號(畫面)
        { name: 'PO_NO_REF', type: 'string' }, //採購單號(畫面)
        { name: 'AGEN_NO', type: 'string' }, //廠商代碼(畫面)
        { name: 'AGEN_NO_NAME', type: 'string' }, //廠商名稱(畫面)
        { name: 'MMCODE', type: 'string' }, //院內碼(畫面)
        { name: 'MMNAME_C', type: 'string' }, //中文品名(畫面)
        { name: 'MMNAME_E', type: 'string' }, //英文品名(畫面)
        { name: 'ACCOUNTDATE', type: 'string' }, //進貨日期(畫面)
        { name: 'ACCOUNTDATE_REF', type: 'string' }, //原進貨日期
        { name: 'PO_QTY', type: 'string' }, //預計進貨量(畫面)
        { name: 'DELI_QTY', type: 'string' }, //實際進貨量(畫面)
        { name: 'DELI_QTY_REF', type: 'string' }, //原實際進貨量
        { name: 'INFLAG', type: 'string' }, //進貨(畫面)
        { name: 'OUTFLAG', type: 'string' }, //退貨(畫面)
        { name: 'PO_PRICE', type: 'string' }, //單價(畫面)
        { name: 'PO_AMT', type: 'string' }, //總金額(畫面)
        { name: 'M_PURUN', type: 'string' }, //進貨單位(畫面)
        { name: 'LOT_NO', type: 'string' }, //批號(畫面
        { name: 'LOT_NO_REF', type: 'string' }, //原批號
        { name: 'EXP_DATE', type: 'string' }, //效期(畫面
        { name: 'EXP_DATE_REF', type: 'string' }, //原效期
        { name: 'MEMO', type: 'string' }, //備註(畫面)
        { name: 'MEMO_REF', type: 'string' }, //原備註
        { name: 'WH_NO', type: 'string' }, //庫房別(畫面)
        { name: 'STATUS', type: 'string' }, //狀態(畫面)
        { name: 'ORI_QTY', type: 'string' }, //進貨量(畫面)
        { name: 'SEQ', type: 'string' }, //流水號
        { name: 'M_DISCPERC', type: 'string' },//折讓比
        { name: 'UNIT_SWAP', type: 'string' },//轉換率
        { name: 'UPRICE', type: 'string' },//最小單價; 計量單位單價
        { name: 'DISC_CPRICE', type: 'string' },//優惠合約單價
        { name: 'DISC_UPRICE', type: 'string' },//優惠最小單價; 計量單位優惠單價
        { name: 'TRANSKIND', type: 'string' },//異動類別
        { name: 'IFLAG', type: 'string' },//新增識別
    ]
});