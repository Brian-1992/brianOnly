Ext.define('WEBAPP.store.ItemAllocationByDept', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ItemAllocation',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'TOWH', direction: 'ASC' }, { property: 'APPTIME_YM', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0061/AllByDept',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});