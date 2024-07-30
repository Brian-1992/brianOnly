Ext.define('WEBAPP.store.CE0040M', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CE0040M',
    pageSize: 10, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'CHK_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CE0040/AllM',
        timeout: 9000000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});