Ext.define('WEBAPP.store.CE0006', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CE0006',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'CHK_NO', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CE0006/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});