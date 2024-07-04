Ext.define('WEBAPP.store.MI_MAST_ViewModel', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MI_MAST'
    ],
    stores: {
        MMCODE: {
            model: 'WEBAPP.model.MI_MAST',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                //url: '/api/AB0021/GetMmcode',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }

});