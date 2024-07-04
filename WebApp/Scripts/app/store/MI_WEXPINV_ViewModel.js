Ext.define('WEBAPP.store.MI_WEXPINV_ViewModel', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MI_WEXPINV'
    ],
    stores: {
        LOT_NO: {
            model: 'WEBAPP.model.MI_WEXPINV',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'LOT_NO', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }

});