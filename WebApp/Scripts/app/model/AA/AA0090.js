Ext.define('WEBAPP.model.AA.AA0090', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' }, //庫別代碼
        { name: 'WH_NAME', type: 'string' }, //庫別名
        { name: 'LOW_QTY', type: 'string' }, //最低庫存
        { name: 'SAFE_DAY', type: 'string' }, //安全存量天數
        { name: 'SAFE_QTY', type: 'string' }, //安全存量
        { name: 'OPER_DAY', type: 'string' }, //基準天數
        { name: 'OPER_QTY', type: 'string' }, //基準量
        { name: 'CANCEL_ID', type: 'string' }, //各庫停用
        { name: 'MIN_ORDQTY', type: 'string' }, //最小包裝
        { name: 'STORE_LOC', type: 'string' }, //儲位
        { name: 'INV_QTY', type: 'string' }, //庫存量
        { name: 'PWH_NO', type: 'string' }, //上級庫
    ]
});