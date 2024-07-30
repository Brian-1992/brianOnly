Ext.define('WEBAPP.store.CC0003_1', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CC0003',
    pageSize: 1000, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'AGEN_NO_NAME', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CC0003/All_1',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});