Ext.define('WEBAPP.store.ME_MDFM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ME_MDFM',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'MDFM', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0041/MasterAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});