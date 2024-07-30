Ext.define('WEBAPP.model.BG0008', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'AGEN_NO', type: 'string' }, //廠商代碼
        { name: 'AGEN_NAMEC', type: 'string' }, //廠商名稱
        { name: 'PO_NO', type: 'string' }, //訂單編號
        { name: 'MMCODE', type: 'string' }, //院內碼
        { name: 'MMNAME_C', type: 'string' }, //中文名稱
        { name: 'MMNAME_E', type: 'string' }, //英文名稱
        { name: 'M_PURUN', type: 'string' }, //計量單位
        { name: 'PO_QTY', type: 'string' }, //訂單數量
        { name: 'PO_PRICE', type: 'string' }, //單價
        { name: 'DELI_QTY', type: 'string' }, //進貨量
        { name: 'NOIN_QTY', type: 'string' }, //未進貨量
    ]

});