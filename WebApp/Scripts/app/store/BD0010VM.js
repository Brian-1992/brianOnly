Ext.define('WEBAPP.store.BD0010VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MM_PO_M',
        'WEBAPP.model.MM_PO_D'
    ],
    stores: {
        MM_PO_M: {
            model: 'WEBAPP.model.MM_PO_M',
            pageSize: 999999, // 每頁顯示筆數
            remoteSort: true,  
            sorters: [{ property: 'PO_NO', direction: 'ASC' }, { property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BD0010/MasterAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        MM_PO_D: {
            model: 'WEBAPP.model.MM_PO_D',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BD0010/DetailAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
      
    }

});