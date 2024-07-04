Ext.define('WEBAPP.store.AA.AA0074VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA0074M',
    pageSize: 50, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MAT_CLASS', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }, { property: 'DATA_YM', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        timeout: 90000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0074/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});