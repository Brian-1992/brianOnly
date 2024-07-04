Ext.define('WEBAPP.store.BG0003', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BG0003',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'M_AGENNO', direction: 'ASC' }, { property: 'MMCODE_2', direction: 'ASC' }, { property: 'APPDEPT', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BG0003/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});