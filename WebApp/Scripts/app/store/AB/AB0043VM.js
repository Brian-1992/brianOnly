Ext.define('WEBAPP.store.AB.AB0043VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.PHRSDPT'
    ],
    stores: {
        PHRSDPT: {
            model: 'WEBAPP.model.PHRSDPT',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'RXTYPE', direction: 'ASC' }],   // 預設庫房代碼排序欄位 
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET 
                },
                url: '/api/AB0043/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts', 
                    totalProperty: 'rc'
                }
            }
        }
    }
});