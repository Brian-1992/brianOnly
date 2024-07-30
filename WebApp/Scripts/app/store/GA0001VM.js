Ext.define('WEBAPP.store.GA0001VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.TC_INVQMTR'
    ],
    stores: {
        TC_INVQMTR: {
            model: 'WEBAPP.model.TC_INVQMTR',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/GA0001/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }
});