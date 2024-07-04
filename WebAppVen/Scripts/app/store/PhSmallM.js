Ext.define('WEBAPP.store.PhSmallM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PhSmallM',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'DN', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BC0002/MasterAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});