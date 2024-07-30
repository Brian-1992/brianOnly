Ext.define('WEBAPP.store.MI_WHMAST_ViewModel', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MI_WHMAST'
    ],
    stores: {
        WH_NO: {
            model: 'WEBAPP.model.MI_WHMAST',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'WH_NO', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                //url: '/api/AA0059/GetWh_no',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }

});