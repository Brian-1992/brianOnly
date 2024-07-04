Ext.define('WEBAPP.store.AA.AA0085', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0085',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0085/All',
        timeout: 900000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});