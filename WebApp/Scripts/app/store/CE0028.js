Ext.define('WEBAPP.store.CE0028', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CE0028',
    pageSize: 9999, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE_A', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CE0028/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});