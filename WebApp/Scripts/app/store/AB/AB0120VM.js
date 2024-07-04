Ext.define('WEBAPP.store.AB.AB0120VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0120M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MAT_CLASS', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        timeout: 90000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0120/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});