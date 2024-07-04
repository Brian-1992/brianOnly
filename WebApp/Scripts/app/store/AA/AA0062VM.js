Ext.define('WEBAPP.store.AA.AA0062VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA0062M',
    pageSize: 50, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MAT_CLASS', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }, { property: 'EXP_DATE', direction: 'DESC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0062/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});