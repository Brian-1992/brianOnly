Ext.define('WEBAPP.store.FA0043', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AA.AA0102',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'INID', direction: 'ASC' }, { property: 'WH_NO', direction: 'ASC' }, { property: 'CHK_TYPE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0102/All_FA0043', //依照AA0103修改
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});