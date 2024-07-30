Ext.define('WEBAPP.store.AB.AB0045VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.ME_CSTM'
    ],
    stores: {
        ME_CSTM: {
            model: 'WEBAPP.model.ME_CSTM',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'CSTM', direction: 'ASC' }],   // 預設庫房代碼排序欄位 
            proxy: {
                type: 'ajax', 
                actionMethods: {
                    read: 'POST' // by default GET 
                },
                url: '/api/AB0045/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }
});