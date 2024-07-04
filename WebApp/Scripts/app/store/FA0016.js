Ext.define('WEBAPP.store.FA0016', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0016',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'APPDEPT', direction: 'ASC' }, { property: 'MAT_CLASS', direction: 'ASC' },{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0016/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});