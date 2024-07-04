Ext.define('WEBAPP.store.AB.AB0038VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.AB0038VM'
    ],
    stores: {
        AB0038VM_L: {
            model: 'WEBAPP.model.AB0038VM',
            pageSize: 1000, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'ENAME', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0038/GetColList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        AB0038VM_R: {
            model: 'WEBAPP.model.AB0038VM',
            pageSize: 1000,
            remoteSort: true,
            sorters: [{ property: 'SEQ', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0038/GetDefaultList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }

});