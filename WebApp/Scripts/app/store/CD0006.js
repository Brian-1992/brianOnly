Ext.define('WEBAPP.store.CD0006', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CD0006',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'PICK_DATE,DOCNO,SEQ', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CD0006/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});