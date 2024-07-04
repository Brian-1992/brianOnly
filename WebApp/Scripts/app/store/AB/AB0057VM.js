Ext.define('WEBAPP.store.AB.AB0057VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0057M',
    pageSize: 50, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MAT_CLASS', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }, { property: 'EXP_DATE', direction: 'DESC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0057/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});