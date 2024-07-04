Ext.define('WEBAPP.store.AB0132', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0132',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [
        { property: 'DATA_DATE', direction: 'ASC' },
        { property: 'ORDERCODE', direction: 'ASC' },
        { property: 'ORDERDR', direction: 'ASC' },
        { property: 'MEDNO', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0132/All',
        timeout: 9000000,
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});