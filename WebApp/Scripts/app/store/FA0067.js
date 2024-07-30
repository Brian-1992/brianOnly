Ext.define('WEBAPP.store.FA0067', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0067',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MAT_CLASS', direction: 'ASC' }, { property: 'mmcode', direction: 'ASC' }, { property: 'data_ym', direction: 'ASC' }, { property: 'miltype', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0067/All',
        timeout: 900000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});