Ext.define('WEBAPP.store.FA0008VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0008M',
    pageSize: 50, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'ROWORDER', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0008/All',
        timeout: 900000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});