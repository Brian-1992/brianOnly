Ext.define('WEBAPP.store.AA.AA0006VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.AA0006M',
        'WEBAPP.model.AA0006D'
    ],
    stores: {
        AA0006M: {
            model: 'WEBAPP.model.AA0006M',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'PO_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0006/MasterAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        AA0006D: {
            model: 'WEBAPP.model.AA0006D',
            pageSize: 500, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0006/DetailAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
      
    }

});