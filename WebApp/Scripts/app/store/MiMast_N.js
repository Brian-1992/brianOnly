Ext.define('WEBAPP.store.MiMast_N', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MiMast',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0125/GetAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});