Ext.define('WEBAPP.model.BC0005D', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' }, //院內碼(畫面)
        { name: 'MMNAME_C', type: 'string' }, //中文品名(畫面)
        { name: 'MMNAME_E', type: 'string' }, //英文品名(畫面)
        { name: 'M_AGENLAB', type: 'string' }, //廠牌(畫面)
        { name: 'M_PURUN', type: 'string' }, //申購計量單位(畫面)
        { name: 'PO_PRICE', type: 'string' }, //訂單單價(合約單價)(畫面)
        { name: 'PO_QTY', type: 'string' }, //訂單數量(包裝單位數量)(畫面)
        { name: 'PO_AMT', type: 'string' }, //總金額(畫面)
        { name: 'MEMO', type: 'string' }, //備註(畫面)
    ]
});