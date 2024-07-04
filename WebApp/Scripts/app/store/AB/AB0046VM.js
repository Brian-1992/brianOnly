Ext.define('WEBAPP.store.AB.AB0046VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.ME_AB0046'
    ],
    stores: {
        ME_AB0046: {
            model: 'WEBAPP.model.ME_AB0046',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'VISITKIND', direction: 'ASC' }],   // 門住診別排序欄位 
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET 
                },
                url: '/api/AB0046/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }
});