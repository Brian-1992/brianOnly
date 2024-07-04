Ext.define('WEBAPP.store.FA0007VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0007M',
    pageSize: 50, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0007/All',
        timeout:0,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});