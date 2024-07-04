Ext.define('WEBAPP.store.FA0076', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0076',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'appdept', direction: 'ASC' }, { property: 'appdate', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0076/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});