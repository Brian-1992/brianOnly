Ext.define('WEBAPP.store.FA0010', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0010',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    proxy: {
        type: 'ajax',
        timeout: 1800000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0010/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});