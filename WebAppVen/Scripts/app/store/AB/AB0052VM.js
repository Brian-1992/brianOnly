Ext.define('WEBAPP.store.AB.AB0052VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MeExpd'
    ],
    stores: {
        MeExpd: {
            model: 'WEBAPP.model.MeExpd',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'EXP_SEQ', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0052/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }

});