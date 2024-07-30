Ext.define('WEBAPP.model.AA.AA0027D', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' }, //院內碼(畫面)
        { name: 'MMCODE_TEXT', type: 'string' }, //院內碼(紅底顯示)(畫面)
        { name: 'MMNAME_C', type: 'string' }, //中文品名(畫面)
        { name: 'MMNAME_E', type: 'string' }, //英文品名(畫面)
        { name: 'AGEN_NAME', type: 'string' }, //廠商名稱(畫面)
        { name: 'LOT_NO', type: 'string' }, //批號(畫面)
        { name: 'LOT_NO_TEXT', type: 'string' }, //批號(紅底顯示)
        { name: 'EXP_DATE', type: 'string' }, //效期(畫面)
        { name: 'APVQTY', type: 'string' }, //效期數量(畫面)
        { name: 'APVQTY_TEXT', type: 'string' }, //效期數量(紅底顯示)
        { name: 'INV_QTY', type: 'string' }, //效期數量(畫面)
        { name: 'BASE_UNIT', type: 'string' }, //單位(紅底)
        { name: 'C_TYPE', type: 'string' }, //進貨/合約
        { name: 'C_TYPE_NAME', type: 'string' }, //進貨/合約(畫面)
        { name: 'C_TYPE_NAME_TEXT', type: 'string' }, //進貨/合約(紅底)
        { name: 'IN_PRICE', type: 'string' }, //進貨單價(畫面)
        { name: 'CONTPRICE', type: 'string' }, //合約單價(畫面)
        { name: 'C_AMT', type: 'string' }, //換貨金額(畫面)
        { name: 'ITEM_NOTE', type: 'string' }, //備註(畫面)
        { name: 'SEQ', type: 'string' }, //單據項次
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'DOCNO', type: 'string' }, //調帳單號
        { name: 'C_UP', type: 'string' }, //退換貨單價
        { name: 'FRWH', type: 'string' }
    ]
});