Ext.define('WEBAPP.store.AB0100VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.LIS_APP'
    ],
    stores: {
        All: {
            model: 'WEBAPP.model.LIS_APP',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0100/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }

    }

});