Ext.define('WEBAPP.model.BD0002D', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' }, //院內碼
        { name: 'MMNAME_C', type: 'string' }, //中文品名
        { name: 'MMNAME_E', type: 'string' }, //英文品名
        { name: 'M_PURUN', type: 'string' }, //申購計量單位
        { name: 'PR_QTY', type: 'string' },//申購數量
        { name: 'PR_PRICE', type: 'string' }, //申購單價
        { name: 'M_CONTPRICE', type: 'string' }, //合約單價
        { name: 'UNIT_SWAP', type: 'string' }, //轉換率
        { name: 'REQ_QTY_T', type: 'string' }, //包裝單位數量
        { name: 'AGEN_NO', type: 'string' }, //廠商代碼
        { name: 'DISC', type: 'string' }, //折讓比
        { name: 'REC_STATUS', type: 'string' }, //申請單狀態
        { name: 'AGEN_NAME', type: 'string' }, //廠商名稱
        { name: 'M_AGENLAB', type: 'string' }, //廠牌
        { name: 'AGEN_TEL', type: 'string' }, //廠商電話
        { name: 'BASE_UNIT', type: 'string' } //最小單位 
    ]
});