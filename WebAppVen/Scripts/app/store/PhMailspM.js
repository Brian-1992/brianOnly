Ext.define('WEBAPP.store.PhMailspM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PhMailspM',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'MGROUP', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BD0003/MasterAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});