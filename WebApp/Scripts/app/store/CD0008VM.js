Ext.define('WEBAPP.store.CD0008VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.BC_WHPICK'
    ],
    stores: {
        BC_WHPICK: {
            model: 'WEBAPP.model.BC_WHPICK',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0008/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }
});