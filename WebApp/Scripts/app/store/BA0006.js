Ext.define('WEBAPP.store.BA0006', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BA0006M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'PO_NO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BA0006/getBC_CS_ACC_LOG',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});