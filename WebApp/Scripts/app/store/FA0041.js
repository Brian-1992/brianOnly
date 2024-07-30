Ext.define('WEBAPP.store.FA0041', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0041',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'INID', direction: 'ASC' }, { property: 'WH_NO', direction: 'ASC' }, { property: 'CHK_TYPE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0041/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});