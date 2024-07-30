Ext.define('WEBAPP.store.BA0007', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MM_PR_DTBAS',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'BEGINDATE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BA0007/getAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});