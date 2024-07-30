Ext.define('WEBAPP.model.CD0005M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'ACT_PICK_QTY_CODE', type: 'string' }, //揀貨差異
        { name: 'HAS_CONFIRMED', type: 'string' }, //確認狀態
        { name: 'WH_NO', type: 'string' }, //庫房代碼
        { name: 'PICK_DATE', type: 'string' }, //揀貨日期
        { name: 'LOT_NO', type: 'string' }, //揀貨批號(畫面)
        { name: 'DOCNO', type: 'string' }, //申請單號碼
        { name: 'PICK_USERID', type: 'string' }, //分配揀貨人員代碼
        { name: 'PICK_USERNAME', type: 'string' }, //揀貨人員名稱(畫面)
        { name: 'HAS_CONFIRMED_CODE', type: 'string' }, //是否已確認揀貨結果
        { name: 'CONFIRM_STATUS', type: 'string' }, //是否已確認揀貨結果(畫面)
        { name: 'ITEM_CNT_SUM', type: 'string' }, //品項數(畫面)
        { name: 'APPQTY_SUM', type: 'string' }, //總件數(畫面)
        { name: 'PICK_ITEM_CNT_SUM', type: 'string' }, //已揀項數(畫面)
        { name: 'ACT_PICK_QTY_SUM', type: 'string' } //已揀總件數(畫面)
    ]
});