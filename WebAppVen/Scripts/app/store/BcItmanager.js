Ext.define('WEBAPP.store.BcItmanager', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BcItmanager',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CB0006/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty:'rc'
        }
    }
});