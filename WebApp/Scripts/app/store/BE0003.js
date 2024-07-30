Ext.define('WEBAPP.store.BE0003', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BE0003',
    pageSize: 99999, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'PO_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BC0003/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});