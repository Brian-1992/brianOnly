Ext.define('WEBAPP.store.MiWexpinv', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MiWexpinv',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'WH_NO', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0076/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});