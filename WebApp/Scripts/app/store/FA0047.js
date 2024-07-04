Ext.define('WEBAPP.store.FA0047', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0047',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'mmcode', direction: 'ASC' }, { property: 'data_ym', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        timeout: 1800000,
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0047/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});