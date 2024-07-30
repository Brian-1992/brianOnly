Ext.define('WEBAPP.store.FA0074', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0074',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    proxy: {
        type: 'ajax',
        timeout: 1800000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0074/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});