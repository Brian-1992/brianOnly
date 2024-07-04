Ext.define('WEBAPP.store.AB.AB0059VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0059M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MAT_CLASS', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        timeout: 90000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0059/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});