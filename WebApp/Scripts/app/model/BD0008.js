Ext.define('WEBAPP.model.BD0008', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'RECYM', type: 'string' }, //月份
        { name: 'MMCODE', type: 'string' }, //院內碼
        { name: 'M_PURUN', type: 'string' }, //申購劑量單位
        { name: 'M_CONTPRICE', type: 'string' }, //合約價
        { name: 'ADVISEQTY', type: 'string' }, //建議量
        { name: 'ADVISEMONEY', type: 'string' }, //建議金額
        //{ name: 'SEKQTY', type: 'string' }, //現存量
        { name: 'ESTQTY', type: 'string' }, //預估量//////
        { name: 'AMOUNT', type: 'string' }, //單筆價
        { name: 'CONTRACNO', type: 'string' }, //合約
        { name: 'AGEN_NO', type: 'string' }, //廠商
        { name: 'INV_QTY', type: 'string' }, //現存量

        
    ]
});