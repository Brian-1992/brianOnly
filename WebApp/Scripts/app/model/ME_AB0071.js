
Ext.define('WEBAPP.model.ME_AB0071', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'CHARTNO', type: 'string' },//病歷號
        { name: 'MEDNO', type: 'string' },//病歷號碼
        { name: 'VISITSEQ', type: 'string' },//門/住診 
        { name: 'ORDERNO', type: 'string' },//醫令序號
        { name: 'DETAILNO', type: 'string' },//明細序號
        { name: 'ORDERCODE', type: 'string' },//院內碼
        { name: 'USEQTY', type: 'string' },//數量
        { name: 'CHINNAME', type: 'string' },//開立醫師
        { name: 'SIGNOPID', type: 'string' },//給藥人員
        { name: 'CREATEDATETIME', type: 'string' },//日期
        { name: 'STOCKCODE', type: 'string' },//庫房代碼
        { name: 'INOUTFLAG', type: 'string' },//A:買 D:退
        { name: 'ORDERENGNAME', type: 'string' },//英文名
        { name: 'RESTRICTCODE', type: 'string' },//管制用藥
        { name: 'HIGHPRICEFLAG', type: 'string' },//高價 
        { name: 'NRCODE', type: 'string' },//病房 
        { name: 'BEDNO', type: 'string' },//床位 
        { name: 'NRCODENAME', type: 'string' },//病房-床位
        { name: 'DOSE', type: 'string' },//劑量
        { name: 'ORDERUNIT', type: 'string' },//劑量單位
        { name: 'PATHNO', type: 'string' },//途徑
        { name: 'FREQNO', type: 'string' },//頻率
        { name: 'PAYFLAG', type: 'string' },//自費
        { name: 'BUYFLAG', type: 'string' },//自備
        { name: 'BAGSEQNO', type: 'string' },//藥袋號
        { name: 'RXNO', type: 'string' },//處方箋

    ]
});