Ext.define('WEBAPP.store.CD0005M', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CD0005M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'LOT_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CD0005/AllM',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});