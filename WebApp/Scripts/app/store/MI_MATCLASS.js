Ext.define('WEBAPP.store.MI_MATCLASS', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MI_MATCLASS',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MAT_CLASS', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0118/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty:'rc'
        }
    }
});