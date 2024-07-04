Ext.define('WEBAPP.store.AB0079VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.ME_AB0079'
    ],
    stores: {
        ME_AB0079: {
            model: 'WEBAPP.model.ME_AB0079',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'ORDERCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0079/QueryD',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }

    }

});