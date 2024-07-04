Ext.define('WEBAPP.store.ME_DOCD', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ME_DOCD',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'DOCNO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0003/AllD',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});